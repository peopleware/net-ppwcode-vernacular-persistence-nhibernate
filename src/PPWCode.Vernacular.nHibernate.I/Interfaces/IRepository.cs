using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using NHibernate;
using NHibernate.Criterion;

using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.nHibernate.I.Interfaces
{
    [ContractClass(typeof(IRepositoryContract<,>))]
    public interface IRepository<T, in TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// <h3>Extra postconditions</h3>
        /// <para>An <see cref="IdNotFoundException{T,TId}"/> is thrown when there is no record with the given
        /// <paramref name="id"/> in the DB. For this exception, it applies that
        /// <code><see cref="IdNotFoundException{T,TId}"/>.<see cref="IdNotFoundException{T,TId}.Id"/> == id</code>.</para>
        /// </remarks>
        T GetById(TId id);

        T Get(IEnumerable<ICriterion> criteria);

        /// <summary>
        /// Same functionality as <see cref="Get(System.Collections.Generic.IEnumerable{NHibernate.Criterion.ICriterion})"/>
        /// but you have the ability to use a pessimistic lock on the database of the fetched rows.
        /// </summary>
        /// <param name="criteria">List of criteria</param>
        /// <param name="lockMode"></param>
        /// <returns></returns>
        T Get(IEnumerable<ICriterion> criteria, LockMode lockMode);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// <h3>Extra postconditions</h3>
        /// <para>All elements of the resulting set fulfill <paramref name="criteria"/>.</para>
        /// <para>The elements are ordered in the resulting set according to <paramref name="orders"/>.</para>
        /// </remarks>
        IList<T> Find(IEnumerable<ICriterion> criteria, IEnumerable<Order> orders);

        /// <summary>
        /// Same functionality as <see cref="Find(System.Collections.Generic.IEnumerable{NHibernate.Criterion.ICriterion},System.Collections.Generic.IEnumerable{NHibernate.Criterion.Order})"/>
        /// but you have the ability to use a pessimistic lock on the database of the fetched rows.
        /// </summary>
        /// <param name="criteria">List of criterions</param>
        /// <param name="orders">Sequence of expressions that will be used to sort the result, order is important</param>
        /// <param name="lockMode"></param>
        /// <returns></returns>
        IList<T> Find(IEnumerable<ICriterion> criteria, IEnumerable<Order> orders, LockMode lockMode);

        IPagedList<T> FindPaged(int pageIndex, int pageSize, IEnumerable<ICriterion> criterions, IEnumerable<Order> orders);

        /// <summary>
        /// A record is saved in the DB to represent <paramref name="entity"/>.
        /// An object is returned that represents the new record.
        /// </summary>
        /// <remarks>
        /// <h3>Extra postconditions</h3>
        /// <para>GetById(Contract.Result<T>().Id) returns an object that is "the same" as the result of this method.
        /// It has the same id, but the contents can be different, because the DB might be changed in the mean time.</para>
        /// </remarks>
        T Save(T entity);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// <h3>Extra postconditions</h3>
        /// <para>An <see cref="IdNotFoundException{T,TId}"/> is thrown when there is no record with the given
        /// <code><paramref name="entity"/>.<see cref="IIdentity{T}.Id"/></code> in the DB. For this exception, it applies that
        /// <code><see cref="IdNotFoundException{T,TId}"/>.<see cref="IdNotFoundException{T,TId}.Id"/> == id</code>.</para>
        /// </remarks>
        T Update(T entity);

        /// <summary>
        /// The record that represents <paramref name="entity"/> is deleted from the DB.
        /// </summary>
        /// <remarks>
        /// <h3>Extra postconditions</h3>
        /// <para>GetById(entity.Id) throws an IdNotFoundException.</para>
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
            //Contract.Ensures(Contract.Result<T>() == null || criteria.Matches(Contract.Result<T>()));
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