using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;

using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace PPWCode.Vernacular.nHibernate.I.Utilities
{
    [Serializable]
    public abstract class GenericWellKnownInstanceType<T, TId> : IUserType
        where T : class
    {
        private readonly Func<T, TId> m_IdGetter;
        private readonly IDictionary<TId, T> m_Repository;

        protected GenericWellKnownInstanceType(IDictionary<TId, T> repository, Func<T, TId> idGetter)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            if (idGetter == null)
            {
                throw new ArgumentNullException("idGetter");
            }
            Contract.EndContractBlock();

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