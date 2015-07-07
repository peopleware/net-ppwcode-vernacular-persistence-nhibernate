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
using System.Diagnostics.Contracts;
using System.Linq;

using NHibernate;
using NHibernate.Cfg;

using Environment = System.Environment;

namespace PPWCode.Vernacular.NHibernate.I.Test
{
    public class DirtyChecking
    {
        private readonly Configuration m_Configuration;
        private readonly Action<string> m_FailCallback;
        private readonly Action<string> m_InconclusiveCallback;
        private readonly ISessionFactory m_SessionFactory;

        public DirtyChecking(Configuration configuration, ISessionFactory sessionFactory, Action<string> failCallback, Action<string> inconclusiveCallback)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(sessionFactory != null);
            Contract.Requires(failCallback != null);
            Contract.Requires(inconclusiveCallback != null);

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
            List<string> ghosts = new List<string>();
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