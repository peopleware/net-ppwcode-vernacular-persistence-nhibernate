﻿// Copyright 2017 by PeopleWare n.v..
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

using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III
{
    public interface ILinqRepository<T, in TId> : IRepository<T, TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        ///     Gets an entity by a function.
        /// </summary>
        /// <param name="func">The given function.</param>
        /// <returns>The entity that is filtered by the function or null if not found.</returns>
        [CanBeNull]
        T Get([NotNull] Func<IQueryable<T>, IQueryable<T>> func);

        /// <summary>
        ///     Gets an entity by a function.
        /// </summary>
        /// <param name="func">The given function.</param>
        /// <typeparam name="TResult">A type that is projected from our <typeparamref name="T" /></typeparam>
        /// <returns>
        ///     An entity projected to an instance of type <typeparamref name="TResult" />, that satisfying the given
        ///     <paramref name="func" />.
        /// </returns>
        TResult Get<TResult>([NotNull] Func<IQueryable<T>, IQueryable<TResult>> func);

        /// <summary>
        ///     Executes the given query <paramref name="func" /> and returns the entity at position <paramref name="index" />
        ///     in the result.
        /// </summary>
        /// <param name="func">The given function.</param>
        /// <param name="index">The given index.</param>
        /// <returns>The entity that is filtered by the function or null if not found.</returns>
        [CanBeNull]
        T GetAtIndex([NotNull] Func<IQueryable<T>, IQueryable<T>> func, int index);

        /// <summary>
        ///     Executes the given query <paramref name="func" /> and returns the entity at position <paramref name="index" />
        ///     in the result.
        /// </summary>
        /// <typeparam name="TResult">A type that is projected from our <typeparamref name="T" /></typeparam>
        /// <param name="func">The given function.</param>
        /// <param name="index">The given index.</param>
        /// <returns>
        ///     An entity projected to an instance of type <typeparamref name="TResult" />, that satisfying the given
        ///     <paramref name="func" />.
        /// </returns>
        TResult GetAtIndex<TResult>([NotNull] Func<IQueryable<T>, IQueryable<TResult>> func, int index);

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
        IList<T> Find([NotNull] Func<IQueryable<T>, IQueryable<T>> func);

        /// <summary>
        ///     Find the records complying with the given function.
        /// </summary>
        /// <typeparam name="TResult">A type that is projected from our <typeparamref name="T" /></typeparam>
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
        IList<TResult> Find<TResult>([NotNull] Func<IQueryable<T>, IQueryable<TResult>> func);

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
        IPagedList<T> FindPaged(int pageIndex, int pageSize, [CanBeNull] Func<IQueryable<T>, IQueryable<T>> func);

        /// <summary>
        ///     Find a set of records complying with the given function.
        ///     Only a subset of records are returned based on <paramref name="pageSize" /> and <paramref name="pageIndex" />.
        /// </summary>
        /// <typeparam name="TResult">A type that is projected from our <typeparamref name="T" /></typeparam>
        /// <param name="pageIndex">The index of the page, indices start from 1.</param>
        /// <param name="pageSize">The size of a page, must be greater then 0.</param>
        /// <param name="func">The predicates that the data must fulfill.</param>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para>All elements of the resulting set fulfill <paramref name="func" />.</para>
        /// </remarks>
        /// <returns>
        ///     An implementation of <see cref="IPagedList{TResult}" />, that holds a max. of <paramref name="pageSize" /> records of type <typeparamref name="TResult"/>.
        /// </returns>
        [NotNull]
        IPagedList<TResult> FindPaged<TResult>(int pageIndex, int pageSize, [CanBeNull] Func<IQueryable<T>, IQueryable<TResult>> func);

        /// <summary>
        ///     Calculates the number of records complying with the given function.
        /// </summary>
        /// <param name="func">The predicates that the data must fulfill.</param>
        /// <returns>
        ///     Number of records that satisfying the given <paramref name="func" />.
        /// </returns>
        int Count([CanBeNull] Func<IQueryable<T>, IQueryable<T>> func);
    }
}
