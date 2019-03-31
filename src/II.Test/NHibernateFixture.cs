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
using System.Data;

using HibernatingRhinos.Profiler.Appender;

using JetBrains.Annotations;

using Moq;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

using PPWCode.Vernacular.NHibernate.II.DbConstraint;
using PPWCode.Vernacular.NHibernate.II.Providers;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.II.Test
{
    public abstract class NHibernateFixture<TId>
        : BaseFixture
        where TId : IEquatable<TId>
    {
        [CanBeNull]
        private ISessionFactory _sessionFactory;

        [CanBeNull]
        private ISessionProvider _sessionProvider;

        protected abstract Configuration Configuration { get; }
        protected abstract string IdentityName { get; }
        protected abstract DateTime UtcNow { get; }

        protected virtual bool UseProfiler
            => ConfigHelper.GetAppSetting("UseProfiler", false);

        protected virtual bool SuppressProfilingWhileCreatingSchema
            => ConfigHelper.GetAppSetting("SuppressProfilingWhileCreatingSchema", true);

        protected virtual bool ShowSql
            => ConfigHelper.GetAppSetting("ShowSql", true);

        protected virtual bool FormatSql
            => ConfigHelper.GetAppSetting("FormatSql", true);

        protected virtual bool GenerateStatistics
            => ConfigHelper.GetAppSetting("GenerateStatistics", true);

        [NotNull]
        protected virtual ISessionFactory SessionFactory
            => _sessionFactory ?? (_sessionFactory = Configuration.BuildSessionFactory());

        [NotNull]
        protected virtual ISession OpenSession()
        {
            Mock<IIdentityProvider> identityProvider = new Mock<IIdentityProvider>();
            identityProvider
                .Setup(ip => ip.IdentityName)
                .Returns(IdentityName);

            Mock<ITimeProvider> timeProvider = new Mock<ITimeProvider>();
            timeProvider
                .Setup(tp => tp.Now)
                .Returns(UtcNow.ToLocalTime);
            timeProvider
                .Setup(tp => tp.UtcNow)
                .Returns(UtcNow);

            AuditInterceptor<TId> sessionLocalInterceptor = new AuditInterceptor<TId>(identityProvider.Object, timeProvider.Object, true);

            return
                SessionFactory
                    .WithOptions()
                    .Interceptor(sessionLocalInterceptor)
                    .OpenSession();
        }

        [NotNull]
        protected virtual ISessionProvider SessionProvider
            => _sessionProvider
               ?? (_sessionProvider =
                       new SessionProvider(
                           OpenSession(),
                           new TransactionProvider(),
                           new SafeEnvironmentProvider(new ExceptionTranslator()),
                           IsolationLevel.ReadCommitted));

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
            _sessionFactory?.Close();
            _sessionFactory = null;
        }

        protected virtual void CloseSession()
        {
            _sessionProvider?.Session.Close();
            _sessionProvider = null;
        }

        [CanBeNull]
        protected T RunInsideTransaction<T>([NotNull] Func<T> func, bool clearSession)
        {
            T result =
                SessionProvider
                    .TransactionProvider
                    .Run(SessionProvider.Session, SessionProvider.IsolationLevel, func);

            if (clearSession)
            {
                SessionProvider.Session.Clear();
            }

            return result;
        }

        protected void RunInsideTransaction([NotNull] Action action, bool clearSession)
        {
            SessionProvider
                .TransactionProvider
                .Run(SessionProvider.Session, SessionProvider.IsolationLevel, action);

            if (clearSession)
            {
                SessionProvider.Session.Clear();
            }
        }
    }
}
