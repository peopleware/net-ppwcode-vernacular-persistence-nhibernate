using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using NHibernate;
using NHibernate.Criterion;

using PPWCode.Vernacular.Persistence.II;
using PPWCode.Vernacular.Persistence.II.Exceptions;

namespace PPWCode.Vernacular.nHibernate.I.Interfaces
{
    [ContractClass(typeof(IRepositoryContract<,>))]
    public interface IRepository<T, in TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        T GetById(TId id);
        T Get(IEnumerable<ICriterion> criterions, IEnumerable<Order> orders);
        T Get(IEnumerable<ICriterion> criterions, IEnumerable<Order> orders, LockMode lockMode);
        Iesi.Collections.Generic.ISet<T> Find(IEnumerable<ICriterion> criterions, IEnumerable<Order> orders);
        Iesi.Collections.Generic.ISet<T> Find(IEnumerable<ICriterion> criterions, IEnumerable<Order> orders, LockMode lockMode);
        IPagedList<T> FindPaged(int pageIndex, int pageSize, IEnumerable<ICriterion> criterions, IEnumerable<Order> orders);
        T Save(T entity);
        T Update(T entity);
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
            Contract.EnsuresOnThrow<IdNotFoundException<T, TId>>(true /* The ID cannot be found inside the store*/);

            return default(T);
        }

        public T Get(IEnumerable<ICriterion> criterions, IEnumerable<Order> orders)
        {
            return default(T);
        }

        public T Get(IEnumerable<ICriterion> criterions, IEnumerable<Order> orders, LockMode lockMode)
        {
            return default(T);
        }

        public Iesi.Collections.Generic.ISet<T> Find(IEnumerable<ICriterion> criterions, IEnumerable<Order> orders)
        {
            Contract.Ensures(Contract.Result<Iesi.Collections.Generic.ISet<T>>() != null);

            return default(Iesi.Collections.Generic.ISet<T>);
        }

        public Iesi.Collections.Generic.ISet<T> Find(IEnumerable<ICriterion> criterions, IEnumerable<Order> orders, LockMode lockMode)
        {
            Contract.Ensures(Contract.Result<Iesi.Collections.Generic.ISet<T>>() != null);

            return default(Iesi.Collections.Generic.ISet<T>);
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

            return default(T);
        }

        public T Update(T entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(!entity.IsTransient);
            Contract.Ensures(Contract.Result<T>() != null);

            return default(T);
        }

        public void Delete(T entity)
        {
            Contract.Requires(entity != null);
        }
    }
}