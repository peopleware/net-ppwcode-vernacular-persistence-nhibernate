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

using HibernatingRhinos.Profiler.Appender.NHibernate;

using log4net.Config;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Context;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Tool.hbm2ddl;

using PPWCode.Util.OddsAndEnds.II.ConfigHelper;
using PPWCode.Vernacular.NHibernate.I.Utilities;

namespace PPWCode.Vernacular.NHibernate.I.Test
{
    public abstract class NHibernateFixture : BaseFixture
    {
        private const string ConnectionString = "Data Source=:memory:;Version=3;New=True;";

        private ISessionFactory m_SessionFactory;
        private Configuration m_Configuration;

        protected Configuration Configuration
        {
            get
            {
                if (m_Configuration == null)
                {
                    m_Configuration = new Configuration()
                        .DataBaseIntegration(
                            db =>
                            {
                                db.Dialect<SQLiteDialect>();
                                db.Driver<SQLite20Driver>();
                            })
                        .Configure()
                        .DataBaseIntegration(
                            db =>
                            {
                                db.Dialect<SQLiteDialect>();
                                db.Driver<SQLite20Driver>();
                                db.ConnectionProvider<TestConnectionProvider>();
                                db.ConnectionString = ConnectionString;
                                db.IsolationLevel = IsolationLevel.ReadCommitted;
                            })
                        .SetProperty(Environment.CurrentSessionContextClass, "thread_static")
                        .SetProperty(Environment.ShowSql, "true")
                        .SetProperty(Environment.FormatSql, "true")
                        .SetProperty(Environment.GenerateStatistics, "true");

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

        protected virtual ISessionFactory SessionFactory
        {
            get { return m_SessionFactory ?? (m_SessionFactory = Configuration.BuildSessionFactory()); }
        }

        protected virtual ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }

        protected ISession Session
        {
            get { return SessionFactory.GetCurrentSession(); }
        }

        protected virtual bool UseProfiler
        {
            get { return ConfigHelper.GetAppSetting("UseProfiler", false); }
        }

        protected override void OnFixtureSetup()
        {
            base.OnFixtureSetup();

            if (UseProfiler)
            {
                NHibernateProfiler.Initialize();
            }
        }

        protected override void OnFixtureTeardown()
        {
            base.OnFixtureTeardown();

            if (UseProfiler)
            {
                NHibernateProfiler.Stop();
            }
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            XmlConfigurator.Configure();
            SetupNHibernateSession();
        }

        protected override void OnTeardown()
        {
            TearDownNHibernateSession();

            base.OnTeardown();
        }

        protected void SetupNHibernateSession()
        {
            TestConnectionProvider.CloseDatabase();
            SetupContextualSession();
            BuildSchema();
        }

        protected void TearDownNHibernateSession()
        {
            TearDownContextualSession();
            TestConnectionProvider.CloseDatabase();
        }

        private void SetupContextualSession()
        {
            ISession session = OpenSession();
            CurrentSessionContext.Bind(session);
        }

        private void TearDownContextualSession()
        {
            ISession session = CurrentSessionContext.Unbind(SessionFactory);
            if (session != null)
            {
                session.Close();
            }
        }

        private void BuildSchema()
        {
            SchemaExport schemaExport = new SchemaExport(Configuration);
            schemaExport.Create(false, true);
        }
    }
}