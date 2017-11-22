// Copyright 2017 by PeopleWare n.v..
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;

using NHibernate.Engine;

using PPWCode.Vernacular.NHibernate.I.Implementations;

namespace PPWCode.Vernacular.NHibernate.I.Utilities
{
    [Serializable]
    public abstract class GenericWellKnownInstanceType<T, TId> : ImmutableUserTypeBase
        where T : class
    {
        private readonly Func<T, TId> m_IdGetter;
        private readonly IDictionary<TId, T> m_Repository;

        protected GenericWellKnownInstanceType(IDictionary<TId, T> repository, Func<T, TId> idGetter)
        {
            Contract.Requires(repository != null);
            Contract.Requires(idGetter != null);

            m_Repository = repository;
            m_IdGetter = idGetter;
        }

        public override Type ReturnedType
        {
            get { return typeof(T); }
        }

        public override object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor sessionImplementor, object owner)
        {
            int index0 = rs.GetOrdinal(names[0]);
            if (rs.IsDBNull(index0))
            {
                return null;
            }

            TId key = (TId)rs.GetValue(index0);
            T value;
            m_Repository.TryGetValue(key, out value);
            return value;
        }

        public override void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor sessionImplementor)
        {
            cmd.Parameters[index].Value = value == null ? (object)DBNull.Value : m_IdGetter((T)value);
        }
    }
}