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
using System.Diagnostics.Contracts;

using NHibernate;
using NHibernate.Criterion;

using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Interfaces
{
    [ContractClass(typeof(IRepositoryContract<,>))]
    public interface IRepository<T, in TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        ///     Gets an entity by its id.
        /// </summary>
        /// <param name="id">The given primary key.</param>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para>
        ///         An <see cref="IdNotFoundException{T,TId}" /> is thrown when there is no record with the given
        ///         <paramref name="id" /> in the DB. For this exception, it applies that
        ///         <code><see cref="IdNotFoundException{T,TId}" />.<see cref="IdNotFoundException{T,TId}.Id" /> == id</code>.
        ///     </para>
        /// </remarks>
        /// <returns>The entity with the given id.</returns>
        T GetById(TId id);

        T Get(IEnumerable<ICriterion> criteria);

        /// <summary>
        ///     Same functionality as <see cref="Get(System.Collections.Generic.IEnumerable{ICriterion})" />
        ///     but you have the ability to use a pessimistic lock on the database of the fetched rows.
        /// </summary>
        /// <param name="criteria">List of criteria.</param>
        /// <param name="lockMode">Locking mode while creating the query.</param>
        /// <returns>The (only) record that satisfies the given <paramref name="criteria" />.</returns>
        T Get(IEnumerable<ICriterion> criteria, LockMode lockMode);

        /// <summary>
        ///     Find the records complying with the given criteria and in the given order.
        /// </summary>
        /// <param name="criteria">The given criteria.</param>
        /// <param name="orders">The given ordering algorithm.</param>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para>All elements of the resulting set fulfill <paramref name="criteria" />.</para>
        ///     <para>The elements are ordered in the resulting set according to <paramref name="orders" />.</para>
        /// </remarks>
        /// <returns>
        ///     A list of the records satisfying the given <paramref name="criteria" />, and in the given
        ///     <paramref name="orders" />.
        /// </returns>
        IList<T> Find(IEnumerable<ICriterion> criteria, IEnumerable<Order> orders);

        /// <summary>
        ///     Same functionality as
        ///     <see
        ///         cref="Find(System.Collections.Generic.IEnumerable{ICriterion},System.Collections.Generic.IEnumerable{Order})" />
        ///     but you have the ability to use a pessimistic lock on the database of the fetched rows.
        /// </summary>
        /// <param name="criteria">List of criteria.</param>
        /// <param name="orders">Sequence of expressions that will be used to sort the result, order is important.</param>
        /// <param name="lockMode">The locking mode.</param>
        /// <returns>
        ///     A list of the records satisfying the given <paramref name="criteria" />, and in the given
        ///     <paramref name="orders" />.
        /// </returns>
        IList<T> Find(IEnumerable<ICriterion> criteria, IEnumerable<Order> orders, LockMode lockMode);

        IPagedList<T> FindPaged(int pageIndex, int pageSize, IEnumerable<ICriterion> criterions, IEnumerable<Order> orders);

        /// <summary>
        ///     A record is saved in the DB to represent <paramref name="entity" />.
        ///     An object is returned that represents the new record.
        /// </summary>
        /// <param name="entity">The given entity.</param>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para>
        ///         <see cref="GetById" /> returns an object that is "the same" as the result of this method.
        ///         It has the same id, but the contents can be different, because the DB might be changed in the mean time.
        ///     </para>
        /// </remarks>
        /// <returns>The saved entity.</returns>
        T Save(T entity);

        /// <summary>
        ///     Update a given entity.
        /// </summary>
        /// <param name="entity">The given entity.</param>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para>
        ///         An <see cref="IdNotFoundException{T,TId}" /> is thrown when there is no record with the given
        ///         <code><paramref name="entity" />.<see cref="IIdentity{T}.Id" /></code> in the DB. For this exception, it
        ///         applies that
        ///         <code><see cref="IdNotFoundException{T,TId}" />.<see cref="IdNotFoundException{T,TId}.Id" /> == id</code>.
        ///     </para>
        /// </remarks>
        /// <returns>The updated entity.</returns>
        T Update(T entity);

        /// <summary>
        ///     The record that represents <paramref name="entity" /> is deleted from the DB.
        /// </summary>
        /// <param name="entity">The given entity.</param>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para><see cref="GetById" /> throws an <see cref="IdNotFoundException{T,TId}" />.</para>
        /// </remarks>
        void Delete(T entity);
    }

    // ReSharper disable InconsistentNaming
    // ReSharper disable PossibleNullReferenceException
    [ContractClassFor(typeof(IRepository<,>))]
    public abstract class IRepositoryContract<T, TId> : IRepository<T, TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        public T GetById(TId id)
        {
            Contract.Ensures(Contract.Result<T>() != null);
            Contract.Ensures(EqualityComparer<TId>.Default.Equals(Contract.Result<T>().Id, id));
            Contract.Ensures(!Contract.Result<T>().IsTransient);
            Contract.EnsuresOnThrow<IdNotFoundException<T, TId>>(true);

            return default(T);
        }

        public T Get(IEnumerable<ICriterion> criteria)
        {
            Contract.Ensures(!Contract.Result<T>().IsTransient);

            // The result should match the criteria, but this is not easy to express in contracts
            // Contract.Ensures(Contract.Result<T>() == null || criteria.Matches(Contract.Result<T>()));
            Contract.Ensures(Contract.Result<T>() != null);
            Contract.EnsuresOnThrow<NotFoundException>(true, "no object in the DB matches the criteria");

            return default(T);
        }

        public T Get(IEnumerable<ICriterion> criteria, LockMode lockMode)
        {
            Contract.Ensures(!Contract.Result<T>().IsTransient);
            Contract.Ensures(Contract.Result<T>() != null);
            Contract.EnsuresOnThrow<NotFoundException>(true, "no object in the DB matches the criteria");

            return default(T);
        }

        public IList<T> Find(IEnumerable<ICriterion> criteria, IEnumerable<Order> orders)
        {
            Contract.Ensures(Contract.Result<IList<T>>() != null);

            return default(IList<T>);
        }

        public IList<T> Find(IEnumerable<ICriterion> criteria, IEnumerable<Order> orders, LockMode lockMode)
        {
            Contract.Ensures(Contract.Result<IList<T>>() != null);

            return default(IList<T>);
        }

        public IPagedList<T> FindPaged(int pageIndex, int pageSize, IEnumerable<ICriterion> criterions, IEnumerable<Order> orders)
        {
            Contract.Requires(pageIndex > 0);
            Contract.Requires(pageSize > 0);
            Contract.Ensures(Contract.Result<IPagedList<T>>() != null);

            return default(IPagedList<T>);
        }

        public T Save(T entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.IsTransient);
            Contract.Ensures(Contract.Result<T>() != null);
            Contract.Ensures(!Contract.Result<T>().IsTransient);
            Contract.Ensures(!EqualityComparer<TId>.Default.Equals(Contract.Result<T>().Id, default(TId)));

            return default(T);
        }

        public T Update(T entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(!entity.IsTransient);
            Contract.Ensures(Contract.Result<T>() != null);
            Contract.Ensures(!Contract.Result<T>().IsTransient);
            Contract.Ensures(EqualityComparer<TId>.Default.Equals(Contract.Result<T>().Id, entity.Id));
            Contract.EnsuresOnThrow<NotFoundException>(true, "entity does not exist in the DB");

            return default(T);
        }

        public void Delete(T entity)
        {
            Contract.Requires(entity != null);
        }
    }
}