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
using System.Linq;

using NHibernate;
using NHibernate.Criterion;
using NHibernate.Engine;

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Implementations
{
    public abstract class Repository<T, TId>
        : RepositoryBase<T, TId>,
          IRepository<T, TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        protected Repository(ISession session)
            : base(session)
        {
        }

        public virtual T GetById(TId id)
        {
            return Execute("GetById", () => GetByIdInternal(id));
        }

        public T Get(Func<ICriteria, ICriteria> func)
        {
            return Execute("Get", () => GetInternal(func));
        }

        public T Get(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute("Get", () => GetInternal(func));
        }

        public virtual IList<T> Find(Func<ICriteria, ICriteria> func)
        {
            return Execute("Find", () => FindInternal(func));
        }

        public IList<T> Find(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute("Find", () => FindInternal(func));
        }

        public virtual IPagedList<T> FindPaged(int pageIndex, int pageSize, Func<ICriteria, ICriteria> func)
        {
            return Execute("FindPaged", () => FindPagedInternal(pageIndex, pageSize, func));
        }

        public IPagedList<T> FindPaged(int pageIndex, int pageSize, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute("FindPaged", () => FindPagedInternal(pageIndex, pageSize, func));
        }

        public virtual T Merge(T entity)
        {
            return Execute("Merge", () => MergeInternal(entity));
        }

        public virtual void Delete(T entity)
        {
            Execute("Delete", () => DeleteInternal(entity));
        }

        protected virtual T GetByIdInternal(TId id)
        {
            T result = Session.Get<T>(id);

            return result;
        }

        protected virtual T GetInternal(Func<ICriteria, ICriteria> func)
        {
            ICriteria criteria = CreateCriteria();
            ICriteria qry = func(criteria);
            T result = qry.UniqueResult<T>();

            return result;
        }

        protected virtual T GetInternal(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            IQueryOver<T, T> queryOver = CreateQueryOver();
            IQueryOver<T> qry = func(queryOver);
            T result = qry.SingleOrDefault();

            return result;
        }

        protected virtual IList<T> FindInternal(Func<ICriteria, ICriteria> func)
        {
            ICriteria criteria = CreateCriteria();
            ICriteria qry = func(criteria);
            IList<T> result = qry.List<T>();

            return result;
        }

        protected virtual IList<T> FindInternal(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            IQueryOver<T, T> queryOver = CreateQueryOver();
            IQueryOver<T> qry = func(queryOver);
            IList<T> result = qry.List<T>();

            return result;
        }

        protected virtual PagedList<T> FindPagedInternal(int pageIndex, int pageSize, Func<ICriteria, ICriteria> func)
        {
            ICriteria qryRowCount = func(CreateCriteria());
            qryRowCount.ClearOrders();

            IFutureValue<int> rowCount = qryRowCount
                .SetFirstResult(0)
                .SetMaxResults(RowSelection.NoValue)
                .SetProjection(Projections.RowCount())
                .FutureValue<int>();

            IList<T> qryResult = func(CreateCriteria())
                .SetFirstResult((pageIndex - 1) * pageSize)
                .SetMaxResults(pageSize)
                .Future<T>()
                .ToList<T>();

            PagedList<T> result = new PagedList<T>(qryResult, pageIndex, pageSize, rowCount.Value);

            return result;
        }

        protected virtual PagedList<T> FindPagedInternal(int pageIndex, int pageSize, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            IQueryOver<T> queryOver = func(CreateQueryOver());

            IFutureValue<int> rowCount = queryOver
                .ToRowCountQuery()
                .FutureValue<int>();

            IList<T> qryResult = queryOver
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Future<T>()
                .ToList<T>();

            PagedList<T> result = new PagedList<T>(qryResult, pageIndex, pageSize, rowCount.Value);

            return result;
        }

        protected virtual T MergeInternal(T entity)
        {
            T result = Session.Merge(entity);

            return result;
        }

        protected virtual void DeleteInternal(T entity)
        {
            if (!entity.IsTransient)
            {
                Session.Delete(entity);
            }
        }

        protected virtual ICriteria CreateCriteria()
        {
            return Session.CreateCriteria<T>();
        }

        protected virtual IQueryOver<T, T> CreateQueryOver()
        {
            return Session.QueryOver<T>();
        }
    }
}