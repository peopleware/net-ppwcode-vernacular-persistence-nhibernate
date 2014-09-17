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

using System.Collections.Generic;
using System.Data;
using System.IO;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Context;
using NHibernate.Driver;

using PPWCode.Vernacular.NHibernate.I.Utilities;

namespace PPWCode.Vernacular.NHibernate.I.Test
{
    public abstract class NHibernateSqlLiteFixture : NHibernateFixture
    {
        private Configuration m_Configuration;
        private string m_DatabasePath;
        private string m_ConnectionString;

        protected override Configuration Configuration
        {
            get
            {
                if (m_Configuration == null)
                {
                    m_Configuration = new Configuration()
                        .DataBaseIntegration(
                            db =>
                            {
                                db.Dialect<PPWSQLiteDialect>();
                                db.Driver<SQLite20Driver>();
                            })
                        .Configure()
                        .DataBaseIntegration(
                            db =>
                            {
                                db.Dialect<PPWSQLiteDialect>();
                                db.Driver<SQLite20Driver>();
                                db.ConnectionProvider<TestConnectionProvider>();
                                db.ConnectionString = ConnectionString;
                                db.IsolationLevel = IsolationLevel.ReadCommitted;
                            })
                        .SetProperty(Environment.CurrentSessionContextClass, "thread_static")
                        .SetProperty(Environment.ShowSql, ShowSql.ToString())
                        .SetProperty(Environment.FormatSql, FormatSql.ToString())
                        .SetProperty(Environment.GenerateStatistics, GenerateStatistics.ToString());

                    IDictionary<string, string> props = m_Configuration.Properties;
                    if (props.ContainsKey(Environment.ConnectionStringName))
                    {
                        props.Remove(Environment.ConnectionStringName);
                    }

                    HbmMapping hbmMapping = GetHbmMapping();
                    if (hbmMapping != null)
                    {
                        m_Configuration.AddMapping(hbmMapping);
                    }

                    new CivilizedEventListener().Register(m_Configuration);
                }

                return m_Configuration;
            }
        }

        protected virtual HbmMapping GetHbmMapping()
        {
            return null;
        }

        protected virtual string ConnectionString
        {
            get
            {
                if (m_ConnectionString == null)
                {
                    m_DatabasePath = Path.GetTempFileName();
                    m_ConnectionString = string.Format(@"FullUri=file:{0}?mode=rwc&amp;cache=shared", m_DatabasePath);
                }

                return m_ConnectionString;
            }
        }

        protected override ISession Session
        {
            get { return CurrentSessionContext.HasBind(SessionFactory) ? SessionFactory.GetCurrentSession() : null; }
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            TestConnectionProvider.CloseDatabase();
            ISession session = OpenSession();
            CurrentSessionContext.Bind(session);

            BuildSchema();
        }

        protected override void OnTeardown()
        {
            ISession session = CurrentSessionContext.Unbind(SessionFactory);
            if (session != null)
            {
                session.Close();
            }

            TestConnectionProvider.CloseDatabase();
            File.Delete(m_DatabasePath);

            base.OnTeardown();
        }
    }
}