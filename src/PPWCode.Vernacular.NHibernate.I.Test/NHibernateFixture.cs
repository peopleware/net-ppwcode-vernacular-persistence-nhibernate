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

using HibernatingRhinos.Profiler.Appender.NHibernate;

using log4net.Config;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Tool.hbm2ddl;

using PPWCode.Util.OddsAndEnds.II.ConfigHelper;

namespace PPWCode.Vernacular.NHibernate.I.Test
{
    public abstract class NHibernateFixture : BaseFixture
    {
        protected ISessionFactory SessionFactory
        {
            get { return NhConfigurator.SessionFactory; }
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

        protected override void OnSetup()
        {
            base.OnSetup();

            XmlConfigurator.Configure();
            if (UseProfiler)
            {
                NHibernateProfiler.Initialize();
            }

            SetupNHibernateSession();
        }

        protected override void OnTeardown()
        {
            TearDownNHibernateSession();
            if (UseProfiler)
            {
                NHibernateProfiler.Stop();
            }

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
            ISessionFactory sessionFactory = NhConfigurator.SessionFactory;
            ISession session = CurrentSessionContext.Unbind(sessionFactory);
            session.Close();
        }

        private void BuildSchema()
        {
            Configuration cfg = NhConfigurator.Configuration;
            SchemaExport schemaExport = new SchemaExport(cfg);
            schemaExport.Create(false, true);
        }
    }
}