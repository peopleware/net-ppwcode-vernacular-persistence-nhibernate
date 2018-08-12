// Copyright 2017 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

using NHibernate;
using NHibernate.Type;

using PPWCode.Vernacular.Persistence.III;

namespace PPWCode.Vernacular.NHibernate.II
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Castle Windsor usage")]
    [Serializable]
    public class AuditInterceptor<T> : EmptyInterceptor
        where T : IEquatable<T>
    {
        private const string CreatedAtPropertyName = "CreatedAt";
        private const string CreatedByPropertyName = "CreatedBy";
        private const string LastModifiedAtPropertyName = "LastModifiedAt";
        private const string LastModifiedByPropertyName = "LastModifiedBy";

        public AuditInterceptor(
            [NotNull] IIdentityProvider identityProvider,
            [NotNull] ITimeProvider timeProvider,
            bool useUtc)
        {
            IdentityProvider = identityProvider;
            TimeProvider = timeProvider;
            UseUtc = useUtc;
        }

        [NotNull]
        public IIdentityProvider IdentityProvider { get; }

        [NotNull]
        public ITimeProvider TimeProvider { get; }

        public bool UseUtc { get; }

        [NotNull]
        protected ConcurrentDictionary<Property, int> IndexCache { get; } = new ConcurrentDictionary<Property, int>();

        protected virtual bool CanAudit([NotNull] object entity, [NotNull] object id)
            => true;

        protected virtual void Set(
            [NotNull] Type entityType,
            [NotNull] string[] propertyNames,
            [NotNull] object[] state,
            [NotNull] string propertyName,
            [NotNull] object value)
        {
            int index = IndexCache.GetOrAdd(new Property(entityType, propertyName), k => Array.IndexOf(propertyNames, propertyName));
            if (index >= 0)
            {
                state[index] = value;
            }
        }

        protected virtual bool SetAuditInfo(
            [CanBeNull] object entity,
            [NotNull] object[] currentState,
            [NotNull] string[] propertyNames,
            bool onSave)
        {
            if (!(entity is IPersistentObject<T> persistentObject))
            {
                return false;
            }

            IInsertAuditable insertAuditable = entity as IInsertAuditable;
            IUpdateAuditable updateAuditable = entity as IUpdateAuditable;
            if ((insertAuditable == null) && (updateAuditable == null))
            {
                return false;
            }

            DateTime time = UseUtc ? TimeProvider.UtcNow : TimeProvider.Now;
            string identityName = IdentityProvider.IdentityName;
            if (identityName == null)
            {
                throw new InvalidOperationException("Unknown IdentityName");
            }

            Type entityType = entity.GetType();

            if ((insertAuditable != null) && (onSave || persistentObject.IsTransient))
            {
                IInsertAuditableProperties insertAuditableProperties = entity as IInsertAuditableProperties;
                string createdAtPropertyName =
                    insertAuditableProperties != null
                        ? insertAuditableProperties.CreatedAtPropertyName
                        : CreatedAtPropertyName;
                string createdByPropertyName =
                    insertAuditableProperties != null
                        ? insertAuditableProperties.CreatedByPropertyName
                        : CreatedByPropertyName;

                Set(entityType, propertyNames, currentState, createdAtPropertyName, time);
                Set(entityType, propertyNames, currentState, createdByPropertyName, identityName);

                insertAuditable.CreatedAt = time;
                insertAuditable.CreatedBy = identityName;
            }
            else if (updateAuditable != null)
            {
                IUpdateAuditableProperties updateAuditableProperties = entity as IUpdateAuditableProperties;
                string lastModifiedAtPropertyName =
                    updateAuditableProperties != null
                        ? updateAuditableProperties.LastModifiedAtPropertyName
                        : LastModifiedAtPropertyName;
                string lastModifiedByPropertyName =
                    updateAuditableProperties != null
                        ? updateAuditableProperties.LastModifiedByPropertyName
                        : LastModifiedByPropertyName;

                Set(entityType, propertyNames, currentState, lastModifiedAtPropertyName, time);
                Set(entityType, propertyNames, currentState, lastModifiedByPropertyName, identityName);

                updateAuditable.LastModifiedAt = time;
                updateAuditable.LastModifiedBy = identityName;
            }

            return true;
        }

        /// <summary>
        ///     Called when an object is detected to be dirty, during a flush.
        /// </summary>
        /// <param name="entity">The given entity.</param>
        /// <param name="id">The id of the given entity.</param>
        /// <param name="currentState">The current state of the entity.</param>
        /// <param name="previousState">The previous state of the entity.</param>
        /// <param name="propertyNames">The property names.</param>
        /// <param name="types">The types.</param>
        /// <remarks>
        ///     The interceptor may modify the detected <c>currentState</c>, which will be propagated to
        ///     both the database and the persistent object. Note that all flushes end in an actual
        ///     synchronization with the database, in which as the new <c>currentState</c> will be propagated
        ///     to the object, but not necessarily (immediately) to the database. It is strongly recommended
        ///     that the interceptor <b>not</b> modify the <c>previousState</c>.
        /// </remarks>
        /// <returns>
        ///     A boolean indicating whether the user modified the  <paramref name="currentState" /> in any way.
        /// </returns>
        public override bool OnFlushDirty(
            [NotNull] object entity,
            [NotNull] object id,
            [NotNull] object[] currentState,
            [NotNull] object[] previousState,
            [NotNull] string[] propertyNames,
            [NotNull] IType[] types)
            => CanAudit(entity, id) && SetAuditInfo(entity, currentState, propertyNames, false);

        /// <summary>
        ///     Called before an object is saved.
        /// </summary>
        /// <param name="entity">The given entity.</param>
        /// <param name="id">The id of the given entity.</param>
        /// <param name="state">The state of the entity.</param>
        /// <param name="propertyNames">The property names.</param>
        /// <param name="types">The types.</param>
        /// <remarks>
        ///     The interceptor may modify the <c>state</c>, which will be used for the SQL <c>INSERT</c>
        ///     and propagated to the persistent object.
        /// </remarks>
        /// <returns>
        ///     A boolean indicating whether the user modified the <c>state</c> in any way.
        /// </returns>
        public override bool OnSave(
            [NotNull] object entity,
            [NotNull] object id,
            [NotNull] object[] state,
            [NotNull] string[] propertyNames,
            [NotNull] IType[] types)
            => CanAudit(entity, id) && SetAuditInfo(entity, state, propertyNames, true);

        protected struct Property : IEquatable<Property>
        {
            private readonly Type _entityType;
            private readonly string _propertyName;

            public static bool operator ==(Property left, Property right)
                => left.Equals(right);

            public static bool operator !=(Property left, Property right)
                => !left.Equals(right);

            public Property(Type entityType, string propertyName)
            {
                _entityType = entityType;
                _propertyName = propertyName;
            }

            /// <summary>
            ///     Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <returns>
            ///     A boolean indicating whether the current object is equal to the <paramref name="other" /> parameter.
            /// </returns>
            /// <param name="other">An object to compare with this object.</param>
            public bool Equals(Property other)
                => ReferenceEquals(_entityType, other._entityType)
                   && string.Equals(_propertyName, other._propertyName);

            /// <summary>
            ///     Indicates whether this instance and a specified object are equal.
            /// </summary>
            /// <returns>
            ///     A boolean indicating whether <paramref name="obj" /> and this instance are the same type and represent the same
            ///     value.
            /// </returns>
            /// <param name="obj">Another object to compare to. </param>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is Property property && Equals(property);
            }

            /// <summary>
            ///     Returns the hash code for this instance.
            /// </summary>
            /// <returns>
            ///     A 32-bit signed integer that is the hash code for this instance.
            /// </returns>
            public override int GetHashCode()
            {
                unchecked
                {
                    return (_entityType.GetHashCode() * 397) ^ _propertyName.GetHashCode();
                }
            }
        }
    }
}
