// Copyright 2020 by PeopleWare n.v..
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

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.III.Test;
using PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests.Sync.QueryOver.GuidPrimaryKey.Repositories;
using PPWCode.Vernacular.NHibernate.III.Tests.Model.GuidPrimaryKey;

namespace PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests.Sync.QueryOver.GuidPrimaryKey
{
    [Parallelizable(ParallelScope.Fixtures)]
    public class GuidPrimaryKeyTest : BaseRepositoryFixture<Guid, TestGuidAuditLog>
    {
        private IPpwHbmMapping _ppwHbmMapping;

        protected CarRepository Repository { get; private set; }

        protected override string CatalogName
            => "Test.PPWCode.Vernacular.NHibernate.I.Tests";

        protected override IPpwHbmMapping PpwHbmMapping
            => _ppwHbmMapping
               ?? (_ppwHbmMapping = new TestsSimpleModelMapper(new TestsMappingAssemblies()));

        protected override string IdentityName
            => "Test - IdentityName";

        protected override void OnSetup()
        {
            base.OnSetup();

            Repository = new CarRepository(SessionProvider);
        }

        protected override void OnTeardown()
        {
            Repository = null;

            base.OnTeardown();
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

                    Repository.SaveOrUpdate(car);

                    Assert.AreNotEqual(Guid.Empty, car.Id);
                },
                true);

            Console.WriteLine("done");
        }
    }
}
