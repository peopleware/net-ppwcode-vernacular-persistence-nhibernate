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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using NHibernate;

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Implementations
{
    public abstract class QueryOverRepository<T, TId>
        : Repository<T, TId>,
          IQueryOverRepository<T, TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        protected QueryOverRepository(ISessionProvider sessionProvider)
            : base(sessionProvider)
        {
        }

        public virtual T Get(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute(nameof(Get), () => GetInternal(alias, func));
        }

        public virtual T Get(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute(nameof(Get), () => GetInternal(func));
        }

        public virtual T GetAtIndex(Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int index)
        {
            return Execute(nameof(GetAtIndex), () => GetAtIndexInternal(func, index));
        }

        public virtual T GetAtIndex(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int index)
        {
            return Execute(nameof(GetAtIndex), () => GetAtIndexInternal(alias, func, index));
        }

        public virtual IList<T> Find(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute(nameof(Find), () => FindInternal(func));
        }

        public virtual IList<T> Find(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute(nameof(Find), () => FindInternal(alias, func));
        }

        public virtual IPagedList<T> FindPaged(int pageIndex, int pageSize, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute(nameof(FindPaged), () => FindPagedInternal(pageIndex, pageSize, func));
        }

        public virtual IPagedList<T> FindPaged(int pageIndex, int pageSize, Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute(nameof(FindPaged), () => FindPagedInternal(pageIndex, pageSize, alias, func));
        }

        public virtual IList<T> Find(Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int? skip, int? count)
        {
            return Execute(nameof(Find), () => FindInternal(func, skip, count));
        }

        public virtual IList<T> Find(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int? skip, int? count)
        {
            return Execute(nameof(Find), () => FindInternal(alias, func, skip, count));
        }

        protected virtual T GetInternal(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            T result = func(CreateQueryOver()).SingleOrDefault<T>();

            return result;
        }

        protected virtual T GetInternal(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            T result = func(CreateQueryOver(alias)).SingleOrDefault<T>();

            return result;
        }

        protected virtual T GetAtIndexInternal(Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int index)
        {
            T result = func(CreateQueryOver()).Skip(index).Take(1).SingleOrDefault<T>();

            return result;
        }

        protected virtual T GetAtIndexInternal(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int index)
        {
            T result = func(CreateQueryOver(alias)).Skip(index).Take(1).SingleOrDefault<T>();

            return result;
        }

        protected override IList<T> FindAllInternal()
        {
            IQueryOver<T> queryOver = CreateQueryOver();
            IList<T> result = queryOver.List<T>();

            return result;
        }

        protected virtual IList<T> FindInternal(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            IQueryOver<T> queryOver = func != null ? func(CreateQueryOver()) : CreateQueryOver();
            IList<T> result = queryOver.List<T>();

            return result;
        }

        protected virtual IList<T> FindInternal(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            IQueryOver<T> queryOver = func != null ? func(CreateQueryOver(alias)) : CreateQueryOver(alias);
            IList<T> result = queryOver.List<T>();

            return result;
        }

        protected virtual IList<T> FindInternal(Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int? skip, int? count)
        {
            IQueryOver<T> queryOver = func != null ? func(CreateQueryOver()) : CreateQueryOver();

            if (skip.HasValue)
            {
                queryOver = queryOver.Skip(skip.Value);
            }

            if (count.HasValue)
            {
                queryOver = queryOver.Take(count.Value);
            }

            IList<T> result = queryOver.List<T>();

            return result;
        }

        protected virtual IList<T> FindInternal(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int? skip, int? count)
        {
            IQueryOver<T> queryOver = func != null ? func(CreateQueryOver(alias)) : CreateQueryOver(alias);

            if (skip.HasValue)
            {
                queryOver = queryOver.Skip(skip.Value);
            }

            if (count.HasValue)
            {
                queryOver = queryOver.Take(count.Value);
            }

            IList<T> result = queryOver.List<T>();

            return result;
        }

        protected virtual PagedList<T> FindPagedInternal(int pageIndex, int pageSize, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return FindPagedInternal(pageIndex, pageSize, () => func != null ? func(CreateQueryOver()) : CreateQueryOver());
        }

        protected virtual PagedList<T> FindPagedInternal(int pageIndex, int pageSize, Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return FindPagedInternal(pageIndex, pageSize, () => func != null ? func(CreateQueryOver(alias)) : CreateQueryOver(alias));
        }

        protected virtual PagedList<T> FindPagedInternal(int pageIndex, int pageSize, Func<IQueryOver<T, T>> queryFactory)
            => FindPagedInternal<T, T>(pageIndex, pageSize, queryFactory);

        protected virtual PagedList<R> FindPagedInternal<R>(int pageIndex, int pageSize, Func<IQueryOver<T, T>> queryFactory)
            => FindPagedInternal<R, T>(pageIndex, pageSize, queryFactory);

        protected virtual PagedList<R> FindPagedInternal<R, X>(int pageIndex, int pageSize, Func<IQueryOver<T, X>> queryFactory)
        {
            IQueryOver<T, X> rowCountQueryOver = queryFactory();
            IFutureValue<int> rowCount =
                rowCountQueryOver
                    .ToRowCountQuery()
                    .FutureValue<int>();

            IQueryOver<T, X> pagingQueryOver = queryFactory();
            IList<R> qryResult =
                pagingQueryOver
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Future<R>()
                    .ToList();

            PagedList<R> result = new PagedList<R>(qryResult, pageIndex, pageSize, rowCount.Value);

            return result;
        }

        protected virtual IQueryOver<T, T> CreateQueryOver()
        {
            T rootAlias = null;
            return CreateQueryOver(() => rootAlias);
        }

        protected virtual IQueryOver<T, T> CreateQueryOver(Expression<Func<T>> alias)
            => Session.QueryOver(alias);
    }
}
