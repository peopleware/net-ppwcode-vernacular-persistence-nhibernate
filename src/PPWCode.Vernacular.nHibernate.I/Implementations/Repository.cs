using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Castle.Core.Logging;

using NHibernate;
using NHibernate.Criterion;
using NHibernate.Exceptions;

using PPWCode.Util.OddsAndEnds.II.Extensions;
using PPWCode.Vernacular.Exceptions.II;
using PPWCode.Vernacular.nHibernate.I.Interfaces;
using PPWCode.Vernacular.Persistence.II;
using PPWCode.Vernacular.Persistence.II.Exceptions;

namespace PPWCode.Vernacular.nHibernate.I.Implementations
{
    public abstract class Repository<T, TId> : IRepository<T, TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        private readonly ILogger m_Logger;
        private readonly ISession m_Session;

        protected Repository(ILogger logger, ISession session)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }
            Contract.EndContractBlock();

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

        public virtual ISet<T> Get(IEnumerable<ICriterion> criterions = null, IEnumerable<Order> orders = null)
        {
            return RunFunctionInsideATransaction(() => GetInternal(criterions, orders));
        }

        public virtual IPagedList<T> GetPaged(int pageIndex, int pageSize, IEnumerable<ICriterion> criterions = null, IEnumerable<Order> orders = null)
        {
            return RunFunctionInsideATransaction(() => GetPagedInternal(pageIndex, pageSize, criterions, orders));
        }

        public virtual TProperty GetPropertyValue<TProperty>(T entity, Expression<Func<TProperty>> propertyExpression)
        {
            return GetPropertyValue<TProperty>(entity, GetPropertyName(propertyExpression));
        }

        public virtual TProperty GetPropertyValue<TProperty>(T entity, string propertyName)
        {
            VerifyPropertyName<T>(propertyName);
            return RunFunctionInsideATransaction(() => GetPropertyValueInternal<TProperty>(entity, propertyName));
        }

        public virtual ISet<TProperty> GetChildren<TProperty>(T entity, Expression<Func<TProperty>> propertyExpression)
            where TProperty : IIdentity<TId>
        {
            return GetChildren<TProperty>(entity, GetPropertyName(propertyExpression));
        }

        public virtual ISet<TProperty> GetChildren<TProperty>(T entity, string propertyName)
            where TProperty : IIdentity<TId>
        {
            VerifyPropertyName<T>(propertyName);
            return RunFunctionInsideATransaction(() => GetChildrenInternal<TProperty>(entity, propertyName));
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

        protected virtual ISet<TProperty> GetChildrenInternal<TProperty>(T entity, string propertyName)
            where TProperty : IIdentity<TId>
        {
            T retrievedEntity = GetByIdInternal(entity.Id);

            return RunControlledFunction(
                "GetChildrenInternal",
                () =>
                {
                    PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName);
                    object propertyValue = propertyInfo.GetValue(retrievedEntity, null);
                    ICollection children;
                    if (propertyValue == null)
                    {
                        children = new List<TProperty>();
                    }
                    else
                    {
                        children = propertyValue as ICollection;
                        if (children == null)
                        {
                            string message = string.Format(
                                "Property {0} isn't of the requested type {1}, it has type {2}.",
                                propertyName,
                                typeof(IEnumerable<TProperty>),
                                propertyInfo.PropertyType);
                            throw new ProgrammingError(message);
                        }
                    }
                    HashSet<TProperty> result = new HashSet<TProperty>(children.Cast<TProperty>());
                    return result;
                },
                entity);
        }

        protected virtual TProperty GetPropertyValueInternal<TProperty>(T entity, string propertyName)
        {
            IIdentity<TId> retrievedEntity = GetByIdInternal(entity.Id);

            return RunControlledFunction(
                "GetPropertyValueInternal",
                () =>
                {
                    TProperty result;
                    PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName);
                    try
                    {
                        result = (TProperty)propertyInfo.GetValue(retrievedEntity, null);
                    }
                    catch (InvalidCastException e)
                    {
                        string message = string.Format(
                            "Property {0} isn't of the requested type {1}, it has type {2}.",
                            propertyName,
                            typeof(TProperty),
                            propertyInfo.PropertyType);
                        throw new ProgrammingError(message, e);
                    }
                    ICollection genericCollection = result as ICollection;
                    if (genericCollection != null)
                    {
                        string message = string.Format(
                            "Property {0} implements ICollection, Use GetChildren instead.",
                            propertyName);
                        throw new ProgrammingError(message);
                    }
                    return result;
                },
                entity);
        }

        protected virtual T GetByIdInternal(TId id)
        {
            return RunControlledFunction(
                "GetByIdInternal",
                () =>
                {
                    T result = Session.Get<T>(id);
                    return result;
                });
        }

        protected virtual ISet<T> GetInternal(IEnumerable<ICriterion> criterions, IEnumerable<Order> orders)
        {
            return RunControlledFunction(
                "GetInternal",
                () =>
                {
                    ICriteria criteria = CreateCriteria(criterions, orders);
                    IList<T> qryResult = criteria.List<T>();
                    return new HashSet<T>(qryResult);
                });
        }

        protected virtual PagedList<T> GetPagedInternal(int pageIndex, int pageSize, IEnumerable<ICriterion> criterions, IEnumerable<Order> orders)
        {
            return RunControlledFunction(
                "GetPagedInternal",
                () =>
                {
                    ICriteria rowCountCriteria = CreateCriteria(criterions, null);
                    IFutureValue<int> rowCount = rowCountCriteria
                        .SetProjection(Projections.RowCount())
                        .FutureValue<int>();

                    ICriteria criteria = CreateCriteria(criterions, orders);
                    if (orders == null || !orders.Any())
                    {
                        criteria.AddOrder(Order.Asc(Projections.Id()));
                    }
                    IList<T> qryResult = criteria
                        .SetFirstResult((pageIndex - 1) * pageSize)
                        .SetMaxResults(pageSize)
                        .Future<T>()
                        .ToList<T>();
                    PagedList<T> result = new PagedList<T>(qryResult, pageIndex, pageSize, rowCount.Value);
                    return result;
                });
        }

        protected virtual ICriteria CreateCriteria(IEnumerable<ICriterion> criterions, IEnumerable<Order> orders)
        {
            ICriteria criteria = Session.CreateCriteria<T>();
            if (criterions != null)
            {
                foreach (ICriterion criterion in criterions)
                {
                    criteria.Add(criterion);
                }
            }
            if (orders != null)
            {
                foreach (Order order in orders)
                {
                    criteria.AddOrder(order);
                }
            }
            return criteria;
        }

        [Conditional("DEBUG"), DebuggerStepThrough]
        protected void VerifyPropertyName<U>(string propertyName)
            where U : T
        {
            Type type = typeof(U);
            if (!string.IsNullOrEmpty(propertyName) && type.GetProperty(propertyName) == null)
            {
                throw new ArgumentException("Property not found", propertyName);
            }
        }

        protected string GetPropertyName<U>(Expression<Func<U>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }
            Contract.EndContractBlock();

            MemberExpression body = propertyExpression.Body as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException("Invalid argument", "propertyExpression");
            }
            PropertyInfo property = body.Member as PropertyInfo;
            if (property == null)
            {
                throw new ArgumentException("Argument is not a property", "propertyExpression");
            }
            return property.Name;
        }

        protected virtual void RunActionInsideATransaction(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            Contract.EndContractBlock();

            RunFunctionInsideATransaction(
                () =>
                {
                    action.Invoke();
                    return default(int);
                });
        }

        protected virtual TResult RunFunctionInsideATransaction<TResult>(Func<TResult> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }
            Contract.EndContractBlock();

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
        /// This method *has* to convert whatever NHibernate exception to a valid PPWCode exception
        /// Some hibernate exceptions might be semantic, some might be errors
        /// This may depend on the actual product.
        /// This method translates semantic exceptions in PPWCode.Util.Exception.SemanticException and throws them
        /// and all other exceptions in PPWCode.Util.Exception.Error and throws them
        /// </summary>
        /// <param name="exception">The hibernate exception we are triaging</param>
        /// <param name="message">This message will be used in the loggin in the case aException = Error</param>
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
            if (string.IsNullOrWhiteSpace(requestDescription))
            {
                throw new ArgumentNullException("requestDescription");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            Contract.EndContractBlock();

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
            if (string.IsNullOrWhiteSpace(requestDescription))
            {
                throw new ArgumentNullException("requestDescription");
            }
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }
            Contract.EndContractBlock();

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
                    typeof(T).Name, entity != null ? entity.ToString() : string.Empty);
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