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
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;

using Castle.Core.Logging;

using NHibernate;
using NHibernate.Criterion;
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
            Contract.Requires<ArgumentNullException>(logger != null);
            Contract.Requires<ArgumentNullException>(session != null);

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
            return RunFunctionInsideATransaction(() => GetByIdInternal(id));
        }

        public T Get(IEnumerable<ICriterion> criteria)
        {
            return RunFunctionInsideATransaction(() => GetInternal(criteria));
        }

        public T Get(IEnumerable<ICriterion> criteria, LockMode lockMode)
        {
            return RunFunctionInsideATransaction(() => GetInternal(criteria));
        }

        public virtual IList<T> Find(IEnumerable<ICriterion> criteria, IEnumerable<Order> orders)
        {
            return RunFunctionInsideATransaction(() => FindInternal(criteria, orders));
        }

        public IList<T> Find(IEnumerable<ICriterion> criteria, IEnumerable<Order> orders, LockMode lockMode)
        {
            return RunFunctionInsideATransaction(() => FindInternal(criteria, orders, lockMode));
        }

        public virtual IPagedList<T> FindPaged(int pageIndex, int pageSize, IEnumerable<ICriterion> criterions, IEnumerable<Order> orders)
        {
            return RunFunctionInsideATransaction(() => FindPagedInternal(pageIndex, pageSize, criterions, orders));
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

        protected virtual T GetByIdInternal(TId id)
        {
            return RunControlledFunction(
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

        protected virtual T GetInternal(IEnumerable<ICriterion> criteria)
        {
            return RunControlledFunction(
                "GetInternal",
                () =>
                {
                    ICriteria qry = CreateCriteria(criteria, null);
                    T result = qry.UniqueResult<T>();
                    if (result == null)
                    {
                        throw new NotFoundException();
                    }

                    return result;
                });
        }

        protected virtual T GetInternal(IEnumerable<ICriterion> criteria, LockMode lockMode)
        {
            return RunControlledFunction(
                "GetInternal",
                () =>
                {
                    ICriteria qry = CreateCriteria(criteria, null);
                    T result = qry.SetLockMode(lockMode).UniqueResult<T>();
                    if (result == null)
                    {
                        throw new NotFoundException();
                    }

                    return result;
                });
        }

        protected virtual IList<T> FindInternal(IEnumerable<ICriterion> criteria, IEnumerable<Order> orders)
        {
            return RunControlledFunction(
                "FindInternal",
                () =>
                {
                    ICriteria qry = CreateCriteria(criteria, orders);
                    IList<T> result = qry.List<T>();
                    return result;
                });
        }

        protected virtual IList<T> FindInternal(IEnumerable<ICriterion> criteria, IEnumerable<Order> orders, LockMode lockMode)
        {
            return RunControlledFunction(
                "FindInternal",
                () =>
                {
                    ICriteria qry = CreateCriteria(criteria, orders).SetLockMode(lockMode);
                    IList<T> result = qry.List<T>();
                    return result;
                });
        }

        protected virtual PagedList<T> FindPagedInternal(int pageIndex, int pageSize, IEnumerable<ICriterion> criteria, IEnumerable<Order> orders)
        {
            return RunControlledFunction(
                "FindPagedInternal",
                () =>
                {
                    ICriteria rowCountQry = CreateCriteria(criteria, null);
                    IFutureValue<int> rowCount = rowCountQry
                        .SetProjection(Projections.RowCount())
                        .FutureValue<int>();

                    ICriteria qry = CreateCriteria(criteria, orders);
                    if (orders == null || !orders.Any())
                    {
                        qry.AddOrder(Order.Asc(Projections.Id()));
                    }

                    IList<T> qryResult = qry
                        .SetFirstResult((pageIndex - 1) * pageSize)
                        .SetMaxResults(pageSize)
                        .Future<T>()
                        .ToList<T>();

                    PagedList<T> result = new PagedList<T>(qryResult, pageIndex, pageSize, rowCount.Value);
                    return result;
                });
        }

        protected virtual ICriteria CreateCriteria(IEnumerable<ICriterion> criteria, IEnumerable<Order> orders)
        {
            ICriteria result = Session.CreateCriteria<T>();
            if (criteria != null)
            {
                foreach (ICriterion criterion in criteria)
                {
                    result.Add(criterion);
                }
            }

            if (orders != null)
            {
                foreach (Order order in orders)
                {
                    result.AddOrder(order);
                }
            }

            return result;
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

        protected virtual void RunActionInsideATransaction(Action action)
        {
            Contract.Requires<ArgumentNullException>(action != null);

            RunFunctionInsideATransaction(
                () =>
                {
                    action.Invoke();
                    return default(int);
                });
        }

        protected virtual TResult RunFunctionInsideATransaction<TResult>(Func<TResult> func)
        {
            Contract.Requires<ArgumentNullException>(func != null);

            if (Session.Transaction.IsActive)
            {
                return func.Invoke();
            }

            TResult result;
            using (ITransaction transaction = Session.BeginTransaction())
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
        /// <returns>An exception that is a sub class either from <see cref="SemanticException"/> or from <see cref="Error"/>.</returns>
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

        protected virtual void RunControlledAction(string requestDescription, Action action, T entity = null)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(requestDescription));
            Contract.Requires<ArgumentNullException>(action != null);

            RunControlledFunction(
                requestDescription,
                () =>
                {
                    action.Invoke();
                    return default(int);
                },
                entity);
        }

        protected virtual TResult RunControlledFunction<TResult>(string requestDescription, Func<TResult> func, T entity = null)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(requestDescription));
            Contract.Requires<ArgumentNullException>(func != null);

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