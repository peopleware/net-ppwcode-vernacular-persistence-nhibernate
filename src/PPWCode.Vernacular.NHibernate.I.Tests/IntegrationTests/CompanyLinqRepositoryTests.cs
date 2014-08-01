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

using System;
using System.Linq;

using NHibernate;
using NHibernate.Linq;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.I.Tests.Models;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.IntegrationTests
{
    // ReSharper disable InconsistentNaming
    public class CompanyLinqRepositoryTests : BaseCompanyTests
    {
        [Test]
        public void Can_Get_Company_with_Lazy_Identifications()
        {
            Func<IQueryable<Company>, IQueryable<Company>> func =
                companies =>
                from c in companies
                where c.Name == "Peopleware NV"
                select c;
            Company company = Repository.Get(func);

            Assert.IsNotNull(company);
            Assert.IsFalse(NHibernateUtil.IsInitialized(company.Identifications));
        }

        [Test]
        public void Can_Get_Company_with_Eager_Identifications()
        {
            Func<IQueryable<Company>, IQueryable<Company>> func =
                companies =>
                (from c in companies
                 where c.Name == "Peopleware NV"
                 select c)
                    .FetchMany(c => c.Identifications);
            Company company = Repository.Get(func);

            Assert.IsNotNull(company);
            Assert.IsTrue(NHibernateUtil.IsInitialized(company.Identifications));
        }

        [Test]
        public void Can_Get_Company_with_Identification_1()
        {
            Func<IQueryable<Company>, IQueryable<Company>> func =
                companies =>
                from c in companies
                where c.Identifications.Any(ci => ci.Identification == "1")
                select c;
            Company company = Repository.Get(func);

            Assert.IsNotNull(company);
            Assert.IsFalse(NHibernateUtil.IsInitialized(company.Identifications));
        }

        [Test]
        public void Can_Get_Company_with_Identification_1_with_join()
        {
            Company company = Repository.Get(
                companies => from c in companies
                             from ci in c.Identifications
                             where ci.Identification == "1"
                             select c);

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
                    companies => from c in companies
                                 where c.Name == "Peopleware NV"
                                 orderby c.Name
                                 select c);

            Assert.IsNotNull(pagedList);
            Assert.IsFalse(pagedList.HasPreviousPage);
            Assert.IsFalse(pagedList.HasNextPage);
            Assert.AreEqual(1, pagedList.Items.Count);
            Assert.AreEqual(1, pagedList.TotalCount);
            Assert.AreEqual(1, pagedList.TotalPages);
        }
    }
}