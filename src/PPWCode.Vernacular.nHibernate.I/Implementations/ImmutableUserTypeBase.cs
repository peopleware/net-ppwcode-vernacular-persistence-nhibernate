using System;
using System.Data;

using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace PPWCode.Vernacular.nHibernate.I.Implementations
{
    public abstract class ImmutableUserTypeBase : IUserType
    {
        #region Implementation of IUserType

        public new bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }

            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }
            return x.GetHashCode();
        }

        public abstract object NullSafeGet(IDataReader rs, string[] names, object owner);
        public abstract void NullSafeSet(IDbCommand cmd, object value, int index);
        public abstract object DeepCopy(object value);

        public object Replace(object original, object target, object owner)
        {
            // since object is immutable, return original
            return original;
        }

        public object Assemble(object cached, object owner)
        {
            //Used for cashing, as our object is immutable we can just return it as is
            return cached;
        }

        public object Disassemble(object value)
        {
            //Used for caching, as our object is immutable we can just return it as is
            return value;
        }

        public abstract SqlType[] SqlTypes { get; }
        public abstract Type ReturnedType { get; }

        public bool IsMutable
        {
            get { return false; }
        }

        #endregion
    }
}