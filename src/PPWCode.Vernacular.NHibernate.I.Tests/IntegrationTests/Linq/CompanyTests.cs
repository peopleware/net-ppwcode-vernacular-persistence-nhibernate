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

using System.Collections.Generic;
using System.Linq;

using NHibernate;
using NHibernate.Linq;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.I.Tests.Models;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.IntegrationTests.Linq
{
    // ReSharper disable InconsistentNaming
    public class CompanyTests : BaseCompanyTests
    {
        [Test]
        public void Can_Get_Company_with_Lazy_Identifications()
        {
            Company company = Repository.Get(qry => qry.Where(c => c.Name == "Peopleware NV"));
            Session.Flush();
            Assert.That(company, Is.Not.Null);
            Assert.That(NHibernateUtil.IsInitialized(company.Identifications), Is.False);
        }

        [Test]
        public void Can_Get_Company_with_Eager_Identifications()
        {
            Company company = Repository.Get(
                qry => qry.Where(c => c.Name == "Peopleware NV")
                          .FetchMany(c => c.Identifications));

            Assert.That(company, Is.Not.Null);
            Assert.That(NHibernateUtil.IsInitialized(company.Identifications), Is.True);
        }

        [Test]
        public void Can_Get_Company_with_Identification_1()
        {
            Company company = Repository.Get(qry => qry.Where(c => c.Identifications.Any(i => i.Identification == "1")));

            Assert.That(company, Is.Not.Null);
            Assert.That(NHibernateUtil.IsInitialized(company.Identifications), Is.False);
        }

        [Test]
        public void Can_Get_Company_with_Identification_1_with_explicit_join()
        {
            Company company = Repository.Get(
                qry => qry.SelectMany(c => c.Identifications, (c, ci) => new { c, ci })
                          .Where(t => t.ci.Identification == "1")
                          .Select(t => t.c)
                          .Distinct());

            Assert.That(company, Is.Not.Null);
            Assert.That(NHibernateUtil.IsInitialized(company.Identifications), Is.False);
        }

        [Test]
        public void Can_Find_All_Companies()
        {
            IList<Company> companies = Repository.FindAll();

            Assert.That(companies, Is.Not.Null);
            Assert.That(companies.Any(c => NHibernateUtil.IsInitialized(c.Identifications)), Is.False);
        }

        [Test]
        public void Can_FindPaged_Company()
        {
            IPagedList<Company> pagedList =
                Repository
                    .FindPaged(
                        1,
                        20,
                        qry => qry.Where(c => c.Name == "Peopleware NV")
                                  .OrderBy(c => c.Name));

            Assert.That(pagedList, Is.Not.Null);
            Assert.That(pagedList.HasPreviousPage, Is.False);
            Assert.That(pagedList.HasNextPage, Is.False);
            Assert.That(pagedList.Items.Count, Is.EqualTo(1));
            Assert.That(pagedList.TotalCount, Is.EqualTo(1));
            Assert.That(pagedList.TotalPages, Is.EqualTo(1));
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
    }
}
