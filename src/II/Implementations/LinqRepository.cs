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
        : Repository<TRoot, TId>
        where TRoot : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        protected LinqRepository(ISessionProvider sessionProvider)
            : base(sessionProvider)
        {
        }

        /// <summary>
        ///     Gets an entity by a function.
        /// </summary>
        /// <param name="func">The given function.</param>
        /// <returns>The entity that is filtered by the function or null if not found.</returns>
        [CanBeNull]
        public virtual TRoot Get([NotNull] Func<IQueryable<TRoot>, IQueryable<TRoot>> func)
            => Execute(nameof(Get), () => GetInternal(func));

        /// <summary>
        ///     Gets an entity by a function.
        /// </summary>
        /// <param name="func">The given function.</param>
        /// <typeparam name="TResult">A type that is projected from our <typeparamref name="TRoot" /></typeparam>
        /// <returns>
        ///     An entity projected to an instance of type <typeparamref name="TResult" />, that satisfying the given
        ///     <paramref name="func" />.
        /// </returns>
        [CanBeNull]
        public TResult Get<TResult>([NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> func)
            => Execute(nameof(Get), () => GetInternal(func));

        /// <summary>
        ///     Executes the given query <paramref name="func" /> and returns the entity at position <paramref name="index" />
        ///     in the result.
        /// </summary>
        /// <param name="func">The given function.</param>
        /// <param name="index">The given index.</param>
        /// <returns>The entity that is filtered by the function or null if not found.</returns>
        [CanBeNull]
        public virtual TRoot GetAtIndex([NotNull] Func<IQueryable<TRoot>, IQueryable<TRoot>> func, int index)
            => Execute(nameof(GetAtIndex), () => GetAtIndexInternal(func, index));

        /// <summary>
        ///     Executes the given query <paramref name="func" /> and returns the entity at position <paramref name="index" />
        ///     in the result.
        /// </summary>
        /// <typeparam name="TResult">A type that is projected from our <typeparamref name="TRoot" /></typeparam>
        /// <param name="func">The given function.</param>
        /// <param name="index">The given index.</param>
        /// <returns>
        ///     An entity projected to an instance of type <typeparamref name="TResult" />, that satisfying the given
        ///     <paramref name="func" />.
        /// </returns>
        [CanBeNull]
        public TResult GetAtIndex<TResult>([NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> func, int index)
            => Execute(nameof(GetAtIndex), () => GetAtIndexInternal(func, index));

        /// <summary>
        ///     Find the records complying with the given function.
        /// </summary>
        /// <param name="func">The given function.</param>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para>All elements of the resulting set fulfill <paramref name="func" />.</para>
        /// </remarks>
        /// <returns>
        ///     A list of the records satisfying the given <paramref name="func" />.
        /// </returns>
        [NotNull]
        [ItemNotNull]
        public virtual IList<TRoot> Find([NotNull] Func<IQueryable<TRoot>, IQueryable<TRoot>> func)
            => Execute(nameof(Find), () => FindInternal(func))
               ?? new List<TRoot>();

        /// <summary>
        ///     Find the records complying with the given function.
        /// </summary>
        /// <typeparam name="TResult">A type that is projected from our <typeparamref name="TRoot" /></typeparam>
        /// <param name="func">The given function.</param>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para>All elements of the resulting set fulfill <paramref name="func" />.</para>
        /// </remarks>
        /// <returns>
        ///     A list of projected entities to an instance of type <typeparamref name="TResult" />, that satisfying the given
        ///     <paramref name="func" />.
        /// </returns>
        [NotNull]
        [ItemNotNull]
        public virtual IList<TResult> Find<TResult>([NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> func)
            => Execute(nameof(Find), () => FindInternal(func))
               ?? new List<TResult>();

        /// <summary>
        ///     Find a set of records complying with the given function.
        ///     Only a subset of records are returned based on <paramref name="pageSize" /> and <paramref name="pageIndex" />.
        /// </summary>
        /// <param name="pageIndex">The index of the page, indices start from 1.</param>
        /// <param name="pageSize">The size of a page, must be greater then 0.</param>
        /// <param name="func">The predicates that the data must fulfill.</param>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para>All elements of the resulting set fulfill <paramref name="func" />.</para>
        /// </remarks>
        /// <returns>
        ///     An implementation of <see cref="IPagedList{T}" />, that holds a max. of <paramref name="pageSize" /> records.
        /// </returns>
        [NotNull]
        public virtual IPagedList<TRoot> FindPaged(int pageIndex, int pageSize, [CanBeNull] Func<IQueryable<TRoot>, IQueryable<TRoot>> func)
            => Execute(nameof(FindPaged), () => FindPagedInternal(pageIndex, pageSize, func))
               ?? new PagedList<TRoot>(Enumerable.Empty<TRoot>(), pageIndex, pageSize, 0);

        /// <summary>
        ///     Find a set of records complying with the given function.
        ///     Only a subset of records are returned based on <paramref name="pageSize" /> and <paramref name="pageIndex" />.
        /// </summary>
        /// <typeparam name="TResult">A type that is projected from our <typeparamref name="TRoot" /></typeparam>
        /// <param name="pageIndex">The index of the page, indices start from 1.</param>
        /// <param name="pageSize">The size of a page, must be greater then 0.</param>
        /// <param name="func">The predicates that the data must fulfill.</param>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para>All elements of the resulting set fulfill <paramref name="func" />.</para>
        /// </remarks>
        /// <returns>
        ///     An implementation of <see cref="IPagedList{TResult}" />, that holds a max. of <paramref name="pageSize" /> records
        ///     of type <typeparamref name="TResult" />.
        /// </returns>
        [NotNull]
        public virtual IPagedList<TResult> FindPaged<TResult>(int pageIndex, int pageSize, [NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> func)
            => Execute(nameof(FindPaged), () => FindPagedInternal(pageIndex, pageSize, func))
               ?? new PagedList<TResult>(Enumerable.Empty<TResult>(), pageIndex, pageSize, 0);

        /// <summary>
        ///     Calculates the number of records complying with the given function.
        /// </summary>
        /// <param name="func">The predicates that the data must fulfill.</param>
        /// <returns>
        ///     Number of records that satisfying the given <paramref name="func" />.
        /// </returns>
        public virtual int Count([CanBeNull] Func<IQueryable<TRoot>, IQueryable<TRoot>> func)
            => Execute(nameof(Count), () => CountInternal(func));

        public virtual IList<TRoot> Find(Func<IQueryable<TRoot>, IQueryable<TRoot>> func, int? skip, int? count)
            => Execute(nameof(Find), () => FindInternal(func, skip, count));

        public virtual IList<TResult> Find<TResult>(Func<IQueryable<TRoot>, IQueryable<TResult>> func, int? skip, int? count)
            => Execute(nameof(Find), () => FindInternal(func, skip, count));

        /// <inheritdoc cref="Get" />
        /// <exception cref="EmptyResultException">
        ///     If <paramref name="func" /> throws this type of excpetion, a <c>null</c> will be returned.
        /// </exception>
        [CanBeNull]
        protected virtual TRoot GetInternal([NotNull] Func<IQueryable<TRoot>, IQueryable<TRoot>> func)
        {
            try
            {
                return func.Invoke(CreateQueryable()).SingleOrDefault();
            }
            catch (EmptyResultException)
            {
                return default(TRoot);
            }
        }

        /// <inheritdoc cref="Get{TResult}" />
        /// <exception cref="EmptyResultException">
        ///     If <paramref name="func" /> throws this type of exception, a <c>null</c> will be returned.
        /// </exception>
        [CanBeNull]
        protected virtual TResult GetInternal<TResult>([NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> func)
        {
            try
            {
                return func.Invoke(CreateQueryable()).SingleOrDefault();
            }
            catch (EmptyResultException)
            {
                return default(TResult);
            }
        }

        /// <inheritdoc cref="GetAtIndex" />
        /// <exception cref="EmptyResultException">
        ///     If <paramref name="func" /> throws this type of excpetion, a <c>null</c> will be returned.
        /// </exception>
        [CanBeNull]
        protected virtual TRoot GetAtIndexInternal([NotNull] Func<IQueryable<TRoot>, IQueryable<TRoot>> func, int index)
        {
            try
            {
                return func.Invoke(CreateQueryable()).Skip(index).Take(1).SingleOrDefault();
            }
            catch (EmptyResultException)
            {
                return default(TRoot);
            }
        }

        /// <inheritdoc cref="GetAtIndex{TResult}" />
        /// <exception cref="EmptyResultException">
        ///     If <paramref name="func" /> throws this type of exception, a <c>null</c> will be returned.
        /// </exception>
        [CanBeNull]
        protected virtual TResult GetAtIndexInternal<TResult>([NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> func, int index)
        {
            try
            {
                return func.Invoke(CreateQueryable()).Skip(index).Take(1).SingleOrDefault();
            }
            catch (EmptyResultException)
            {
                return default(TResult);
            }
        }

        protected override IList<TRoot> FindAllInternal()
            => FindInternal(null);

        /// <inheritdoc
        ///     cref="FindInternal(System.Func{System.Linq.IQueryable{TRoot},System.Linq.IQueryable{TRoot}},int?,int?)" />
        [NotNull]
        [ItemNotNull]
        protected virtual IList<TRoot> FindInternal([CanBeNull] Func<IQueryable<TRoot>, IQueryable<TRoot>> func)
            => FindInternal(func, null, null);

        /// <inheritdoc
        ///     cref="FindInternal(System.Func{System.Linq.IQueryable{TRoot},System.Linq.IQueryable{TRoot}},int?,int?)" />
        [NotNull]
        [ItemNotNull]
        protected virtual IList<TResult> FindInternal<TResult>([NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> func)
            => FindInternal(func, null, null);

        /// <inheritdoc
        ///     cref="FindInternal(System.Func{System.Linq.IQueryable{TRoot},System.Linq.IQueryable{TRoot}},int?,int?)" />
        [NotNull]
        [ItemNotNull]
        protected virtual IList<TRoot> FindInternal([CanBeNull] Func<IQueryable<TRoot>, IQueryable<TRoot>> func, int? skip, int? count)
            => FindInternal(() => func != null ? func(CreateQueryable()) : CreateQueryable(), skip, count);

        /// <inheritdoc
        ///     cref="FindInternal{TResult}(System.Func{System.Linq.IQueryable{TResult}},int?,int?)" />
        [NotNull]
        [ItemNotNull]
        protected virtual IList<TResult> FindInternal<TResult>([NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> func, int? skip, int? count)
            => FindInternal(() => func(CreateQueryable()), skip, count);

        /// <summary>
        ///     <para>
        ///         Find a set of records that fulfills the <paramref name="queryFactory" /> and optionally we have a maximum of
        ///         <paramref name="count" /> records.In the result set we kan optionally skip the first <paramref name="skip" />
        ///         records.
        ///     </para>
        ///     <para>
        ///         When using the optional parameters think about the order of the set of records, aka sorting.
        ///     </para>
        /// </summary>
        /// <param name="queryFactory">Create the base of the query of type <typeparamref name="TResult" />.</param>
        /// <param name="skip">Optional parameter to skip a number of records.</param>
        /// <param name="count">Maximum number of records to be included in our result.</param>
        /// <typeparam name="TResult">A type that is projected from our <typeparamref name="TRoot" /></typeparam>
        /// <returns>A set of records </returns>
        /// <exception cref="EmptyResultException">
        ///     If <paramref name="queryFactory" /> throws this type of exception, a <c>null</c> will be returned.
        /// </exception>
        [NotNull]
        [ItemNotNull]
        protected virtual IList<TResult> FindInternal<TResult>([NotNull] Func<IQueryable<TResult>> queryFactory, int? skip, int? count)
        {
            try
            {
                IQueryable<TResult> query = queryFactory();
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
                return new List<TResult>();
            }
        }

        [NotNull]
        protected virtual PagedList<TResult> FindPagedInternal<TResult>(int pageIndex, int pageSize, [NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> func)
            => FindPagedInternal(pageIndex, pageSize, () => func(CreateQueryable()));

        [NotNull]
        protected virtual PagedList<TRoot> FindPagedInternal(int pageIndex, int pageSize, [CanBeNull] Func<IQueryable<TRoot>, IQueryable<TRoot>> func)
            => FindPagedInternal(pageIndex, pageSize, () => func != null ? func(CreateQueryable()) : CreateQueryable());

        [NotNull]
        protected virtual PagedList<TResult> FindPagedInternal<TResult>(int pageIndex, int pageSize, [NotNull] Func<IQueryable<TResult>> queryFactory)
        {
            try
            {
                IQueryable<TResult> rowCountQueryOver = queryFactory();
                IFutureValue<int> rowCount =
                    rowCountQueryOver
                        .ToFutureValue(x => x.Count());

                IQueryable<TResult> pagingQueryOver = queryFactory();
                IFutureEnumerable<TResult> qryResult =
                    pagingQueryOver
                        .Skip((pageIndex - 1) * pageSize)
                        .Take(pageSize)
                        .ToFuture();

                return new PagedList<TResult>(qryResult, pageIndex, pageSize, rowCount.Value);
            }
            catch (EmptyResultException)
            {
                return new PagedList<TResult>(Enumerable.Empty<TResult>(), 1, pageSize, 0);
            }
        }

        protected virtual int CountInternal([CanBeNull] Func<IQueryable<TRoot>, IQueryable<TRoot>> func)
        {
            try
            {
                return func?.Invoke(CreateQueryable()).Count() ?? CreateQueryable().Count();
            }
            catch (EmptyResultException)
            {
                return 0;
            }
        }

        [NotNull]
        [ItemNotNull]
        protected override IEnumerable<TRoot> FindByIdsInternal([NotNull] IEnumerable<TId> ids)
            => FindInternal(qry => qry.Where(e => ids.Contains(e.Id)));

        [NotNull]
        protected virtual IQueryable<TRoot> CreateQueryable()
            => Session.Query<TRoot>();
    }
}
