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
using System.Linq;

using JetBrains.Annotations;

using NHibernate;

namespace PPWCode.Vernacular.NHibernate.II
{
    public static class AssociationContracts
    {
        public static bool BiDirOneToMany<TOne, TMany>([CanBeNull] TOne one, [CanBeNull] ISet<TMany> many, [NotNull] Func<TMany, TOne> toOne)
            where TMany : class
            where TOne : class
            => (many != null) && (!NHibernateUtil.IsInitialized(many) || many.All(x => (x != null) && (toOne(x) == one)));

        public static bool BiDirParentToChild<TOne, TMany>([CanBeNull] TOne one, [CanBeNull] ISet<TMany> many, [NotNull] Func<TMany, TOne> toOne)
            where TMany : class
            where TOne : class
            => BiDirOneToMany(one, many, toOne);

        public static bool BiDirManyToOne<TOne, TMany>([CanBeNull] TMany many, [CanBeNull] TOne one, [NotNull] Func<TOne, ISet<TMany>> toMany)
            where TMany : class
            where TOne : class
            => (one == null) || !NHibernateUtil.IsInitialized(one) || toMany(one).Any(x => x == many);

        public static bool BiDirChildToParent<TOne, TMany>([CanBeNull] TMany many, [CanBeNull] TOne one, [NotNull] Func<TOne, ISet<TMany>> toMany)
            where TMany : class
            where TOne : class
            => BiDirManyToOne(many, one, toMany);

        public static bool BiDirManyToMany<TMany, TOtherMany>([CanBeNull] TMany origin, [CanBeNull] ISet<TOtherMany> destinations, [NotNull] Func<TOtherMany, ISet<TMany>> toOrigin)
            where TMany : class
            where TOtherMany : class
            => (destinations != null)
               && (!NHibernateUtil.IsInitialized(destinations)
                   || destinations.All(
                       x =>
                       {
                           if (x != null)
                           {
                               ISet<TMany> origins = toOrigin(x);
                               return (origins == null) || !NHibernateUtil.IsInitialized(origins) || origins.Contains(origin);
                           }

                           return true;
                       }));
    }
}
