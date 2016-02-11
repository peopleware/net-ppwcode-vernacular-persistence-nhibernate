// Copyright 2016 by PeopleWare n.v..
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
using System.Data;

using HibernatingRhinos.Profiler.Appender;

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
    public abstract class NHibernateFixture<TId> : BaseFixture
        where TId : IEquatable<TId>
    {
        private ISessionFactory m_SessionFactory;
        private ISession m_Session;

        protected abstract Configuration Configuration { get; }
        protected abstract string IdentityName { get; }
        protected abstract DateTime Now { get; }

        protected virtual bool UseProfiler
        {
            get { return ConfigHelper.GetAppSetting("UseProfiler", false); }
        }

        protected virtual bool SuppressProfilingWhileCreatingSchema
        {
            get { return ConfigHelper.GetAppSetting("SuppressProfilingWhileCreatingSchema", true); }
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
            identityProvider
                .Setup(ip => ip.IdentityName)
                .Returns(IdentityName);

            Mock<ITimeProvider> timeProvider = new Mock<ITimeProvider>();
            timeProvider
                .Setup(tp => tp.Now)
                .Returns(Now);

            AuditInterceptor<TId> sessionLocalInterceptor = new AuditInterceptor<TId>(identityProvider.Object, timeProvider.Object);
            return SessionFactory.OpenSession(sessionLocalInterceptor);
        }

        protected virtual ISession Session
        {
            get { return m_Session ?? (m_Session = OpenSession()); }
        }

        protected virtual void BuildSchema()
        {
            SchemaExport schemaExport = new SchemaExport(Configuration);
            if (UseProfiler && SuppressProfilingWhileCreatingSchema)
            {
                using (ProfilerIntegration.IgnoreAll())
                {
                    schemaExport.Create(false, true);
                }
            }
            else
            {
                schemaExport.Create(false, true);
            }
        }

        protected virtual void CloseSessionFactory()
        {
            if (m_SessionFactory != null)
            {
                m_SessionFactory.Close();
                m_SessionFactory = null;
            }
        }

        protected virtual void CloseSession()
        {
            if (m_Session != null)
            {
                m_Session.Close();
                m_Session = null;
            }
        }

        protected T RunInsideTransaction<T>(Func<T> func, bool clearSession)
        {
            T result;
            ITransaction transaction = Session.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                result = func();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                transaction.Dispose();
            }

            if (clearSession)
            {
                Session.Clear();
            }

            return result;
        }

        protected void RunInsideTransaction(Action action, bool clearSession)
        {
            RunInsideTransaction(
                () =>
                {
                    action();
                    return 0;
                },
                clearSession);
        }
    }
}