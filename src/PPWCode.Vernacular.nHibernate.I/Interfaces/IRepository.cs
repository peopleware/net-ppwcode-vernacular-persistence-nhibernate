using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

using NHibernate.Criterion;

using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.nHibernate.I.Interfaces
{
    [ContractClass(typeof(IRepositoryContract<,>))]
    public interface IRepository<T, in TId> : IReadonlyRepository<T, TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
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

        public abstract T GetById(TId id);
        public abstract Iesi.Collections.Generic.ISet<T> Find(IEnumerable<ICriterion> criterions = null, IEnumerable<Order> orders = null);
        public abstract IPagedList<T> FindPaged(int pageIndex, int pageSize, IEnumerable<ICriterion> criterions = null, IEnumerable<Order> orders = null);
        public abstract TProperty GetPropertyValue<TProperty>(T entity, Expression<Func<TProperty>> propertyExpression);
        public abstract TProperty GetPropertyValue<TProperty>(T entity, string propertyName);
        public abstract Iesi.Collections.Generic.ISet<TProperty> GetChildren<TProperty>(T entity, Expression<Func<TProperty>> propertyExpression) where TProperty : IIdentity<TId>;
        public abstract Iesi.Collections.Generic.ISet<TProperty> GetChildren<TProperty>(T entity, string propertyName) where TProperty : IIdentity<TId>;
    }
}