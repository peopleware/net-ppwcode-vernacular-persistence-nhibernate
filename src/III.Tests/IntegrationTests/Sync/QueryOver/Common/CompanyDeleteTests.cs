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

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.III.Tests.Model.Common;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests.Sync.QueryOver.Common
{
    // ReSharper disable InconsistentNaming
    public class CompanyDeleteTests : BaseCompanyTests
    {
        [Test]
        public void Can_Delete_A_Company()
        {
            Company company = CreateCompany(CompanyCreationType.WITH_2_CHILDREN);

            // Check if no deletes are already performed
            Assert.That(SessionFactory.Statistics.EntityDeleteCount, Is.EqualTo(0));

            RunInsideTransaction(() => Repository.Delete(company), true);

            // A company with 2 children are deleted
            Assert.That(SessionFactory.Statistics.EntityDeleteCount, Is.EqualTo(3));
        }

        [Test]
        public void Can_Delete_A_Company_With_A_Failed_Company()
        {
            Company company = CreateFailedCompany(CompanyCreationType.WITH_2_CHILDREN);

            // Check if no deletes are already performed
            Assert.That(SessionFactory.Statistics.EntityDeleteCount, Is.EqualTo(0));

            RunInsideTransaction(() => Repository.Delete(company), true);

            // A company with 2 children are deleted
            Assert.That(SessionFactory.Statistics.EntityDeleteCount, Is.EqualTo(4));
        }

        [Test]
        public void Can_Delete_A_Failed_Company_From_Company_Side()
        {
            Company company = CreateFailedCompany(CompanyCreationType.WITH_2_CHILDREN);

            // Check if no deletes are already performed
            Assert.That(SessionFactory.Statistics.EntityDeleteCount, Is.EqualTo(0));

            RunInsideTransaction(
                () =>
                {
                    Company fetchedCompany = Repository.GetById(company.Id);
                    Assert.That(fetchedCompany, Is.Not.Null);
                    fetchedCompany.FailedCompany = null;
                },
                true);

            // A failed company is deleted
            Assert.That(SessionFactory.Statistics.EntityDeleteCount, Is.EqualTo(1));
        }

        [Test]
        public void Can_Delete_A_Failed_Company_From_Failed_Company_Side()
        {
            Company company = CreateFailedCompany(CompanyCreationType.WITH_2_CHILDREN);

            // Check if no deletes are already performed
            Assert.That(SessionFactory.Statistics.EntityDeleteCount, Is.EqualTo(0));

            RunInsideTransaction(
                () =>
                {
                    Company fetchedCompany = Repository.GetById(company.Id);
                    Assert.That(fetchedCompany, Is.Not.Null);
                    fetchedCompany.FailedCompany.Company = null;
                },
                true);

            // A failed company is deleted
            Assert.That(SessionFactory.Statistics.EntityDeleteCount, Is.EqualTo(1));
        }

        [Test]
        public void Can_Delete_A_None_Existing_Company()
        {
            Company noneExistingCompany =
                new Company(-1, 1)
                {
                    Name = "My Company name"
                };

            // Check if no deletes are already performed
            Assert.That(SessionFactory.Statistics.EntityDeleteCount, Is.EqualTo(0));

            Repository.Delete(noneExistingCompany);

            // No deletes are performed
            Assert.That(SessionFactory.Statistics.EntityDeleteCount, Is.EqualTo(0));
        }

        [Test]
        public void Can_Delete_A_Transient_Company()
        {
            Company transientCompany = new Company();

            // Check if no deletes are already performed
            Assert.That(SessionFactory.Statistics.EntityDeleteCount, Is.EqualTo(0));

            Repository.Delete(transientCompany);

            // No deletes are performed
            Assert.That(SessionFactory.Statistics.EntityDeleteCount, Is.EqualTo(0));
        }

        [Test]
        public void Can_Not_Delete_A_Stale_Company()
        {
            Company company = CreateCompany(CompanyCreationType.WITH_2_CHILDREN);

            // Check if no deletes are already performed
            Assert.That(SessionFactory.Statistics.EntityDeleteCount, Is.EqualTo(0));

            RunInsideTransaction(
                () =>
                {
                    company.Name = string.Concat(company.Name, " 2");
                    Repository.Merge(company);
                },
                true);

            Assert.That(() => Repository.Delete(company), Throws.TypeOf<ObjectAlreadyChangedException>());

            // No deletes are performed
            Assert.That(SessionFactory.Statistics.EntityDeleteCount, Is.EqualTo(0));
        }
    }
}
