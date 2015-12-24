// Copyright 2015 by PeopleWare n.v..
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
            T result = func(CreateCriteria()).UniqueResult<T>();

            return result;
        }

        protected virtual T GetInternal(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            T result = func(CreateQueryOver()).SingleOrDefault();

            return result;
        }

        protected virtual IList<T> FindInternal(Func<ICriteria, ICriteria> func)
        {
            ICriteria criteria = func != null ? func(CreateCriteria()) : CreateCriteria();
            IList<T> result = criteria.List<T>();

            return result;
        }

        protected virtual IList<T> FindInternal(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            IQueryOver<T> queryOver = func != null ? func(CreateQueryOver()) : CreateQueryOver();
            IList<T> result = queryOver.List<T>();

            return result;
        }

        protected virtual PagedList<T> FindPagedInternal(int pageIndex, int pageSize, Func<ICriteria, ICriteria> func)
        {
            ICriteria rowCountCriteria = func != null ? func(CreateCriteria()) : CreateCriteria();
            rowCountCriteria.ClearOrders();
            IFutureValue<int> rowCount =
                rowCountCriteria
                    .SetFirstResult(0)
                    .SetMaxResults(RowSelection.NoValue)
                    .SetProjection(Projections.RowCount())
                    .FutureValue<int>();

            ICriteria pagingCriteria = func != null ? func(CreateCriteria()) : CreateCriteria();
            IList<T> qryResult =
                pagingCriteria
                    .SetFirstResult((pageIndex - 1) * pageSize)
                    .SetMaxResults(pageSize)
                    .Future<T>()
                    .ToList();

            PagedList<T> result = new PagedList<T>(qryResult, pageIndex, pageSize, rowCount.Value);

            return result;
        }

        protected virtual PagedList<T> FindPagedInternal(int pageIndex, int pageSize, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            IQueryOver<T> rowCountQueryOver = func != null ? func(CreateQueryOver()) : CreateQueryOver();
            IFutureValue<int> rowCount =
                rowCountQueryOver
                    .ToRowCountQuery()
                    .FutureValue<int>();

            IQueryOver<T> pagingQueryOver = func != null ? func(CreateQueryOver()) : CreateQueryOver();
            IList<T> qryResult =
                pagingQueryOver
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Future<T>()
                    .ToList();

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
                // Check if entity exists
                T fetchedEntity = Session.Get<T>(entity.Id);
                if (fetchedEntity != null)
                {
                    // Handle stale objects
                    Session.Merge(entity);
                    // finally delete none-transient not stale existing entity
                    Session.Delete(fetchedEntity);
                }
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