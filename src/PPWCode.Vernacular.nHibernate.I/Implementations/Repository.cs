using System;

using Castle.Core.Logging;

using NHibernate;

using PPWCode.Vernacular.nHibernate.I.Interfaces;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.nHibernate.I.Implementations
{
    public abstract class Repository<T, TId> :
        ReadonlyRepository<T, TId>,
        IRepository<T, TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        protected Repository(ILogger logger, ISession session)
            : base(logger, session)
        {
        }

        public virtual T Save(T entity)
        {
            return RunFunctionInsideATransaction(() => SaveInternal(entity));
        }

        public virtual T Update(T entity)
        {
            return RunFunctionInsideATransaction(() => UpdateInternal(entity));
        }

        public virtual void Delete(T entity)
        {
            RunActionInsideATransaction(() => DeleteInternal(entity));
        }

        protected virtual T SaveInternal(T entity)
        {
            return RunControlledFunction(
                "SaveInternal",
                () =>
                {
                    Session.Save(entity);
                    return entity;
                },
                entity);
        }

        protected virtual T UpdateInternal(T entity)
        {
            return RunControlledFunction(
                "UpdateInternal",
                () =>
                {
                    T result = Session.Merge(entity);
                    return result;
                },
                entity);
        }

        protected virtual void DeleteInternal(T entity)
        {
            RunControlledAction("DeleteInternal", () => Session.Delete(entity), entity);
        }
    }
}