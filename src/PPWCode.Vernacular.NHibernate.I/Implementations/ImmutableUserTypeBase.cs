// Copyright 2017-2018 by PeopleWare n.v..
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
using System.Data.Common;

using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace PPWCode.Vernacular.NHibernate.I.Implementations
{
    public abstract class ImmutableUserTypeBase : IUserType
    {
        public new bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if ((x == null) || (y == null))
            {
                return false;
            }

            return x.Equals(y);
        }

        public int GetHashCode(object x)
            => x == null ? 0 : x.GetHashCode();

        public abstract object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor sessionImplementor, object owner);

        public abstract void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor sessionImplementor);

        public object DeepCopy(object value)
            => value;

        public object Replace(object original, object target, object owner)
            => original;

        public object Assemble(object cached, object owner)
            => cached;

        public object Disassemble(object value)
            => value;

        public abstract SqlType[] SqlTypes { get; }

        public abstract Type ReturnedType { get; }

        public bool IsMutable
            => false;
    }
}
