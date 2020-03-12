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

using NHibernate;
using NHibernate.Criterion;
using NHibernate.Proxy;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.III.Providers;
using PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests.Async.QueryOver.Common.Repositories;
using PPWCode.Vernacular.NHibernate.III.Tests.Model.Common;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests.Async.QueryOver.Common
{
    // ReSharper disable InconsistentNaming
    public class CompanyTests : BaseCompanyTests
    {
        protected override void OnSetup()
        {
            base.OnSetup();

            CreatedCompany = CreateExtendedCompany(CompanyCreationType.WITH_2_CHILDREN);
        }

        protected Company CreatedCompany { get; set; }

        public class ExtendedCompanyRepository : TestRepository<ExtendedCompany>
        {
            public ExtendedCompanyRepository(ISessionProvider sessionProvider)
                : base(sessionProvider)
            {
            }
        }

        [Test]
        public void Can_Count_Companies()
        {
            Company company1 = CreateCompany(CompanyCreationType.NO_CHILDREN);
            CreateCompany(CompanyCreationType.NO_CHILDREN);

            int count = Repository.Count(companies => companies.Where(c => c.Id == company1.Id));

            Assert.AreEqual(1, count);
        }

        [Test]
        public void Can_Find_All_Companies()
        {
            IList<Company> companies = Repository.FindAll();

            Assert.That(companies, Is.Not.Null);
            Assert.That(companies.Any(c => NHibernateUtil.IsInitialized(c.Identifications)), Is.False);
        }

        [Test]
        public void Can_Find_Companies_By_Ids()
        {
            Company company1 = CreateCompany(CompanyCreationType.NO_CHILDREN);
            Company company2 = CreateCompany(CompanyCreationType.NO_CHILDREN);

            IList<Company> companies = Repository.FindByIds(new[] { company1.Id, company2.Id });

            Assert.That(companies, Is.Not.Null);
            Assert.AreEqual(2, companies.Count);
        }

        [Test]
        public void Can_Find_Company_with_use_of_an_root_alias()
        {
            Company rootAlias = null;
            IList<Company> resultSet = Repository.Find(() => rootAlias, qry => qry.Where(() => rootAlias.Name == "Peopleware NV"));

            Assert.That(resultSet, Is.Not.Null);
            Assert.That(resultSet.Count, Is.EqualTo(1));
            Assert.That(NHibernateUtil.IsInitialized(resultSet.First().Identifications), Is.False);
        }

        [Test]
        public void Can_FindPaged_Company()
        {
            IPagedList<Company> pagedList =
                Repository.FindPaged(
                    1,
                    20,
                    qry => qry.Where(c => c.Name == "Peopleware NV")
                        .OrderBy(c => c.Name).Asc());

            Assert.That(pagedList, Is.Not.Null);
            Assert.That(pagedList.HasPreviousPage, Is.False);
            Assert.That(pagedList.HasNextPage, Is.False);
            Assert.That(pagedList.Items.Count, Is.EqualTo(1));
            Assert.That(pagedList.TotalCount, Is.EqualTo(1));
            Assert.That(pagedList.TotalPages, Is.EqualTo(1));
        }

        [Test]
        public void Can_FindPaged_Company_with_use_of_an_root_alias()
        {
            Company rootAlias = null;
            IPagedList<Company> pagedList =
                Repository.FindPaged(
                    1,
                    20,
                    () => rootAlias,
                    qry => qry.Where(() => rootAlias.Name == "Peopleware NV")
                        .OrderBy(() => rootAlias.Name).Asc());

            Assert.That(pagedList, Is.Not.Null);
            Assert.That(pagedList.HasPreviousPage, Is.False);
            Assert.That(pagedList.HasNextPage, Is.False);
            Assert.That(pagedList.Items.Count, Is.EqualTo(1));
            Assert.That(pagedList.TotalCount, Is.EqualTo(1));
            Assert.That(pagedList.TotalPages, Is.EqualTo(1));
        }

        [Test]
        public void Can_Get_Company_with_Eager_Identifications()
        {
            Company company =
                Repository.Get(
                    qry => qry.Where(c => c.Name == "Peopleware NV")
                        .Fetch(SelectMode.Fetch, c => c.Identifications));

            Assert.That(company, Is.Not.Null);
            Assert.That(NHibernateUtil.IsInitialized(company.Identifications), Is.True);
        }

        [Test]
        public void Can_Get_Company_with_Identification_1()
        {
            QueryOver<CompanyIdentification, CompanyIdentification> detachedQuery =
                global::NHibernate.Criterion.QueryOver.Of<CompanyIdentification>()
                    .Where(ci => ci.Identification == "1")
                    .Select(Projections.Id());
            Company company = Repository.Get(qry => qry.WithSubquery.WhereExists(detachedQuery));

            Assert.That(company, Is.Not.Null);
            Assert.That(NHibernateUtil.IsInitialized(company.Identifications), Is.False);
        }

        [Test]
        public void Can_Get_Company_with_Identification_1_with_explicit_join()
        {
            CompanyIdentification ci = null;
            Company company =
                Repository.Get(
                    qry => qry.Inner.JoinAlias(c => c.Identifications, () => ci)
                        .Where(() => ci.Identification == "1"));

            Assert.That(company, Is.Not.Null);
            Assert.That(NHibernateUtil.IsInitialized(company.Identifications), Is.False);
        }

        [Test]
        public void Can_Get_Company_with_Lazy_Identifications()
        {
            Company company = Repository.Get(qry => qry.Where(c => c.Name == "Peopleware NV"));

            Assert.That(company, Is.Not.Null);
            Assert.That(NHibernateUtil.IsInitialized(company.Identifications), Is.False);
        }

        [Test]
        public void Can_Get_Company_with_use_of_an_root_alias()
        {
            Company rootAlias = null;
            Company company = Repository.Get(() => rootAlias, qry => qry.Where(() => rootAlias.Name == "Peopleware NV"));

            Assert.That(company, Is.Not.Null);
            Assert.That(NHibernateUtil.IsInitialized(company.Identifications), Is.False);
        }

        [Test]
        public void Can_Load_Company_By_Id()
        {
            Company company = CreateCompany(CompanyCreationType.NO_CHILDREN);

            Company loadedCompany = Repository.LoadById(company.Id);

            Assert.That(loadedCompany, Is.Not.Null);
            Assert.AreEqual(loadedCompany.Id, company.Id);
        }

        [Test]
        public void Can_Page_All_Companies()
        {
            IPagedList<Company> pagedList =
                Repository.FindPaged(1, 20, null);

            Assert.That(pagedList, Is.Not.Null);
            Assert.That(pagedList.HasPreviousPage, Is.False);
            Assert.That(pagedList.HasNextPage, Is.False);
            Assert.That(pagedList.Items.Count, Is.EqualTo(1));
            Assert.That(pagedList.TotalCount, Is.EqualTo(1));
            Assert.That(pagedList.TotalPages, Is.EqualTo(1));
        }

        [Test]
        public void ExtendCompany_Is_Lazy_One_To_One_From_Company_Side()
        {
            Company company = Repository.GetById(CreatedCompany.Id);

            Assert.That(company, Is.Not.Null);
            Assert.That(company.ExtendedCompany.IsProxy(), Is.True);
            Assert.That(NHibernateUtil.IsInitialized(company.ExtendedCompany), Is.False);
            Assert.That(company.ExtendedCompany, Is.Not.Null);
            string extraData = company.ExtendedCompany.ExtraData;
            Assert.That(extraData, Is.Not.Null);
            Assert.That(NHibernateUtil.IsInitialized(company.ExtendedCompany), Is.True);
        }
    }
}
