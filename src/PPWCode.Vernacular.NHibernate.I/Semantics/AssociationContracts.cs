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
using System.Diagnostics.Contracts;
using System.Linq;

using NHibernate;

namespace PPWCode.Vernacular.NHibernate.I.Semantics
{
    public static class AssociationContracts
    {
        [Pure]
        public static bool BiDirOneToMany<O, M>(O one, ISet<M> many, Func<M, O> toOne)
            where M : class
            where O : class
        {
            return many != null && (!NHibernateUtil.IsInitialized(many) || many.All(x => x != null && toOne(x) == one));
        }

        [Pure]
        public static bool BiDirParentToChild<O, M>(O one, ISet<M> many, Func<M, O> toOne)
            where M : class
            where O : class
        {
            return BiDirOneToMany(one, many, toOne);
        }

        [Pure]
        public static bool BiDirManyToOne<O, M>(M many, O one, Func<O, ISet<M>> toMany)
            where M : class
            where O : class
        {
            return one == null || !NHibernateUtil.IsInitialized(one) || toMany(one).Any(x => x == many);
        }

        [Pure]
        public static bool BiDirChildToParent<O, M>(M many, O one, Func<O, ISet<M>> toMany)
            where M : class
            where O : class
        {
            return BiDirManyToOne(many, one, toMany);
        }
    }
}