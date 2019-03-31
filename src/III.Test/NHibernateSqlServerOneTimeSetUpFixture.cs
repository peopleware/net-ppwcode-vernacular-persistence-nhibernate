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
using System.IO;
using System.Reflection;

using HibernatingRhinos.Profiler.Appender.NHibernate;

#if !NET461
using log4net;
#endif
using log4net.Config;
#if !NET461
using log4net.Repository;
#endif

using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Test
{
    public abstract class NHibernateSqlServerOneTimeSetUpFixture<TId, TAuditEntity>
        : NHibernateSqlServerFixture<TId, TAuditEntity>
        where TId : IEquatable<TId>
        where TAuditEntity : AuditLog<TId>, new()
    {
        protected override void OnFixtureSetup()
        {
#if NET461
            // configure for all tests in this class
            XmlConfigurator.Configure();
#else
            ILoggerRepository logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
#endif

            // configure for all tests in this class
            if (UseProfiler)
            {
                NHibernateProfiler.Initialize();
            }

            // initialize for all tests in this test class
            CreateCatalog();
            BuildSchema();
        }

        protected override void OnFixtureTeardown()
        {
            // configure for all tests in this class
            if (UseProfiler)
            {
                NHibernateProfiler.Shutdown();
            }

            CloseSessionFactory();
            DropCatalog();
            ResetConfiguration();
        }

        protected override void OnSetup()
        {
        }

        protected override void OnTeardown()
        {
            // configure for each test
            CloseSession();
        }
    }
}
