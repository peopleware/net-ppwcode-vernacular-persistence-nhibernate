// Copyright 2020 by PeopleWare n.v..
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
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using NHibernate;
using NHibernate.Linq;

using PPWCode.Vernacular.NHibernate.II.Async.Interfaces.Providers;
using PPWCode.Vernacular.Persistence.III;

namespace PPWCode.Vernacular.NHibernate.II.Async.Implementations
{
    public abstract class LinqRepositoryAsync<TRoot, TId>
        : RepositoryAsync<TRoot, TId>
        where TRoot : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        protected LinqRepositoryAsync([NotNull] ISessionProviderAsync sessionProviderAsync)
            : base(sessionProviderAsync)
        {
        }

        /// <inheritdoc cref="LinqRepository{TRoot,TId}.Get{TResult}" />
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        [NotNull]
        [ItemCanBeNull]
        public virtual async Task<TResult> GetAsync<TResult>(
            [NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> lambda,
            CancellationToken cancellationToken)
            => await ExecuteAsync(nameof(GetAsync), can => GetInternalAsync(lambda, can), cancellationToken)
                   .ConfigureAwait(false);

        /// <inheritdoc cref="LinqRepository{TRoot,TId}.GetAtIndex{TResult}" />
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        [NotNull]
        [ItemCanBeNull]
        public virtual async Task<TResult> GetAtIndexAsync<TResult>(
            [NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> lambda,
            int index,
            CancellationToken cancellationToken)
            => await ExecuteAsync(nameof(GetAtIndexAsync), can => GetAtIndexInternalAsync(lambda, index, can), cancellationToken)
                   .ConfigureAwait(false);

        /// <inheritdoc
        ///     cref="LinqRepository{TRoot,TId}.Find{TResult}(System.Func{System.Linq.IQueryable{TRoot},System.Linq.IQueryable{TResult}})" />
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        [NotNull]
        [ItemNotNull]
        public virtual async Task<IList<TResult>> FindAsync<TResult>(
            [NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> lambda,
            CancellationToken cancellationToken)
            => await ExecuteAsync(
                       nameof(FindAsync),
                       can => FindInternalAsync(() => lambda(CreateQueryable()), null, null, can),
                       cancellationToken)
                   .ConfigureAwait(false)
               ?? new List<TResult>();

        /// <inheritdoc
        ///     cref="LinqRepository{TRoot,TId}.Find{TResult}(System.Func{System.Linq.IQueryable{TRoot},System.Linq.IQueryable{TResult}},int?,int?)" />
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        [NotNull]
        [ItemNotNull]
        public virtual async Task<IList<TResult>> FindAsync<TResult>(
            [NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> lambda,
            [CanBeNull] int? skip,
            [CanBeNull] int? count,
            CancellationToken cancellationToken)
            => await ExecuteAsync(
                       nameof(FindAsync),
                       can => FindInternalAsync(() => lambda(CreateQueryable()), skip, count, can),
                       cancellationToken)
                   .ConfigureAwait(false)
               ?? new List<TResult>();

        /// <inheritdoc
        ///     cref="LinqRepository{TRoot,TId}.FindPaged{TResult}" />
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        [NotNull]
        [ItemNotNull]
        public virtual async Task<IPagedList<TResult>> FindPagedAsync<TResult>(
            [NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> lambda,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken)
            => await ExecuteAsync(
                       nameof(FindPagedAsync),
                       can => FindPagedInternalAsync(() => lambda(CreateQueryable()), pageIndex, pageSize, can),
                       cancellationToken)
                   .ConfigureAwait(false)
               ?? new PagedList<TResult>(Enumerable.Empty<TResult>(), pageIndex, pageSize, 0);

        /// <inheritdoc
        ///     cref="LinqRepository{TRoot,TId}.Count" />
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        [NotNull]
        public virtual async Task<int> CountAsync(
            [NotNull] Func<IQueryable<TRoot>, IQueryable<TRoot>> lambda,
            CancellationToken cancellationToken)
            => await ExecuteAsync(
                       nameof(CountAsync),
                       can => CountInternalAsync(() => lambda(CreateQueryable()), can),
                       cancellationToken)
                   .ConfigureAwait(false);

        /// <inheritdoc />
        protected override async Task<IList<TRoot>> FindAllInternalAsync(CancellationToken cancellationToken)
            => await FindInternalAsync(CreateQueryable, null, null, cancellationToken)
                   .ConfigureAwait(false);

        /// <inheritdoc />
        protected override async Task<IList<TRoot>> FindByIdsInternalAsync(TId[] ids, CancellationToken cancellationToken)
            => await FindInternalAsync(() => CreateQueryable().Where(e => ids.Contains(e.Id)), null, null, cancellationToken)
                   .ConfigureAwait(false);

        /// <inheritdoc cref="LinqRepository{TRoot,TId}.Get{TResult}" />
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        [NotNull]
        [ItemCanBeNull]
        protected virtual async Task<TResult> GetInternalAsync<TResult>(
            [NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> lambda,
            CancellationToken cancellationToken)
        {
            try
            {
                return
                    await lambda(CreateQueryable())
                        .SingleOrDefaultAsync(cancellationToken)
                        .ConfigureAwait(false);
            }
            catch (EmptyResultException)
            {
                return default;
            }
        }

        /// <inheritdoc cref="LinqRepository{TRoot,TId}.GetAtIndex{TResult}" />
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        [NotNull]
        [ItemCanBeNull]
        protected virtual async Task<TResult> GetAtIndexInternalAsync<TResult>(
            [NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> lambda,
            int index,
            CancellationToken cancellationToken)
        {
            try
            {
                return
                    await lambda(CreateQueryable())
                        .Skip(index)
                        .Take(1)
                        .SingleOrDefaultAsync(cancellationToken)
                        .ConfigureAwait(false);
            }
            catch (EmptyResultException)
            {
                return default;
            }
        }

        /// <inheritdoc
        ///     cref="LinqRepository{TRoot,TId}.FindInternal{TResult}(System.Func{System.Linq.IQueryable{TResult}},int?,int?)" />
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        [NotNull]
        [ItemNotNull]
        protected virtual async Task<IList<TResult>> FindInternalAsync<TResult>(
            [NotNull] Func<IQueryable<TResult>> lambda,
            [CanBeNull] int? skip,
            [CanBeNull] int? count,
            CancellationToken cancellationToken)
        {
            try
            {
                IQueryable<TResult> query = lambda();
                if (skip != null)
                {
                    query = query.Skip(skip.Value);
                }

                if (count != null)
                {
                    query = query.Take(count.Value);
                }

                return
                    await query
                        .ToListAsync(cancellationToken)
                        .ConfigureAwait(false);
            }
            catch (EmptyResultException)
            {
                return new List<TResult>();
            }
        }

        /// <inheritdoc
        ///     cref="LinqRepository{TRoot,TId}.FindPagedInternal{TResult}(System.Func{System.Linq.IQueryable{TResult}},int,int)" />
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        [NotNull]
        [ItemNotNull]
        protected virtual async Task<PagedList<TResult>> FindPagedInternalAsync<TResult>(
            [NotNull] Func<IQueryable<TResult>> lambda,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken)
        {
            try
            {
                IQueryable<TResult> countQuery = lambda();
                IFutureValue<int> rowCount =
                    countQuery
                        .ToFutureValue(x => x.Count());

                IQueryable<TResult> pagingQuery = lambda();
                IFutureEnumerable<TResult> qryResult =
                    pagingQuery
                        .Skip((pageIndex - 1) * pageSize)
                        .Take(pageSize)
                        .ToFuture();

                IEnumerable<TResult> items =
                    await qryResult
                        .GetEnumerableAsync(cancellationToken)
                        .ConfigureAwait(false);
                int count =
                    await rowCount
                        .GetValueAsync(cancellationToken)
                        .ConfigureAwait(false);
                return new PagedList<TResult>(items, pageIndex, pageSize, count);
            }
            catch (EmptyResultException)
            {
                return new PagedList<TResult>(Enumerable.Empty<TResult>(), 1, pageSize, 0);
            }
        }

        /// <inheritdoc
        ///     cref="LinqRepository{TRoot,TId}.CountInternal" />
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        protected virtual async Task<int> CountInternalAsync(
            [NotNull] Func<IQueryable<TRoot>> lambda,
            CancellationToken cancellationToken)
        {
            try
            {
                return
                    await lambda()
                        .CountAsync(cancellationToken)
                        .ConfigureAwait(false);
            }
            catch (EmptyResultException)
            {
                return 0;
            }
        }

        [NotNull]
        protected virtual IQueryable<TRoot> CreateQueryable()
            => Session.Query<TRoot>();
    }
}
