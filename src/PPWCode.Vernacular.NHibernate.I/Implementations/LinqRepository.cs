// Copyright 2017 by PeopleWare n.v..
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
using NHibernate.Linq;

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Implementations
{
    public abstract class LinqRepository<T, TId>
        : Repository<T, TId>,
          ILinqRepository<T, TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        protected LinqRepository(ISession session)
            : base(session)
        {
        }

        public virtual T Get(Func<IQueryable<T>, IQueryable<T>> func)
        {
            return Execute("Get", () => GetInternal(func));
        }

        public virtual T GetAtIndex(Func<IQueryable<T>, IQueryable<T>> func, int index)
        {
            return Execute("GetAtIndex", () => GetAtIndexInternal(func, index));
        }

        public virtual IList<T> Find(Func<IQueryable<T>, IQueryable<T>> func)
        {
            return Execute("Find", () => FindInternal(func));
        }

        public virtual IList<R> Find<R>(Func<IQueryable<T>, IQueryable<R>> func)
        {
            return Execute("Find", () => FindInternal(func));
        }

        public virtual IList<T> Find(Func<IQueryable<T>, IQueryable<T>> func, int? skip, int? count)
        {
            return Execute("Find", () => FindInternal(func, skip, count));
        }

        public virtual IList<R> Find<R>(Func<IQueryable<T>, IQueryable<R>> func, int? skip, int? count)
        {
            return Execute("Find", () => FindInternal(func, skip, count));
        }

        public virtual IPagedList<T> FindPaged(int pageIndex, int pageSize, Func<IQueryable<T>, IQueryable<T>> func)
        {
            return Execute("FindPaged", () => FindPagedInternal(pageIndex, pageSize, func));
        }

        public virtual IPagedList<R> FindPaged<R>(int pageIndex, int pageSize, Func<IQueryable<T>, IQueryable<R>> func)
        {
            return Execute("FindPaged", () => FindPagedInternal(pageIndex, pageSize, func));
        }

        protected virtual T GetInternal(Func<IQueryable<T>, IQueryable<T>> func)
        {
            if (func == null)
            {
                return default(T);
            }

            T result = func(CreateQueryable()).SingleOrDefault();

            return result;
        }

        protected virtual T GetAtIndexInternal(Func<IQueryable<T>, IQueryable<T>> func, int index)
        {
            if (func == null)
            {
                return default(T);
            }

            T result = func(CreateQueryable()).Skip(index).Take(1).SingleOrDefault();

            return result;
        }

        protected override IList<T> FindAllInternal()
        {
            return FindInternal(null);
        }

        protected virtual IList<T> FindInternal(Func<IQueryable<T>, IQueryable<T>> func)
        {
            return FindInternal(func, null, null);
        }

        protected virtual IList<R> FindInternal<R>(Func<IQueryable<T>, IQueryable<R>> func)
        {
            return FindInternal(func, null, null);
        }

        protected virtual IList<T> FindInternal(Func<IQueryable<T>, IQueryable<T>> func, int? skip, int? count)
        {
            IList<T> result = FindInternal(() => func != null ? func(CreateQueryable()) : CreateQueryable(), skip, count);

            return result;
        }

        protected virtual IList<R> FindInternal<R>(Func<IQueryable<T>, IQueryable<R>> func, int? skip, int? count)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            IList<R> result = FindInternal(() => func(CreateQueryable()), skip, count);

            return result;
        }

        protected virtual IList<R> FindInternal<R>(Func<IQueryable<R>> queryFactory, int? skip, int? count)
        {
            IQueryable<R> query = queryFactory();

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (count.HasValue)
            {
                query = query.Take(count.Value);
            }

            IList<R> result = query.ToList();

            return result;
        }

        protected virtual PagedList<R> FindPagedInternal<R>(int pageIndex, int pageSize, Func<IQueryable<T>, IQueryable<R>> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            return FindPagedInternal(pageIndex, pageSize, () => func(CreateQueryable()));
        }

        protected virtual PagedList<T> FindPagedInternal(int pageIndex, int pageSize, Func<IQueryable<T>, IQueryable<T>> func)
        {
            return FindPagedInternal(pageIndex, pageSize, () => func != null ? func(CreateQueryable()) : CreateQueryable());
        }

        protected virtual PagedList<R> FindPagedInternal<R>(int pageIndex, int pageSize, Func<IQueryable<R>> queryFactory)
        {
            IQueryable<R> rowCountQueryOver = queryFactory();
            IFutureValue<int> rowCount =
                rowCountQueryOver
                    .ToFutureValue(x => x.Count());

            IQueryable<R> pagingQueryOver = queryFactory();
            IFutureEnumerable<R> qryResult =
                pagingQueryOver
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToFuture();

            PagedList<R> result = new PagedList<R>(qryResult, pageIndex, pageSize, rowCount.Value);

            return result;
        }

        protected virtual IQueryable<T> CreateQueryable()
        {
            return Session.Query<T>();
        }
    }
}
