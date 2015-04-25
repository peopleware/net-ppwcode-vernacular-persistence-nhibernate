// Copyright 2015 by PeopleWare n.v..
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
using System.Collections.Generic;
using System.Linq;

using NHibernate;
using NHibernate.Criterion;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.I.Tests.Models;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.IntegrationTests
{
    // ReSharper disable InconsistentNaming
    public class CompanyQueryOverTests : CompanyBaseTests
    {
        [Test]
        public void Can_Get_Company_with_Lazy_Identifications()
        {
            Company company = Repository.Get(qry => qry.Where(c => c.Name == "Peopleware NV"));

            Assert.IsNotNull(company);
            Assert.IsFalse(NHibernateUtil.IsInitialized(company.Identifications));
        }

        [Test]
        public void Can_Get_Company_with_Eager_Identifications()
        {
            Company company = Repository.Get(
                qry => qry.Where(c => c.Name == "Peopleware NV")
                          .Fetch(c => c.Identifications).Eager);

            Assert.IsNotNull(company);
            Assert.IsTrue(NHibernateUtil.IsInitialized(company.Identifications));
        }

        [Test]
        public void Can_Get_Company_with_Identification_1()
        {
            QueryOver<CompanyIdentification, CompanyIdentification> detachedQuery =
                QueryOver.Of<CompanyIdentification>()
                         .Where(ci => ci.Identification == "1")
                         .Select(Projections.Id());
            Company company = Repository.Get(qry => qry.WithSubquery.WhereExists(detachedQuery));

            Assert.IsNotNull(company);
            Assert.IsFalse(NHibernateUtil.IsInitialized(company.Identifications));
        }

        [Test]
        public void Can_Get_Company_with_Identification_1_with_explicit_join()
        {
            CompanyIdentification ci = null;
            Company company = Repository.Get(
                qry => qry.Inner.JoinAlias(c => c.Identifications, () => ci)
                          .Where(() => ci.Identification == "1"));

            Assert.IsNotNull(company);
            Assert.IsFalse(NHibernateUtil.IsInitialized(company.Identifications));
        }

        [Test]
        public void Can_Find_All_Companies()
        {
            IList<Company> companies = Repository.Find((Func<IQueryOver<Company, Company>, IQueryOver<Company, Company>>)null);

            Assert.IsNotNull(companies);
            Assert.IsFalse(companies.Any(c => NHibernateUtil.IsInitialized(c.Identifications)));
        }

        [Test]
        public void Can_FindPaged_Company()
        {
            IPagedList<Company> pagedList = Repository.FindPaged(
                1,
                20,
                qry => qry.Where(c => c.Name == "Peopleware NV")
                          .OrderBy(c => c.Name).Asc());

            Assert.IsNotNull(pagedList);
            Assert.IsFalse(pagedList.HasPreviousPage);
            Assert.IsFalse(pagedList.HasNextPage);
            Assert.AreEqual(1, pagedList.Items.Count);
            Assert.AreEqual(1, pagedList.TotalCount);
            Assert.AreEqual(1, pagedList.TotalPages);
        }

        [Test]
        public void Can_Page_All_Companies()
        {
            IPagedList<Company> pagedList =
                Repository.FindPaged(1, 20, (Func<IQueryOver<Company, Company>, IQueryOver<Company, Company>>)null);

            Assert.IsNotNull(pagedList);
            Assert.IsFalse(pagedList.HasPreviousPage);
            Assert.IsFalse(pagedList.HasNextPage);
            Assert.AreEqual(1, pagedList.Items.Count);
            Assert.AreEqual(1, pagedList.TotalCount);
            Assert.AreEqual(1, pagedList.TotalPages);
        }
    }
}