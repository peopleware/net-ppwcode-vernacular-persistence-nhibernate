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
using System.Linq;

using Iesi.Collections.Generic;

using NHibernate;

namespace PPWCode.Vernacular.nHibernate.I.Semantics
{
    public static class AssociationContracts
    {
        public static bool BiDirOneToMany<O, M>(O one, ISet<M> many, Func<M, O> toOne)
            where M : class
            where O : class
        {
            return many != null
                   && NHibernateUtil.IsInitialized(many)
                   && many.All(x => x != null && toOne(x) == one);
        }

        public static bool BiDirManyToOne<O, M>(M many, O one, Func<O, ISet<M>> toMany)
            where M : class
            where O : class
        {
            return one != null
                   && NHibernateUtil.IsInitialized(one)
                   && toMany(one).Any(x => x == many);
        }
    }
}