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
using NHibernate.Linq;

using PPWCode.Vernacular.NHibernate.II.Providers;
using PPWCode.Vernacular.Persistence.III;

namespace PPWCode.Vernacular.NHibernate.II
{
    public abstract class LinqRepository<TRoot, TId>
        : Repository<TRoot, TId>,
          ILinqRepository<TRoot, TId>
        where TRoot : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        protected LinqRepository(ISessionProvider sessionProvider)
            : base(sessionProvider)
        {
        }

        public virtual TRoot Get(Func<IQueryable<TRoot>, IQueryable<TRoot>> func)
            => Execute(nameof(Get), () => GetInternal(func));

        public virtual TRoot GetAtIndex(Func<IQueryable<TRoot>, IQueryable<TRoot>> func, int index)
            => Execute(nameof(GetAtIndex), () => GetAtIndexInternal(func, index));

        public virtual IList<TRoot> Find(Func<IQueryable<TRoot>, IQueryable<TRoot>> func)
            => Execute(nameof(Find), () => FindInternal(func)) ?? new List<TRoot>();

        public virtual IPagedList<TRoot> FindPaged(int pageIndex, int pageSize, Func<IQueryable<TRoot>, IQueryable<TRoot>> func)
            => Execute(nameof(FindPaged), () => FindPagedInternal(pageIndex, pageSize, func)) ?? new PagedList<TRoot>(Enumerable.Empty<TRoot>(), pageIndex, pageSize, 0);

        public virtual IList<TSubType> Find<TSubType>(Func<IQueryable<TRoot>, IQueryable<TSubType>> func)
            => Execute(nameof(Find), () => FindInternal(func));

        public virtual IList<TRoot> Find(Func<IQueryable<TRoot>, IQueryable<TRoot>> func, int? skip, int? count)
            => Execute(nameof(Find), () => FindInternal(func, skip, count));

        public virtual IList<TSubType> Find<TSubType>(Func<IQueryable<TRoot>, IQueryable<TSubType>> func, int? skip, int? count)
            => Execute(nameof(Find), () => FindInternal(func, skip, count));

        public virtual IPagedList<TSubType> FindPaged<TSubType>(int pageIndex, int pageSize, Func<IQueryable<TRoot>, IQueryable<TSubType>> func)
            => Execute(nameof(FindPaged), () => FindPagedInternal(pageIndex, pageSize, func));

        [CanBeNull]
        protected virtual TRoot GetInternal([NotNull] Func<IQueryable<TRoot>, IQueryable<TRoot>> func)
        {
            try
            {
                return func.Invoke(CreateQueryable()).SingleOrDefault();
            }
            catch (EmptyResultException)
            {
                return null;
            }
        }

        [CanBeNull]
        protected virtual TRoot GetAtIndexInternal([NotNull] Func<IQueryable<TRoot>, IQueryable<TRoot>> func, int index)
        {
            try
            {
                return func.Invoke(CreateQueryable()).Skip(index).Take(1).SingleOrDefault();
            }
            catch (EmptyResultException)
            {
                return null;
            }
        }

        protected override IList<TRoot> FindAllInternal()
            => FindInternal(null);

        [NotNull]
        protected virtual IList<TRoot> FindInternal([CanBeNull] Func<IQueryable<TRoot>, IQueryable<TRoot>> func)
            => FindInternal(func, null, null);

        [NotNull]
        protected virtual IList<TSubType> FindInternal<TSubType>([NotNull] Func<IQueryable<TRoot>, IQueryable<TSubType>> func)
            => FindInternal(func, null, null);

        [NotNull]
        protected virtual IList<TRoot> FindInternal([CanBeNull] Func<IQueryable<TRoot>, IQueryable<TRoot>> func, int? skip, int? count)
            => FindInternal(() => func != null ? func(CreateQueryable()) : CreateQueryable(), skip, count);

        [NotNull]
        protected virtual IList<TSubType> FindInternal<TSubType>([NotNull] Func<IQueryable<TRoot>, IQueryable<TSubType>> func, int? skip, int? count)
            => FindInternal(() => func(CreateQueryable()), skip, count);

        [NotNull]
        protected virtual IList<TSubType> FindInternal<TSubType>([NotNull] Func<IQueryable<TSubType>> queryFactory, int? skip, int? count)
        {
            try
            {
                IQueryable<TSubType> query = queryFactory();

                if (skip.HasValue)
                {
                    query = query.Skip(skip.Value);
                }

                if (count.HasValue)
                {
                    query = query.Take(count.Value);
                }

                return query.ToList();
            }
            catch (EmptyResultException)
            {
                return new List<TSubType>();
            }
        }

        [NotNull]
        protected virtual PagedList<TSubType> FindPagedInternal<TSubType>(int pageIndex, int pageSize, [NotNull] Func<IQueryable<TRoot>, IQueryable<TSubType>> func)
            => FindPagedInternal(pageIndex, pageSize, () => func(CreateQueryable()));

        [NotNull]
        protected virtual PagedList<TRoot> FindPagedInternal(int pageIndex, int pageSize, [CanBeNull] Func<IQueryable<TRoot>, IQueryable<TRoot>> func)
            => FindPagedInternal(pageIndex, pageSize, () => func != null ? func(CreateQueryable()) : CreateQueryable());

        [NotNull]
        protected virtual PagedList<TSubType> FindPagedInternal<TSubType>(int pageIndex, int pageSize, [NotNull] Func<IQueryable<TSubType>> queryFactory)
        {
            try
            {
                IQueryable<TSubType> rowCountQueryOver = queryFactory();
                IFutureValue<int> rowCount =
                    rowCountQueryOver
                        .ToFutureValue(x => x.Count());

                IQueryable<TSubType> pagingQueryOver = queryFactory();
                IFutureEnumerable<TSubType> qryResult =
                    pagingQueryOver
                        .Skip((pageIndex - 1) * pageSize)
                        .Take(pageSize)
                        .ToFuture();

                return new PagedList<TSubType>(qryResult, pageIndex, pageSize, rowCount.Value);
            }
            catch (EmptyResultException)
            {
                return new PagedList<TSubType>(Enumerable.Empty<TSubType>(), 1, pageSize, 0);
            }
        }

        [NotNull]
        protected virtual IQueryable<TRoot> CreateQueryable()
            => Session.Query<TRoot>();
    }
}
