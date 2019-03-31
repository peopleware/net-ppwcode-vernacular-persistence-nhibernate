// Copyright 2017 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using NHibernate;
using NHibernate.Cfg;

using Environment = System.Environment;

namespace PPWCode.Vernacular.NHibernate.III.Test
{
    public class DirtyChecking
    {
        public DirtyChecking(
            [NotNull] Configuration configuration,
            [NotNull] ISessionFactory sessionFactory,
            [NotNull] Action<string> failCallback,
            [NotNull] Action<string> inconclusiveCallback)
        {
            Configuration = configuration;
            SessionFactory = sessionFactory;
            FailCallback = failCallback;
            InconclusiveCallback = inconclusiveCallback;
        }

        protected Configuration Configuration { get; }
        protected Action<string> FailCallback { get; }
        protected Action<string> InconclusiveCallback { get; }
        protected ISessionFactory SessionFactory { get; }

        public void Test()
        {
            IEnumerable<string> mappedEntityNames =
                Configuration
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
                string msg = $"No instances of {entityName} in database.";
                InconclusiveCallback.Invoke(msg);

                return;
            }

            Test(entityName, id);
        }

        public void Test(string entityName, object id)
        {
            List<string> ghosts = new List<string>();
            DirtyCheckingInterceptor interceptor = new DirtyCheckingInterceptor(ghosts);

            using (ISession session = SessionFactory.WithOptions().Interceptor(interceptor).OpenSession())
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
                FailCallback.Invoke(string.Join(Environment.NewLine, ghosts.ToArray()));
            }
        }

        [CanBeNull]
        private object FindEntityId(string entityName)
        {
            object id;
            using (ISession session = SessionFactory.OpenSession())
            {
                string cmdText = $"SELECT e.id FROM {entityName} e";

                IQuery query =
                    session
                        .CreateQuery(cmdText)
                        .SetMaxResults(1);

                using (ITransaction tx = session.BeginTransaction())
                {
                    id = query.UniqueResult();
                    tx.Commit();
                }
            }

            return id;
        }
    }
}
