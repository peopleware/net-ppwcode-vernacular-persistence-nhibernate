// Copyright 2014 by PeopleWare n.v..
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

using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.I.Tests;
using PPWCode.Vernacular.NHibernate.I.Tests.Models;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.IntegrationTests
{
    // ReSharper disable InconsistentNaming
    public class CompanyCriteriaTests : CompanyBaseTests
    {
        [Test]
        public void Can_Get_Company_with_Lazy_Identifications()
        {
            Company company = Repository.Get(
                qry => qry.Add(Property.ForName("Name").Eq("Peopleware NV")));

            Assert.IsNotNull(company);
            Assert.IsFalse(NHibernateUtil.IsInitialized(company.Identifications));
        }

        [Test]
        public void Can_Get_Company_with_Eager_Identifications()
        {
            Company company = Repository.Get(
                qry => qry.Add(Property.ForName("Name").Eq("Peopleware NV"))
                          .SetFetchMode("Identifications", FetchMode.Eager));

            Assert.IsNotNull(company);
            Assert.IsTrue(NHibernateUtil.IsInitialized(company.Identifications));
        }

        [Test]
        public void Can_Get_Company_with_Identification_1()
        {
            DetachedCriteria detachedCriteria = DetachedCriteria
                .For<CompanyIdentification>()
                .Add(Property.ForName("Identification").Eq("1"))
                .SetProjection(Projections.Id());
            Company company = Repository.Get(qry => qry.Add(Subqueries.Exists(detachedCriteria)));

            Assert.IsNotNull(company);
            Assert.IsFalse(NHibernateUtil.IsInitialized(company.Identifications));
        }

        [Test]
        public void Can_Get_Company_with_Identification_1_with_join()
        {
            Company company = Repository.Get(
                qry => qry.CreateAlias("Identifications", "i", JoinType.InnerJoin)
                          .Add(Property.ForName("i.Identification").Eq("1")));

            Assert.IsNotNull(company);
            Assert.IsFalse(NHibernateUtil.IsInitialized(company.Identifications));
        }

        [Test]
        public void Can_FindPaged_Company()
        {
            IPagedList<Company> pagedList =
                Repository.FindPaged(
                    1,
                    20,
                    qry => qry.Add(Property.ForName("Name").Eq("Peopleware NV"))
                              .AddOrder(Order.Asc("Name")));

            Assert.IsNotNull(pagedList);
            Assert.IsFalse(pagedList.HasPreviousPage);
            Assert.IsFalse(pagedList.HasNextPage);
            Assert.AreEqual(1, pagedList.Items.Count);
            Assert.AreEqual(1, pagedList.TotalCount);
            Assert.AreEqual(1, pagedList.TotalPages);
        }
    }
}