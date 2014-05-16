using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

using NHibernate;
using NHibernate.Cfg;

using Environment = System.Environment;

namespace PPWCode.Vernacular.nHibernate.I.Utilities
{
    public class DirtyChecking
    {
        private readonly Configuration m_Configuration;
        private readonly Action<string> m_FailCallback;
        private readonly Action<string> m_InconclusiveCallback;
        private readonly ISessionFactory m_SessionFactory;

        public DirtyChecking(Configuration configuration, ISessionFactory sessionFactory, Action<string> failCallback, Action<string> inconclusiveCallback)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            if (sessionFactory == null)
            {
                throw new ArgumentNullException("sessionFactory");
            }
            if (failCallback == null)
            {
                throw new ArgumentNullException("failCallback");
            }
            if (inconclusiveCallback == null)
            {
                throw new ArgumentNullException("inconclusiveCallback");
            }
            Contract.EndContractBlock();

            m_Configuration = configuration;
            m_SessionFactory = sessionFactory;
            m_FailCallback = failCallback;
            m_InconclusiveCallback = inconclusiveCallback;
        }

        public void Test()
        {
            IEnumerable<string> mappedEntityNames = m_Configuration
                .ClassMappings
                .Select(m => m.EntityName);

            foreach (string entityName in mappedEntityNames)
            {
                Test(entityName);
            }
        }

        public void Test<TEntity>()
        {
            Test(typeof(TEntity).FullName);
        }

        public void Test(string entityName)
        {
            object id = FindEntityId(entityName);
            if (id == null)
            {
                string msg = string.Format("No instances of {0} in database.", entityName);
                m_InconclusiveCallback.Invoke(msg);
                return;
            }
            Test(entityName, id);
        }

        public void Test(string entityName, object id)
        {
            List<string> ghosts = new List<String>();
            DirtyCheckingInterceptor interceptor = new DirtyCheckingInterceptor(ghosts);

            using (ISession session = m_SessionFactory.OpenSession(interceptor))
            {
                using (ITransaction tx = session.BeginTransaction())
                {
                    session.Get(entityName, id);
                    session.Flush();
                    tx.Rollback();
                }
            }

            if (ghosts.Any())
            {
                m_FailCallback.Invoke(string.Join(Environment.NewLine, ghosts.ToArray()));
            }
        }

        private object FindEntityId(string entityName)
        {
            object id;
            using (ISession session = m_SessionFactory.OpenSession())
            {
                string idQueryString = string.Format("SELECT e.id FROM {0} e", entityName);

                IQuery idQuery = session
                    .CreateQuery(idQueryString)
                    .SetMaxResults(1);

                using (ITransaction tx = session.BeginTransaction())
                {
                    id = idQuery.UniqueResult();
                    tx.Commit();
                }
            }
            return id;
        }
    }
}