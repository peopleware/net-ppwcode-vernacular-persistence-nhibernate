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

using PPWCode.Vernacular.NHibernate.III.Providers;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III
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
        ///     Gets an entity by the given query, expressed as a <paramref name="lambda" />.
        /// </summary>
        /// <param name="lambda">The given query, expressed as a lambda.</param>
        /// <typeparam name="TResult">A type that is projected from our <typeparamref name="TRoot" /></typeparam>
        /// <returns>
        ///     <para>
        ///         An entity projected to an instance of type <typeparamref name="TResult" />, that satisfying the given
        ///         query, expressed as a <paramref name="lambda" />.
        ///     </para>
        ///     <para>If no entity is found, <c>null</c> will be returned</para>
        /// </returns>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para>If an entity is found, it fulfills the query, expressed by <paramref name="lambda" />.</para>
        /// </remarks>
        /// <exception cref="EmptyResultException">
        ///     If <paramref name="lambda" /> throws this type of exception, a <c>null</c> will be returned.
        /// </exception>
        [CanBeNull]
        public TResult Get<TResult>([NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> lambda)
            => Execute(nameof(Get), () => GetInternal(lambda));

        /// <summary>
        ///     Executes the given query, expressed as a <paramref name="lambda" />, and returns the entity at position
        ///     <paramref name="index" />
        ///     in the result.
        /// </summary>
        /// <typeparam name="TResult">A type that is projected from our <typeparamref name="TRoot" /></typeparam>
        /// <param name="lambda">The given query, expressed as a lambda.</param>
        /// <param name="index">The given index.</param>
        /// <returns>
        ///     <para>
        ///         An entity projected to an instance of type <typeparamref name="TResult" />, that satisfying the given
        ///         query, expressed as a <paramref name="lambda" />.
        ///     </para>
        ///     <para>If no entity is found, <c>null</c> will be returned</para>
        /// </returns>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para>If an entity is found, it fulfills the query, expressed by <paramref name="lambda" />.</para>
        /// </remarks>
        /// <exception cref="EmptyResultException">
        ///     If <paramref name="lambda" /> throws this type of exception, a <c>null</c> will be returned.
        /// </exception>
        [CanBeNull]
        public TResult GetAtIndex<TResult>([NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> lambda, int index)
            => Execute(nameof(GetAtIndex), () => GetAtIndexInternal(lambda, index));

        /// <summary>
        ///     Executes the given query, expressed as a <paramref name="lambda" />.
        /// </summary>
        /// <typeparam name="TResult">A type that is projected from our <typeparamref name="TRoot" /></typeparam>
        /// <param name="lambda">The given query, expressed as a lambda.</param>
        /// <returns>
        ///     <para>
        ///         A list of projected entities to an instance of type <typeparamref name="TResult" />, that satisfying the given
        ///         <paramref name="lambda" />.
        ///     </para>
        ///     <para>If no information is found, an empty list will be returned.</para>
        /// </returns>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para>All elements of the resulting set fulfills the query, expressed by <paramref name="lambda" />.</para>
        /// </remarks>
        /// <exception cref="EmptyResultException">
        ///     If <paramref name="lambda" /> throws this type of exception, an <c>empty list</c> will be returned.
        /// </exception>
        [NotNull]
        [ItemNotNull]
        public virtual IList<TResult> Find<TResult>([NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> lambda)
            => Execute(nameof(Find), () => FindInternal(() => lambda(CreateQueryable()), null, null))
               ?? new List<TResult>();

        /// <summary>
        ///     Executes the given query, expressed as a <paramref name="lambda" />.
        /// </summary>
        /// <typeparam name="TResult">A type that is projected from our <typeparamref name="TRoot" /></typeparam>
        /// <param name="lambda">The given query, expressed as a lambda.</param>
        /// <param name="skip">Optional number of record(s) to skip.</param>
        /// <param name="count">Optional maximum number of record(s) to take, after skipping.</param>
        /// <returns>
        ///     <para>
        ///         A list of projected entities to an instance of type <typeparamref name="TResult" />, that satisfying the given
        ///         <paramref name="lambda" />.
        ///     </para>
        ///     <para>
        ///         The first <paramref name="skip" /> record(s) are skipped and then a maximum of <paramref name="count" />
        ///         records are taken.
        ///     </para>
        ///     <para>If no information is found, an empty list will be returned.</para>
        /// </returns>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para>All elements of the resulting set fulfills the query, expressed by <paramref name="lambda" />.</para>
        /// </remarks>
        /// <exception cref="EmptyResultException">
        ///     If <paramref name="lambda" /> throws this type of exception, an <c>empty list</c> will be returned.
        /// </exception>
        [NotNull]
        [ItemNotNull]
        public virtual IList<TResult> Find<TResult>(
            [NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> lambda,
            int? skip,
            int? count)
            => Execute(nameof(Find), () => FindInternal(() => lambda(CreateQueryable()), skip, count))
               ?? new List<TResult>();

        /// <summary>
        ///     <para>Executes the given query, expressed as a <paramref name="lambda" />.</para>
        ///     <para>
        ///         Only a subset of records, called a page, is returned based on <paramref name="pageSize" /> and
        ///         <paramref name="pageIndex" />
        ///     </para>
        /// </summary>
        /// <typeparam name="TResult">A type that is projected from our <typeparamref name="TRoot" /></typeparam>
        /// <param name="lambda">The given query, expressed as a lambda.</param>
        /// <param name="pageIndex">The index of the page, indices start from 1.</param>
        /// <param name="pageSize">The size of a page, must be greater then 0.</param>
        /// <returns>
        ///     An implementation of <see cref="IPagedList{TResult}" />, that holds a max. of <paramref name="pageSize" /> records
        ///     of type <typeparamref name="TResult" />.
        /// </returns>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para>All elements of the resulting set fulfills the query, expressed by <paramref name="lambda" />.</para>
        /// </remarks>
        /// <exception cref="EmptyResultException">
        ///     If <paramref name="lambda" /> throws this type of exception, an <c>empty page</c> will be returned.
        /// </exception>
        [NotNull]
        public virtual IPagedList<TResult> FindPaged<TResult>(
            [NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> lambda,
            int pageIndex,
            int pageSize)
            => Execute(nameof(FindPaged), () => FindPagedInternal(() => lambda(CreateQueryable()), pageIndex, pageSize))
               ?? new PagedList<TResult>(Enumerable.Empty<TResult>(), pageIndex, pageSize, 0);

        /// <summary>
        ///     Calculates the number of records of the given query, expressed as a <paramref name="lambda" />.
        /// </summary>
        /// <param name="lambda">The given query, expressed as a lambda.</param>
        /// <returns>
        ///     Number of records that satisfying the given query, expressed as a <paramref name="lambda" />.
        /// </returns>
        /// <exception cref="EmptyResultException">
        ///     If <paramref name="lambda" /> throws this type of exception, a <c>0</c> will be returned.
        /// </exception>
        public virtual int Count([NotNull] Func<IQueryable<TRoot>, IQueryable<TRoot>> lambda)
            => Execute(nameof(Count), () => CountInternal(lambda));

        /// <inheritdoc />
        protected override IList<TRoot> FindAllInternal()
            => FindInternal(CreateQueryable, null, null);

        /// <inheritdoc cref="Repository{TRoot,TId}.FindByIdsInternal" />
        [NotNull]
        [ItemNotNull]
        protected override IEnumerable<TRoot> FindByIdsInternal([NotNull] IEnumerable<TId> ids)
            => FindInternal(() => CreateQueryable().Where(e => ids.Contains(e.Id)), null, null);

        /// <inheritdoc cref="Get{TResult}" />
        [CanBeNull]
        protected virtual TResult GetInternal<TResult>(
            [NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> lambda)
        {
            try
            {
                return lambda.Invoke(CreateQueryable()).SingleOrDefault();
            }
            catch (EmptyResultException)
            {
                return default;
            }
        }

        /// <inheritdoc cref="GetAtIndex{TResult}" />
        [CanBeNull]
        protected virtual TResult GetAtIndexInternal<TResult>(
            [NotNull] Func<IQueryable<TRoot>, IQueryable<TResult>> lambda,
            int index)
        {
            try
            {
                return lambda.Invoke(CreateQueryable()).Skip(index).Take(1).SingleOrDefault();
            }
            catch (EmptyResultException)
            {
                return default;
            }
        }

        // TODO, i don't get it why <inheritdoc cref="Find{TResult}(Func{IQuerable{TRoot},IQuerable{TResult}},int?,int?)" />doesn't work

        /// <summary>
        ///     Executes the given query, expressed as a <paramref name="lambda" />.
        /// </summary>
        /// <typeparam name="TResult">A type that is projected from our <typeparamref name="TRoot" /></typeparam>
        /// <param name="lambda">The given query, expressed as a lambda.</param>
        /// <param name="skip">Optional number of record(s) to skip.</param>
        /// <param name="count">Optional maximum number of record(s) to take, after skipping.</param>
        /// <returns>
        ///     <para>
        ///         A list of projected entities to an instance of type <typeparamref name="TResult" />, that satisfying the given
        ///         <paramref name="lambda" />.
        ///     </para>
        ///     <para>
        ///         The first <paramref name="skip" /> record(s) are skipped and then a maximum of <paramref name="count" />
        ///         records are taken.
        ///     </para>
        ///     <para>If no information is found, an empty list will be returned.</para>
        /// </returns>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para>All elements of the resulting set fulfills the query, expressed by <paramref name="lambda" />.</para>
        /// </remarks>
        /// <exception cref="EmptyResultException">
        ///     If <paramref name="lambda" /> throws this type of exception, an <c>empty list</c> will be returned.
        /// </exception>
        [NotNull]
        [ItemNotNull]
        protected virtual IList<TResult> FindInternal<TResult>(
            [NotNull] Func<IQueryable<TResult>> lambda,
            [CanBeNull] int? skip,
            [CanBeNull] int? count)
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

                return query.ToList();
            }
            catch (EmptyResultException)
            {
                return new List<TResult>();
            }
        }

        /// <inheritdoc cref="FindPaged{TResult}" />
        [NotNull]
        protected virtual PagedList<TResult> FindPagedInternal<TResult>(
            [NotNull] Func<IQueryable<TResult>> lambda,
            int pageIndex,
            int pageSize)
        {
            try
            {
                IQueryable<TResult> rowCountQueryOver = lambda();
                IFutureValue<int> rowCount =
                    rowCountQueryOver
                        .ToFutureValue(x => x.Count());

                IQueryable<TResult> pagingQueryOver = lambda();
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

        /// <inheritdoc cref="Count" />
        protected virtual int CountInternal(
            [NotNull] Func<IQueryable<TRoot>, IQueryable<TRoot>> lambda)
        {
            try
            {
                return lambda.Invoke(CreateQueryable()).Count();
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
