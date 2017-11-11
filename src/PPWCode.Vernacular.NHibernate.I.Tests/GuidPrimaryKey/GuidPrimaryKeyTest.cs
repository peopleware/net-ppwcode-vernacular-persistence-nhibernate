// Copyright 2017 by PeopleWare n.v..
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

using NHibernate.Cfg.MappingSchema;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.NHibernate.I.Test;
using PPWCode.Vernacular.NHibernate.I.Tests.GuidPrimaryKey.Models;
using PPWCode.Vernacular.NHibernate.I.Tests.GuidPrimaryKey.Repositories;
using PPWCode.Vernacular.NHibernate.I.Tests.Models.Mapping;

namespace PPWCode.Vernacular.NHibernate.I.Tests.GuidPrimaryKey
{
    public class GuidPrimaryKeyTest : BaseRepositoryFixture<Car, Guid, TestGuidAuditLog>
    {
        protected override string CatalogName
        {
            get { return "Test.PPWCode.Vernacular.NHibernate.I.Tests"; }
        }

        protected override string ConnectionString
        {
            get { return FixedConnectionString; }
        }

        protected override HbmMapping GetHbmMapping()
        {
            IHbmMapping mapper = new TestsSimpleModelMapper(new TestsMappingAssemblies());
            return mapper.GetHbmMapping();
        }

        protected override string IdentityName
        {
            get { return "Test - IdentityName"; }
        }

        protected override Func<IRepository<Car, Guid>> RepositoryFactory
        {
            get { return () => new CarRepository(Session); }
        }

        [Test]
        public void Test()
        {
            RunInsideTransaction(
                () =>
                {
                    Car car = new Car
                              {
                                  ModelName = "Fiat"
                              };

                    RepositoryFactory().SaveOrUpdate(car);

                    Assert.AreNotEqual(Guid.Empty, car.Id);
                },
                true);

            Console.WriteLine("done");
        }
    }
}