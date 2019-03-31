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

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.III.Test;
using PPWCode.Vernacular.NHibernate.III.Tests.GuidPrimaryKey.Models;
using PPWCode.Vernacular.NHibernate.III.Tests.GuidPrimaryKey.Repositories;

namespace PPWCode.Vernacular.NHibernate.III.Tests.GuidPrimaryKey
{
    public class GuidPrimaryKeyTest : BaseRepositoryFixture<Guid, TestGuidAuditLog>
    {
        private IPpwHbmMapping _ppwHbmMapping;

        protected IQueryOverRepository<Car, Guid> Repository { get; private set; }

        protected override string CatalogName
            => "Test.PPWCode.Vernacular.NHibernate.I.Tests";

        /// <inheritdoc />
        protected override string ConnectionString
            => FixedConnectionString;

        protected override IPpwHbmMapping PpwHbmMapping
            => _ppwHbmMapping
               ?? (_ppwHbmMapping = new TestsSimpleModelMapper(new TestsMappingAssemblies()));

        protected override string IdentityName
            => "Test - IdentityName";

        protected virtual Func<IQueryOverRepository<Car, Guid>> RepositoryFactory
        {
            get { return () => new CarRepository(SessionProvider); }
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            Repository = RepositoryFactory();
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

                    RepositoryFactory().SaveOrUpdate(car);

                    Assert.AreNotEqual(Guid.Empty, car.Id);
                },
                true);

            Console.WriteLine("done");
        }
    }
}
