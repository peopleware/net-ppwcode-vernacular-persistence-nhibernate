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

using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.II.SqlServer;
using PPWCode.Vernacular.NHibernate.II.Tests.Models;

namespace PPWCode.Vernacular.NHibernate.II.Tests
{
    [TestFixture]
    public class ConventionMappingTests
    {
        [Test]
        [Explicit]
        public void DatabaseScript()
        {
            Configuration configuration =
                new Configuration()
                    .DataBaseIntegration(db => db.Dialect<MsSqlDialect>())
                    .Configure();
            IPpwHbmMapping mapper = new TestsSimpleModelMapper(new TestsMappingAssemblies(), configuration);
            HbmMapping hbmMapping = mapper.HbmMapping;
            configuration.AddMapping(hbmMapping);
            IAuxiliaryDatabaseObject[] auxiliaryDatabaseObjects =
            {
                new TestHighLowPerTableAuxiliaryDatabaseObject(mapper),
                new UniqueConstraintsForExtendedCompany(mapper)
            };
            foreach (IAuxiliaryDatabaseObject auxiliaryDatabaseObject in auxiliaryDatabaseObjects)
            {
                IPpwAuxiliaryDatabaseObject ppwAuxiliaryDatabaseObject = auxiliaryDatabaseObject as IPpwAuxiliaryDatabaseObject;
                ppwAuxiliaryDatabaseObject?.SetConfiguration(configuration);
                configuration.AddAuxiliaryDatabaseObject(auxiliaryDatabaseObject);
            }

            SchemaExport schemaExport = new SchemaExport(configuration);
            schemaExport.Create(true, false);
        }

        [Test]
        [Explicit]
        public void XmlMapping()
        {
            Configuration configuration =
                new Configuration()
                    .DataBaseIntegration(db => db.Dialect<MsSqlDialect>())
                    .Configure();
            IPpwHbmMapping mapper = new TestsSimpleModelMapper(new TestsMappingAssemblies(), configuration);
            HbmMapping hbmMapping = mapper.HbmMapping;
            Console.WriteLine(hbmMapping.AsString());
        }
    }
}
