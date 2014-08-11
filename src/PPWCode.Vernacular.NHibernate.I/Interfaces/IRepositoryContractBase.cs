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

using NHibernate;

using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Interfaces
{
    // ReSharper disable once InconsistentNaming
    public abstract class IRepositoryContractBase<T, TId> : IRepository<T, TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        public abstract T GetById(TId id);

        public abstract T Get(Func<ICriteria, ICriteria> func);

        public abstract T Get(Func<IQueryOver<T, T>, IQueryOver<T, T>> func);

        public abstract IList<T> Find(Func<ICriteria, ICriteria> func);

        public abstract IList<T> Find(Func<IQueryOver<T, T>, IQueryOver<T, T>> func);

        public abstract IPagedList<T> FindPaged(int pageIndex, int pageSize, Func<ICriteria, ICriteria> func);

        public abstract IPagedList<T> FindPaged(int pageIndex, int pageSize, Func<IQueryOver<T, T>, IQueryOver<T, T>> func);

        public abstract T Save(T entity);

        public abstract void Delete(T entity);
    }
}