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

using System.Collections.Generic;

using JetBrains.Annotations;

using NHibernate;
using NHibernate.Engine;
using NHibernate.Persister.Entity;
using NHibernate.Proxy;
using NHibernate.Type;

namespace PPWCode.Vernacular.NHibernate.III.Test
{
    public class DirtyCheckingInterceptor
        : EmptyInterceptor
    {
        [CanBeNull]
        private ISession _session;

        public DirtyCheckingInterceptor([NotNull] IList<string> dirtyProps)
        {
            DirtyProps = dirtyProps;
        }

        [NotNull]
        protected IList<string> DirtyProps { get; }

        public override void SetSession(ISession session)
        {
            _session = session;
        }

        public override bool OnFlushDirty(
            [NotNull] object entity,
            [NotNull] object id,
            [NotNull] object[] currentState,
            [NotNull] object[] previousState,
            [NotNull] string[] propertyNames,
            [NotNull] IType[] types)
        {
            string msg = $"Flush Dirty {entity.GetType().FullName}";
            DirtyProps.Add(msg);
            ListDirtyProperties(entity);
            return false;
        }

        public override bool OnSave(
            [NotNull] object entity,
            [NotNull] object id,
            [NotNull] object[] state,
            [NotNull] string[] propertyNames,
            [NotNull] IType[] types)
        {
            string msg = $"Save {entity.GetType().FullName}";
            DirtyProps.Add(msg);
            return false;
        }

        public override void OnDelete(
            [NotNull] object entity,
            [NotNull] object id,
            [NotNull] object[] state,
            [NotNull] string[] propertyNames,
            [NotNull] IType[] types)
        {
            string msg = $"Delete {entity.GetType().FullName}";
            DirtyProps.Add(msg);
        }

        private void ListDirtyProperties([NotNull] object entity)
        {
            if (_session != null)
            {
                string className = NHibernateProxyHelper.GuessClass(entity).FullName;
                ISessionImplementor sessionImpl = _session.GetSessionImplementation();
                IEntityPersister persister = sessionImpl.Factory.GetEntityPersister(className);
                EntityEntry oldEntry = sessionImpl.PersistenceContext.GetEntry(entity);

                if (oldEntry == null)
                {
                    object obj =
                        entity is INHibernateProxy proxy
                            ? sessionImpl.PersistenceContext.Unproxy(proxy)
                            : entity;
                    oldEntry = sessionImpl.PersistenceContext.GetEntry(obj);
                }

                object[] oldState = oldEntry.LoadedState;
                object[] currentState = persister.GetPropertyValues(entity);
                int[] dirtyProperties = persister.FindDirty(currentState, oldState, entity, sessionImpl);

                foreach (int index in dirtyProperties)
                {
                    string msg = $"Dirty property {className}.{persister.PropertyNames[index]} was {oldState[index] ?? "null"}, is {currentState[index] ?? "null"}.";
                    DirtyProps.Add(msg);
                }
            }
        }
    }
}
