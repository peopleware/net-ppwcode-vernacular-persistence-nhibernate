// Copyright 2014 by PeopleWare n.v..
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
using System.Data;
using System.Diagnostics.Contracts;

using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace PPWCode.Vernacular.NHibernate.I.Utilities
{
    [Serializable]
    public abstract class GenericWellKnownInstanceType<T, TId> : IUserType
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

        public Type ReturnedType
        {
            get { return typeof(T); }
        }

        public bool IsMutable
        {
            get { return false; }
        }

        public new bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(null, x) || ReferenceEquals(null, y))
            {
                return false;
            }

            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            return (x == null) ? 0 : x.GetHashCode();
        }

        public object NullSafeGet(IDataReader rs, string[] names, object owner)
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

        public void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            if (value == null)
            {
                ((IDbDataParameter)cmd.Parameters[index]).Value = DBNull.Value;
            }
            else
            {
                ((IDbDataParameter)cmd.Parameters[index]).Value = m_IdGetter((T)value);
            }
        }

        public object DeepCopy(object value)
        {
            return value;
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public abstract SqlType[] SqlTypes { get; }
    }
}