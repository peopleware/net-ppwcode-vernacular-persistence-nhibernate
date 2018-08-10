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
using System.Linq.Expressions;

using JetBrains.Annotations;

using NHibernate;

using PPWCode.Vernacular.NHibernate.II.Interfaces;
using PPWCode.Vernacular.Persistence.III;

namespace PPWCode.Vernacular.NHibernate.II.Implementations
{
    public abstract class QueryOverRepository<TRoot, TId>
        : Repository<TRoot, TId>,
          IQueryOverRepository<TRoot, TId>
        where TRoot : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        protected QueryOverRepository([NotNull] ISessionProvider sessionProvider)
            : base(sessionProvider)
        {
        }

        public virtual TRoot Get(Expression<Func<TRoot>> alias, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
            => Execute(nameof(Get), () => GetInternal(alias, func));

        public virtual TRoot Get(Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
            => Execute(nameof(Get), () => GetInternal(func));

        public virtual TRoot GetAtIndex(Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func, int index)
            => Execute(nameof(GetAtIndex), () => GetAtIndexInternal(func, index));

        public virtual TRoot GetAtIndex(Expression<Func<TRoot>> alias, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func, int index)
            => Execute(nameof(GetAtIndex), () => GetAtIndexInternal(alias, func, index));

        public virtual IList<TRoot> Find(Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
            => Execute(nameof(Find), () => FindInternal(func)) ?? new List<TRoot>();

        public virtual IList<TRoot> Find(Expression<Func<TRoot>> alias, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
            => Execute(nameof(Find), () => FindInternal(alias, func)) ?? new List<TRoot>();

        public virtual IList<TRoot> Find(Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func, int? skip, int? count)
            => Execute(nameof(Find), () => FindInternal(func, skip, count)) ?? new List<TRoot>();

        public virtual IList<TRoot> Find(Expression<Func<TRoot>> alias, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func, int? skip, int? count)
            => Execute(nameof(Find), () => FindInternal(alias, func, skip, count)) ?? new List<TRoot>();

        public virtual IPagedList<TRoot> FindPaged(int pageIndex, int pageSize, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
            => Execute(nameof(FindPaged), () => FindPagedInternal(pageIndex, pageSize, func)) ?? new PagedList<TRoot>(Enumerable.Empty<TRoot>(), pageIndex, pageSize, 0);

        public virtual IPagedList<TRoot> FindPaged(int pageIndex, int pageSize, Expression<Func<TRoot>> alias, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
            => Execute(nameof(FindPaged), () => FindPagedInternal(pageIndex, pageSize, alias, func)) ?? new PagedList<TRoot>(Enumerable.Empty<TRoot>(), pageIndex, pageSize, 0);

        [CanBeNull]
        protected virtual TRoot GetInternal([NotNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
            => func(CreateQueryOver()).SingleOrDefault<TRoot>();

        [CanBeNull]
        protected virtual TRoot GetInternal(Expression<Func<TRoot>> alias, [NotNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
            => func(CreateQueryOver(alias)).SingleOrDefault<TRoot>();

        [CanBeNull]
        protected virtual TRoot GetAtIndexInternal([NotNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func, int index)
            => func(CreateQueryOver()).Skip(index).Take(1).SingleOrDefault<TRoot>();

        [CanBeNull]
        protected virtual TRoot GetAtIndexInternal([CanBeNull] Expression<Func<TRoot>> alias, [NotNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func, int index)
            => func(CreateQueryOver(alias)).Skip(index).Take(1).SingleOrDefault<TRoot>();

        protected override IList<TRoot> FindAllInternal()
            => CreateQueryOver().List<TRoot>();

        [NotNull]
        protected virtual IList<TRoot> FindInternal([CanBeNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
        {
            IQueryOver<TRoot> queryOver = func != null ? func(CreateQueryOver()) : CreateQueryOver();
            return queryOver.List<TRoot>();
        }

        [NotNull]
        protected virtual IList<TRoot> FindInternal([CanBeNull] Expression<Func<TRoot>> alias, [CanBeNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
        {
            IQueryOver<TRoot> queryOver = func != null ? func(CreateQueryOver(alias)) : CreateQueryOver(alias);
            return queryOver.List<TRoot>();
        }

        [NotNull]
        protected virtual IList<TRoot> FindInternal([CanBeNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func, int? skip, int? count)
        {
            IQueryOver<TRoot> queryOver = func != null ? func(CreateQueryOver()) : CreateQueryOver();

            if (skip.HasValue)
            {
                queryOver = queryOver.Skip(skip.Value);
            }

            if (count.HasValue)
            {
                queryOver = queryOver.Take(count.Value);
            }

            return queryOver.List<TRoot>();
        }

        [NotNull]
        protected virtual IList<TRoot> FindInternal([CanBeNull] Expression<Func<TRoot>> alias, [CanBeNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func, int? skip, int? count)
        {
            IQueryOver<TRoot> queryOver = func != null ? func(CreateQueryOver(alias)) : CreateQueryOver(alias);

            if (skip.HasValue)
            {
                queryOver = queryOver.Skip(skip.Value);
            }

            if (count.HasValue)
            {
                queryOver = queryOver.Take(count.Value);
            }

            return queryOver.List<TRoot>();
        }

        [NotNull]
        protected virtual PagedList<TRoot> FindPagedInternal(int pageIndex, int pageSize, [CanBeNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
            => FindPagedInternal(pageIndex, pageSize, () => func != null ? func(CreateQueryOver()) : CreateQueryOver());

        [NotNull]
        protected virtual PagedList<TRoot> FindPagedInternal(int pageIndex, int pageSize, [CanBeNull] Expression<Func<TRoot>> alias, [CanBeNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
            => FindPagedInternal(pageIndex, pageSize, () => func != null ? func(CreateQueryOver(alias)) : CreateQueryOver(alias));

        [NotNull]
        protected virtual PagedList<TRoot> FindPagedInternal(int pageIndex, int pageSize, [NotNull] Func<IQueryOver<TRoot, TRoot>> queryFactory)
            => FindPagedInternal<TRoot, TRoot>(pageIndex, pageSize, queryFactory);

        [NotNull]
        protected virtual PagedList<TSubType> FindPagedInternal<TSubType>(int pageIndex, int pageSize, [NotNull] Func<IQueryOver<TRoot, TRoot>> queryFactory)
            => FindPagedInternal<TSubType, TRoot>(pageIndex, pageSize, queryFactory);

        [NotNull]
        protected virtual PagedList<TDto> FindPagedInternal<TDto, TSubType>(int pageIndex, int pageSize, [NotNull] Func<IQueryOver<TRoot, TSubType>> queryFactory)
        {
            IQueryOver<TRoot, TSubType> rowCountQueryOver = queryFactory();
            IFutureValue<int> rowCount =
                rowCountQueryOver
                    .ToRowCountQuery()
                    .FutureValue<int>();

            IQueryOver<TRoot, TSubType> pagingQueryOver = queryFactory();
            IList<TDto> qryResult =
                pagingQueryOver
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Future<TDto>()
                    .ToList();

            PagedList<TDto> result = new PagedList<TDto>(qryResult, pageIndex, pageSize, rowCount.Value);

            return result;
        }

        [NotNull]
        protected virtual IQueryOver<TRoot, TRoot> CreateQueryOver()
            => CreateQueryOver(() => null);

        [NotNull]
        protected virtual IQueryOver<TRoot, TRoot> CreateQueryOver([CanBeNull] Expression<Func<TRoot>> alias)
            => Session.QueryOver(alias);
    }
}
