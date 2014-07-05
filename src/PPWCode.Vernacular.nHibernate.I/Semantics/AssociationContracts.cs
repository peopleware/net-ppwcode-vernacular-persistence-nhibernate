using System;
using System.Security.Cryptography.X509Certificates;

using Iesi.Collections.Generic;

using NHibernate;

using System.Linq;

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