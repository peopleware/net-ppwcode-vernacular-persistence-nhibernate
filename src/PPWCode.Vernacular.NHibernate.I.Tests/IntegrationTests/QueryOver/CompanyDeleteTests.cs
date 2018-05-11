// Copyright 2017-2018 by PeopleWare n.v..
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

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.I.Tests.Models;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.IntegrationTests.QueryOver
{
    // ReSharper disable InconsistentNaming
    public class CompanyDeleteTests : BaseCompanyTests
    {
        [Test]
        public void Can_Delete_A_Company()
        {
            // Check if no deletes are already performed
            Assert.That(SessionFactory.Statistics.EntityDeleteCount, Is.EqualTo(0));

            RunInsideTransaction(() => Repository.Delete(CreatedCompany), true);

            // A company with 2 children are deleted
            Assert.That(SessionFactory.Statistics.EntityDeleteCount, Is.EqualTo(3));
        }

        [Test]
        public void Can_Delete_A_None_Existing_Company()
        {
            Company noneExistingCompany =
                new Company(-1, 1)
                {
                    Name = "My Company name"
                };
            Repository.Delete(noneExistingCompany);
        }

        [Test]
        public void Can_Delete_A_Transient_Company()
        {
            Company transientCompany = new Company();
            Repository.Delete(transientCompany);
        }

        [Test]
        public void Can_Not_Delete_A_Stale_Company()
        {
            RunInsideTransaction(
                () =>
                {
                    CreatedCompany.Name = string.Concat(CreatedCompany.Name, " 2");
                    Repository.Merge(CreatedCompany);
                },
                true);

            Assert.That(() => Repository.Delete(CreatedCompany), Throws.TypeOf<ObjectAlreadyChangedException>());
        }
    }
}
