// Copyright 2015 by PeopleWare n.v..
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
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Transactions;

using Castle.Core.Logging;

using NHibernate;
using NHibernate.Exceptions;

using PPWCode.Util.OddsAndEnds.II.Extensions;
using PPWCode.Vernacular.Exceptions.II;
using PPWCode.Vernacular.Persistence.II;

using IsolationLevel = System.Data.IsolationLevel;

namespace PPWCode.Vernacular.NHibernate.I.Implementations
{
    public abstract class RepositoryBase<T, TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        private readonly ISession m_Session;
        private ILogger m_Logger = NullLogger.Instance;

        protected RepositoryBase(ISession session)
        {
            Contract.Requires(session != null);
            Contract.Ensures(Session == session);

            m_Session = session;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Logger != null);
            Contract.Invariant(Session != null);
        }

        public ILogger Logger
        {
            get
            {
                Contract.Ensures(Contract.Result<ILogger>() != null);

                return m_Logger;
            }

            set
            {
                Contract.Requires(value != null);
                Contract.Ensures(value == Logger);

                m_Logger = value;
            }
        }

        public ISession Session
        {
            get
            {
                Contract.Ensures(Contract.Result<ISession>() != null);

                return m_Session;
            }
        }

        protected abstract IsolationLevel IsolationLevel { get; }

        protected virtual void Execute(string requestDescription, Action action)
        {
            Contract.Requires(!string.IsNullOrEmpty(requestDescription));
            Contract.Requires(action != null);

            Execute(requestDescription, action, null);
        }

        protected virtual void Execute(string requestDescription, Action action, T entity)
        {
            Contract.Requires(!string.IsNullOrEmpty(requestDescription));
            Contract.Requires(action != null);

            EnsureTransaction(() => EnsureControlledEnvironment(requestDescription, action, entity));
        }

        protected virtual TResult Execute<TResult>(string requestDescription, Func<TResult> func)
        {
            Contract.Requires(!string.IsNullOrEmpty(requestDescription));
            Contract.Requires(func != null);

            return Execute(requestDescription, func, null);
        }

        protected virtual TResult Execute<TResult>(string requestDescription, Func<TResult> func, T entity)
        {
            Contract.Requires(!string.IsNullOrEmpty(requestDescription));
            Contract.Requires(func != null);

            return EnsureTransaction(() => EnsureControlledEnvironment(requestDescription, func, entity));
        }

        protected virtual void EnsureTransaction(Action action)
        {
            Contract.Requires(action != null);

            EnsureTransaction(
                () =>
                {
                    action.Invoke();
                    return default(int);
                });
        }

        protected virtual TResult EnsureTransaction<TResult>(Func<TResult> func)
        {
            Contract.Requires(func != null);

            if (Session.Transaction.IsActive || Transaction.Current != null)
            {
                return func.Invoke();
            }

            TResult result;
            ITransaction transaction = Session.BeginTransaction(IsolationLevel);
            try
            {
                result = func.Invoke();
                transaction.Commit();
                transaction.Dispose();
            }
            catch
            {
                transaction.Rollback();
                transaction.Dispose();
                throw;
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
            Contract.Requires(exception != null);
            Contract.Requires(!string.IsNullOrEmpty(message));

            Exception result;

            Logger.Debug(message, exception);
            GenericADOException genericAdoException = exception as GenericADOException;
            if (genericAdoException != null)
            {
                RepositorySqlException repositorySqlException =
                    new RepositorySqlException(message, genericAdoException.InnerException)
                    {
                        SqlString = genericAdoException.SqlString
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

        protected virtual void EnsureControlledEnvironment(string requestDescription, Action action, T entity)
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

        protected virtual TResult EnsureControlledEnvironment<TResult>(string requestDescription, Func<TResult> func, T entity)
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
                Exception triagedException = TriageException(he, msg);
                if (triagedException != null)
                {
                    throw triagedException;
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