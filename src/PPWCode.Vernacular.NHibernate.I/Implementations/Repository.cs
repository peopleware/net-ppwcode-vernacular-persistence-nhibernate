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
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;

using Castle.Core.Logging;

using NHibernate;
using NHibernate.Criterion;
using NHibernate.Engine;
using NHibernate.Exceptions;

using PPWCode.Util.OddsAndEnds.II.Extensions;
using PPWCode.Vernacular.Exceptions.II;
using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Implementations
{
    public abstract class Repository<T, TId> : IRepository<T, TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        private readonly ILogger m_Logger;
        private readonly ISession m_Session;

        protected Repository(ILogger logger, ISession session)
        {
            Contract.Requires(logger != null);
            Contract.Requires(session != null);

            m_Logger = logger;
            m_Session = session;
        }

        protected ILogger Logger
        {
            get { return m_Logger; }
        }

        protected ISession Session
        {
            get { return m_Session; }
        }

        public virtual T GetById(TId id)
        {
            return EnsureNhTransaction(() => GetByIdInternal(id));
        }

        public T Get(Func<ICriteria, ICriteria> func)
        {
            return EnsureNhTransaction(() => GetInternal(func));
        }

        public T Get(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return EnsureNhTransaction(() => GetInternal(func));
        }

        public virtual IList<T> Find(Func<ICriteria, ICriteria> func)
        {
            return EnsureNhTransaction(() => FindInternal(func));
        }

        public IList<T> Find(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return EnsureNhTransaction(() => FindInternal(func));
        }

        public virtual IPagedList<T> FindPaged(int pageIndex, int pageSize, Func<ICriteria, ICriteria> func)
        {
            return EnsureNhTransaction(() => FindPagedInternal(pageIndex, pageSize, func));
        }

        public IPagedList<T> FindPaged(int pageIndex, int pageSize, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return EnsureNhTransaction(() => FindPagedInternal(pageIndex, pageSize, func));
        }

        public virtual T Save(T entity)
        {
            return EnsureNhTransaction(() => SaveInternal(entity));
        }

        public virtual void Delete(T entity)
        {
            EnsureNhTransaction(() => DeleteInternal(entity));
        }

        protected virtual T GetByIdInternal(TId id)
        {
            return EnsureControlledEnvironment(
                "GetByIdInternal",
                () =>
                {
                    T result = Session.Get<T>(id);
                    if (result == null)
                    {
                        throw new IdNotFoundException<T, TId>(id);
                    }

                    return result;
                });
        }

        protected virtual T GetInternal(Func<ICriteria, ICriteria> func)
        {
            return EnsureControlledEnvironment(
                "GetInternal",
                () =>
                {
                    ICriteria qry = func(CreateCriteria());
                    T result = qry.UniqueResult<T>();
                    if (result == null)
                    {
                        throw new NotFoundException();
                    }

                    return result;
                });
        }

        protected virtual T GetInternal(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return EnsureControlledEnvironment(
                "GetInternal",
                () =>
                {
                    IQueryOver<T> qry = func(CreateQueryOver());
                    T result = qry.SingleOrDefault();
                    if (result == null)
                    {
                        throw new NotFoundException();
                    }

                    return result;
                });
        }

        protected virtual IList<T> FindInternal(Func<ICriteria, ICriteria> func)
        {
            return EnsureControlledEnvironment(
                "FindInternal",
                () =>
                {
                    ICriteria qry = func(CreateCriteria());
                    IList<T> result = qry.List<T>();
                    return result;
                });
        }

        protected virtual IList<T> FindInternal(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return EnsureControlledEnvironment(
                "FindInternal",
                () =>
                {
                    IQueryOver<T> qry = func(CreateQueryOver());
                    IList<T> result = qry.List<T>();
                    return result;
                });
        }

        protected virtual PagedList<T> FindPagedInternal(int pageIndex, int pageSize, Func<ICriteria, ICriteria> func)
        {
            return EnsureControlledEnvironment(
                "FindPagedInternal",
                () =>
                {
                    ICriteria qryRowCount = func(CreateCriteria());
                    qryRowCount.ClearOrders();

                    IFutureValue<int> rowCount = qryRowCount
                        .SetFirstResult(0)
                        .SetMaxResults(RowSelection.NoValue)
                        .SetProjection(Projections.RowCount())
                        .FutureValue<int>();

                    IList<T> qryResult = func(CreateCriteria())
                        .SetFirstResult((pageIndex - 1) * pageSize)
                        .SetMaxResults(pageSize)
                        .Future<T>()
                        .ToList<T>();

                    PagedList<T> result = new PagedList<T>(qryResult, pageIndex, pageSize, rowCount.Value);
                    return result;
                });
        }

        protected virtual PagedList<T> FindPagedInternal(int pageIndex, int pageSize, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return EnsureControlledEnvironment(
                "FindPagedInternal",
                () =>
                {
                    IQueryOver<T> queryOver = func(CreateQueryOver());

                    IFutureValue<int> rowCount = queryOver
                        .ToRowCountQuery()
                        .FutureValue<int>();

                    IList<T> qryResult = queryOver
                        .Skip((pageIndex - 1) * pageSize)
                        .Take(pageSize)
                        .Future<T>()
                        .ToList<T>();

                    PagedList<T> result = new PagedList<T>(qryResult, pageIndex, pageSize, rowCount.Value);
                    return result;
                });
        }

        protected virtual T SaveInternal(T entity)
        {
            return EnsureControlledEnvironment("SaveInternal", () => Session.Merge(entity), entity);
        }

        protected virtual void DeleteInternal(T entity)
        {
            EnsureControlledEnvironment(
                "DeleteInternal",
                () =>
                {
                    if (!entity.IsTransient)
                    {
                        Session.Delete(entity);
                    }
                },
                entity);
        }

        protected virtual ICriteria CreateCriteria()
        {
            return Session.CreateCriteria<T>();
        }

        protected virtual IQueryOver<T, T> CreateQueryOver()
        {
            return Session.QueryOver<T>();
        }

        protected virtual void EnsureNhTransaction(Action action)
        {
            Contract.Requires(action != null);

            EnsureNhTransaction(
                () =>
                {
                    action.Invoke();
                    return default(int);
                });
        }

        protected virtual TResult EnsureNhTransaction<TResult>(Func<TResult> func)
        {
            Contract.Requires(func != null);

            if (Session.Transaction.IsActive)
            {
                return func.Invoke();
            }

            TResult result;
            using (ITransaction transaction = Session.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                result = func.Invoke();
                transaction.Commit();
            }

            return result;
        }

        /// <summary>
        ///     This method *has* to convert whatever NHibernate exception to a valid PPWCode exception
        ///     Some hibernate exceptions might be semantic, some might be errors.
        ///     This may depend on the actual product.
        ///     This method translates semantic exceptions in PPWCode.Util.Exception.SemanticException and throws them
        ///     and all other exceptions in PPWCode.Util.Exception.Error and throws them.
        /// </summary>
        /// <param name="exception">The hibernate exception we are triaging.</param>
        /// <param name="message">This message will be used in the logging in the case aException = Error.</param>
        /// <returns>An exception that is a sub class either from <see cref="SemanticException" /> or from <see cref="Error" />.</returns>
        protected virtual Exception TriageException(Exception exception, string message)
        {
            Exception result;

            Logger.Debug(message, exception);
            GenericADOException genericAdoException = exception as GenericADOException;
            if (genericAdoException != null)
            {
                RepositorySqlException repositorySqlException =
                    new RepositorySqlException(message, genericAdoException.InnerException)
                    {
                        SqlString = genericAdoException.SqlString,
                    };
                SqlException sqlException = genericAdoException.InnerException as SqlException;
                if (sqlException != null)
                {
                    repositorySqlException.Constraint = sqlException.GetConstraint();
                }

                result = repositorySqlException;
            }
            else
            {
                result = new ExternalError(message, exception);
            }

            throw result;
        }

        protected virtual void EnsureControlledEnvironment(string requestDescription, Action action, T entity = null)
        {
            Contract.Requires(!string.IsNullOrEmpty(requestDescription));
            Contract.Requires(action != null);

            EnsureControlledEnvironment(
                requestDescription,
                () =>
                {
                    action.Invoke();
                    return default(int);
                },
                entity);
        }

        protected virtual TResult EnsureControlledEnvironment<TResult>(string requestDescription, Func<TResult> func, T entity = null)
        {
            Contract.Requires(!string.IsNullOrEmpty(requestDescription));
            Contract.Requires(func != null);

            if (Logger.IsInfoEnabled)
            {
                string msg =
                    entity != null
                        ? string.Format(@"Request {0} for class {1}, entity={2} started", requestDescription, typeof(T).Name, entity)
                        : string.Format(@"Request {0} for class {1} started", requestDescription, typeof(T).Name);
                Logger.Info(msg);
            }

            TResult result;
            try
            {
                result = func.Invoke();
            }
            catch (StaleObjectStateException sose)
            {
                string errmsg = string.Format(
                    @"Object already changed for request {0}, class {1}, {2}",
                    requestDescription,
                    typeof(T).Name,
                    entity != null ? entity.ToString() : string.Empty);
                Logger.Debug(errmsg, sose);
                throw new ObjectAlreadyChangedException(entity);
            }
            catch (HibernateException he)
            {
                string msg =
                    entity != null
                        ? string.Format(@"Request {0} for class {1}, entity={2} failed", requestDescription, typeof(T).Name, entity)
                        : string.Format(@"Request {0} for class {1} failed", requestDescription, typeof(T).Name);
                Exception e = TriageException(he, msg);
                if (e != null)
                {
                    throw e;
                }

                throw;
            }

            if (Logger.IsInfoEnabled)
            {
                string msg =
                    entity != null
                        ? string.Format(@"Request {0} for class {1}, entity={2} finished", requestDescription, typeof(T).Name, entity)
                        : string.Format(@"Request {0} for class {1} finised", requestDescription, typeof(T).Name);
                Logger.Info(msg);
            }

            return result;
        }
    }
}