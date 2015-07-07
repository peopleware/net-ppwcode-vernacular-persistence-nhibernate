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

using HibernatingRhinos.Profiler.Appender.NHibernate;

using log4net.Config;

using Moq;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

using PPWCode.Util.OddsAndEnds.II.ConfigHelper;
using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.NHibernate.I.Utilities;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Test
{
    public abstract class NHibernateFixture : BaseFixture
    {
        private ISessionFactory m_SessionFactory;
        private ISession m_Session;

        protected abstract Configuration Configuration { get; }

        protected virtual bool UseProfiler
        {
            get { return ConfigHelper.GetAppSetting("UseProfiler", false); }
        }

        protected virtual bool ShowSql
        {
            get { return ConfigHelper.GetAppSetting("ShowSql", true); }
        }

        protected virtual bool FormatSql
        {
            get { return ConfigHelper.GetAppSetting("FormatSql", true); }
        }

        protected virtual bool GenerateStatistics
        {
            get { return ConfigHelper.GetAppSetting("GenerateStatistics", true); }
        }

        protected virtual ISessionFactory SessionFactory
        {
            get { return m_SessionFactory ?? (m_SessionFactory = Configuration.BuildSessionFactory()); }
        }

        protected virtual ISession OpenSession()
        {
            Mock<IIdentityProvider> identityProvider = new Mock<IIdentityProvider>();
            identityProvider.Setup(ip => ip.IdentityName).Returns("Test");

            Mock<ITimeProvider> timeProvider = new Mock<ITimeProvider>();
            timeProvider.Setup(tp => tp.Now).Returns(DateTime.Now);

            AuditInterceptor<long> sessionLocalInterceptor = new AuditInterceptor<long>(identityProvider.Object, timeProvider.Object);
            return SessionFactory.OpenSession(sessionLocalInterceptor);
        }

        protected virtual ISession Session
        {
            get { return m_Session ?? (m_Session = OpenSession()); }
        }

        protected virtual void BuildSchema()
        {
            SchemaExport schemaExport = new SchemaExport(Configuration);
            schemaExport.Create(false, true);
        }

        protected override void OnFixtureSetup()
        {
            base.OnFixtureSetup();

            XmlConfigurator.Configure();
            if (UseProfiler)
            {
                NHibernateProfiler.Initialize();
            }
        }

        protected override void OnFixtureTeardown()
        {
            if (m_SessionFactory != null)
            {
                m_SessionFactory.Close();
                m_SessionFactory = null;
            }

            if (UseProfiler)
            {
                NHibernateProfiler.Stop();
            }

            base.OnFixtureTeardown();
        }

        protected override void OnTeardown()
        {
            if (m_Session != null)
            {
                m_Session.Close();
                m_Session = null;
            }

            base.OnTeardown();
        }
    }
}