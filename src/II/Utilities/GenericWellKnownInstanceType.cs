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
using System.Collections.Generic;
using System.Data.Common;

using JetBrains.Annotations;

using NHibernate.Engine;

namespace PPWCode.Vernacular.NHibernate.II
{
    /// <inheritdoc />
    [Serializable]
    public abstract class GenericWellKnownInstanceType<T, TId> : ImmutableUserTypeBase
        where T : class
    {
        [NotNull]
        private readonly Func<T, TId> _idGetter;

        [NotNull]
        private readonly IDictionary<TId, T> _repository;

        protected GenericWellKnownInstanceType(
            [NotNull] IDictionary<TId, T> repository,
            [NotNull] Func<T, TId> idGetter)
        {
            _repository = repository;
            _idGetter = idGetter;
        }

        [NotNull]
        public override Type ReturnedType
            => typeof(T);

        [CanBeNull]
        public override object NullSafeGet(
            [NotNull] DbDataReader rs,
            [NotNull] string[] names,
            [NotNull] ISessionImplementor sessionImplementor,
            [NotNull] object owner)
        {
            int index0 = rs.GetOrdinal(names[0]);
            if (rs.IsDBNull(index0))
            {
                return null;
            }

            TId key = (TId)rs.GetValue(index0);
            _repository.TryGetValue(key, out T value);
            return value;
        }

        public override void NullSafeSet(
            [NotNull] DbCommand cmd,
            [CanBeNull] object value,
            int index,
            [NotNull] ISessionImplementor sessionImplementor)
        {
            cmd.Parameters[index].Value = value == null ? (object)DBNull.Value : _idGetter((T)value);
        }
    }
}
