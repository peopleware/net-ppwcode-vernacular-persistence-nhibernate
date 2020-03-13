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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NHibernate;
using NHibernate.Linq;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.III.Tests.Model.Common;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests.Sync.Linq.Common
{
    public class CompanyTests : BaseCompanyTests
    {
        [Test]
        public async Task Can_Count_Companies()
        {
            Company company = await CreateCompanyAsync(CompanyCreationType.WITH_2_CHILDREN, CancellationToken);
            int count =
                await Repository
                    .CountAsync(
                        query => query.Where(c => c.Id == company.Id),
                        CancellationToken);

            Assert.AreEqual(1, count);
        }

        [Test]
        public async Task Can_Find_All_Companies()
        {
            await CreateCompanyAsync(CompanyCreationType.WITH_2_CHILDREN, CancellationToken);
            SessionProviderAsync.Session.Clear();
            IList<Company> companies =
                await Repository
                    .FindAllAsync(CancellationToken)
                    .ConfigureAwait(false);

            Assert.That(companies, Is.Not.Null);
            Assert.That(companies.Count, Is.EqualTo(1));
            Assert.That(companies.Any(c => NHibernateUtil.IsInitialized(c.Identifications)), Is.False);
        }

        [TestCase(1, 0, 1)]
        [TestCase(5, 2, 3)]
        public async Task Can_Find_Companies_With_Skip_And_Count(int numberOfTuples, int skip, int count)
        {
            Assert.That(numberOfTuples, Is.GreaterThanOrEqualTo(skip + count));
            IList<Company> companies = new List<Company>();
            for (int i = 0; i < numberOfTuples; i++)
            {
                companies.Add(await CreateCompanyAsync(CompanyCreationType.NO_CHILDREN, CancellationToken));
            }

            SessionProviderAsync.Session.Clear();
            IList<Company> foundCompanies =
                await Repository
                    .FindAsync(
                        query => query.OrderBy(c => c.Id),
                        skip,
                        count,
                        CancellationToken)
                    .ConfigureAwait(false);

            Assert.That(foundCompanies, Is.Not.Null);
            Assert.That(foundCompanies.Count, Is.EqualTo(count));
            for (int i = 0; i < count; i++)
            {
                Assert.That(foundCompanies[i].Id, Is.EqualTo(companies[skip + i].Id));
            }
        }

        [Test]
        public async Task Can_Find_Companies_By_Ids()
        {
            Company company1 = await CreateCompanyAsync(CompanyCreationType.WITH_2_CHILDREN, CancellationToken);
            Company company2 = await CreateCompanyAsync(CompanyCreationType.NO_CHILDREN, CancellationToken);
            Company company3 = await CreateCompanyAsync(CompanyCreationType.NO_CHILDREN, CancellationToken);
            IList<Company> companies = await Repository.FindByIdsAsync(new[] { company1.Id, company2.Id, company3.Id }, CancellationToken);

            Assert.That(companies, Is.Not.Null);
            Assert.AreEqual(3, companies.Count);
        }

        [Test]
        public async Task Can_FindPaged_Company()
        {
            await CreateCompanyAsync(CompanyCreationType.WITH_2_CHILDREN, CancellationToken);
            IPagedList<Company> pagedList =
                await Repository
                    .FindPagedAsync(
                        query =>
                            query
                                .Where(c => c.Name == "Peopleware NV")
                                .OrderBy(c => c.Name),
                        1,
                        20,
                        CancellationToken);

            Assert.That(pagedList, Is.Not.Null);
            Assert.That(pagedList.HasPreviousPage, Is.False);
            Assert.That(pagedList.HasNextPage, Is.False);
            Assert.That(pagedList.Items.Count, Is.EqualTo(1));
            Assert.That(pagedList.TotalCount, Is.EqualTo(1));
            Assert.That(pagedList.TotalPages, Is.EqualTo(1));
        }

        [Test]
        public async Task Can_Get_Company_By_Id()
        {
            Company company = await CreateCompanyAsync(CompanyCreationType.NO_CHILDREN, CancellationToken);
            SessionProviderAsync.Session.Clear();
            Company loadedCompany = await Repository.GetByIdAsync(company.Id, CancellationToken);

            Assert.That(loadedCompany, Is.Not.Null);
            Assert.AreEqual(loadedCompany.Id, company.Id);
        }

        [TestCase(1, 0)]
        [TestCase(2, 0)]
        [TestCase(2, 1)]
        public async Task Can_Get_Company_At_Index(int numberOfTuples, int index)
        {
            Assert.That(numberOfTuples, Is.GreaterThanOrEqualTo(index));
            IList<Company> companies = new List<Company>();
            for (int i = 0; i < numberOfTuples; i++)
            {
                companies.Add(await CreateCompanyAsync(CompanyCreationType.NO_CHILDREN, CancellationToken));
            }

            SessionProviderAsync.Session.Clear();
            Company companyAtIndex =
                await Repository
                    .GetAtIndexAsync(
                        query => query.OrderBy(c => c.Id),
                        index,
                        CancellationToken);

            Assert.That(companyAtIndex, Is.Not.Null);
            Assert.AreEqual(companyAtIndex.Id, companies[index].Id);
        }

        [Test]
        public async Task Can_Get_Company_with_Eager_Identifications()
        {
            await CreateCompanyAsync(CompanyCreationType.WITH_2_CHILDREN, CancellationToken);
            SessionProviderAsync.Session.Clear();

            Company company =
                await Repository
                    .GetAsync(
                        query =>
                            query
                                .Where(c => c.Name == "Peopleware NV")
                                .FetchMany(c => c.Identifications),
                        CancellationToken);

            Assert.That(company, Is.Not.Null);
            Assert.That(NHibernateUtil.IsInitialized(company.Identifications), Is.True);
        }

        [Test]
        public async Task Can_Get_Company_with_Identification_1()
        {
            await CreateCompanyAsync(CompanyCreationType.WITH_2_CHILDREN, CancellationToken);
            SessionProviderAsync.Session.Clear();

            Company company =
                await Repository
                    .GetAsync(
                        qry => qry.Where(c => c.Identifications.Any(i => i.Identification == "1")),
                        CancellationToken);

            Assert.That(company, Is.Not.Null);
            Assert.That(NHibernateUtil.IsInitialized(company.Identifications), Is.False);
        }

        [Test]
        public async Task Can_Get_Company_with_Identification_1_with_explicit_join()
        {
            await CreateCompanyAsync(CompanyCreationType.WITH_2_CHILDREN, CancellationToken);
            SessionProviderAsync.Session.Clear();

            Company company =
                await Repository
                    .GetAsync(
                        qry =>
                            qry
                                .SelectMany(c => c.Identifications, (c, ci) => new { c, ci })
                                .Where(t => t.ci.Identification == "1")
                                .Select(t => t.c)
                                .Distinct(),
                        CancellationToken);

            Assert.That(company, Is.Not.Null);
            Assert.That(NHibernateUtil.IsInitialized(company.Identifications), Is.False);
        }

        [Test]
        public async Task Can_Get_Company_with_Lazy_Identifications()
        {
            await CreateCompanyAsync(CompanyCreationType.WITH_2_CHILDREN, CancellationToken);
            SessionProviderAsync.Session.Clear();

            Company company =
                await Repository
                    .GetAsync(
                        query => query.Where(c => c.Name == "Peopleware NV"),
                        CancellationToken);
            await SessionProviderAsync.FlushAsync(CancellationToken);

            Assert.That(company, Is.Not.Null);
            Assert.That(NHibernateUtil.IsInitialized(company.Identifications), Is.False);
        }

        [Test]
        public async Task Can_Load_Company_By_Id()
        {
            Company company = await CreateCompanyAsync(CompanyCreationType.NO_CHILDREN, CancellationToken);
            SessionProviderAsync.Session.Clear();
            Company loadedCompany = await Repository.LoadByIdAsync(company.Id, CancellationToken);
            await NHibernateUtil.InitializeAsync(loadedCompany, CancellationToken);

            Assert.That(loadedCompany, Is.Not.Null);
            Assert.AreEqual(loadedCompany.Id, company.Id);
        }

        [Test]
        public async Task Can_Page_All_Companies()
        {
            await CreateCompanyAsync(CompanyCreationType.WITH_2_CHILDREN, CancellationToken);
            IPagedList<Company> pagedList =
                await Repository
                    .FindPagedAsync(
                        query => query,
                        1,
                        20,
                        CancellationToken);

            Assert.That(pagedList, Is.Not.Null);
            Assert.That(pagedList.HasPreviousPage, Is.False);
            Assert.That(pagedList.HasNextPage, Is.False);
            Assert.That(pagedList.Items.Count, Is.EqualTo(1));
            Assert.That(pagedList.TotalCount, Is.EqualTo(1));
            Assert.That(pagedList.TotalPages, Is.EqualTo(1));
        }
    }
}
