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

using HibernatingRhinos.Profiler.Appender.NHibernate;

using log4net.Config;

using PPWCode.Vernacular.Persistence.III;

namespace PPWCode.Vernacular.NHibernate.II.Test
{
    public abstract class NHibernateSqlServerSetUpFixture<TId, TAuditEntity>
        : NHibernateSqlServerFixture<TId, TAuditEntity>
        where TId : IEquatable<TId>
        where TAuditEntity : AuditLog<TId>, new()
    {
        protected override void OnFixtureSetup()
        {
            // configure for all tests in this class
            XmlConfigurator.Configure();

            // configure for all tests in this class
            if (UseProfiler)
            {
                NHibernateProfiler.Initialize();
            }
        }

        protected override void OnFixtureTeardown()
        {
            // configure for all tests in this class
            if (UseProfiler)
            {
                NHibernateProfiler.Shutdown();
            }
        }

        protected override void OnSetup()
        {
            // initialize for each test
            CreateCatalog();
            BuildSchema();
        }

        protected override void OnTeardown()
        {
            // configure for each test
            CloseSession();
            CloseSessionFactory();
            DropCatalog();
            ResetConfiguration();
        }
    }
}
