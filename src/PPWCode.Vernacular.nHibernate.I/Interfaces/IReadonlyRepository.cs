using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

using NHibernate.Criterion;

using PPWCode.Vernacular.Persistence.II;
using PPWCode.Vernacular.Persistence.II.Exceptions;

namespace PPWCode.Vernacular.nHibernate.I.Interfaces
{
    [ContractClass(typeof(IReadonlyRepositoryContract<,>))]
    public interface IReadonlyRepository<T, TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        T GetById(TId id);
        ISet<T> Find(IEnumerable<ICriterion> criterions = null, IEnumerable<Order> orders = null);
        IPagedList<T> FindPaged(int pageIndex, int pageSize, IEnumerable<ICriterion> criterions = null, IEnumerable<Order> orders = null);
        TProperty GetPropertyValue<TProperty>(T entity, Expression<Func<TProperty>> propertyExpression);
        TProperty GetPropertyValue<TProperty>(T entity, string propertyName);
        ISet<TProperty> GetChildren<TProperty>(T entity, Expression<Func<TProperty>> propertyExpression) where TProperty : IIdentity<TId>;
        ISet<TProperty> GetChildren<TProperty>(T entity, string propertyName) where TProperty : IIdentity<TId>;
    }

    // ReSharper disable InconsistentNaming
    // ReSharper disable PossibleNullReferenceException
    [ContractClassFor(typeof(IReadonlyRepository<,>))]
    public abstract class IReadonlyRepositoryContract<T, TId> : IReadonlyRepository<T, TId>
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

        public ISet<T> Find(IEnumerable<ICriterion> criterions, IEnumerable<Order> orders)
        {
            Contract.Ensures(Contract.Result<ISet<T>>() != null);

            return default(ISet<T>);
        }

        public IPagedList<T> FindPaged(int pageIndex, int pageSize, IEnumerable<ICriterion> criterions, IEnumerable<Order> orders)
        {
            Contract.Requires(pageIndex > 0);
            Contract.Requires(pageSize > 0);
            Contract.Ensures(Contract.Result<IPagedList<T>>() != null);

            return default(IPagedList<T>);
        }

        public TProperty GetPropertyValue<TProperty>(T entity, Expression<Func<TProperty>> propertyExpression)
        {
            Contract.Requires(entity != null);
            Contract.Requires(propertyExpression != null);

            return default(TProperty);
        }

        public TProperty GetPropertyValue<TProperty>(T entity, string propertyName)
        {
            Contract.Requires(entity != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            return default(TProperty);
        }

        public ISet<TProperty> GetChildren<TProperty>(T entity, Expression<Func<TProperty>> propertyExpression)
            where TProperty : IIdentity<TId>
        {
            Contract.Requires(entity != null);
            Contract.Requires(propertyExpression != null);
            Contract.Ensures(Contract.Result<ISet<TProperty>>() != null);

            return default(ISet<TProperty>);
        }

        public ISet<TProperty> GetChildren<TProperty>(T entity, string propertyName)
            where TProperty : IIdentity<TId>
        {
            return default(ISet<TProperty>);
        }
    }
}