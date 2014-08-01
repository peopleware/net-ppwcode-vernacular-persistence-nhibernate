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

using NHibernate;
using NHibernate.Criterion;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.I.Tests.Models;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.IntegrationTests
{
    // ReSharper disable InconsistentNaming
    public class CompanyQueryOverTests : BaseCompanyTests
    {
        [Test]
        public void Can_Get_Company_with_Lazy_Identifications()
        {
            Func<IQueryOver<Company, Company>, IQueryOver<Company, Company>> func =
                qry => qry.Where(c => c.Name == "Peopleware NV");
            Company company = Repository.Get(func);

            Assert.IsNotNull(company);
            Assert.IsFalse(NHibernateUtil.IsInitialized(company.Identifications));
        }

        [Test]
        public void Can_Get_Company_with_Eager_Identifications()
        {
            Func<IQueryOver<Company, Company>, IQueryOver<Company, Company>> func =
                qry => qry
                           .Where(c => c.Name == "Peopleware NV")
                           .Fetch(c => c.Identifications).Eager;
            Company company = Repository.Get(func);

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
            Func<IQueryOver<Company, Company>, IQueryOver<Company, Company>> func =
                qry => qry.WithSubquery.WhereExists(detachedQuery);
            Company company = Repository.Get(func);

            Assert.IsNotNull(company);
            Assert.IsFalse(NHibernateUtil.IsInitialized(company.Identifications));
        }

        [Test]
        public void Can_Get_Company_with_Identification_1_with_explicit_join()
        {
            CompanyIdentification ci = null;
            Func<IQueryOver<Company, Company>, IQueryOver<Company, Company>> func =
                qry => qry
                           .Inner.JoinAlias(c => c.Identifications, () => ci)
                           .Where(() => ci.Identification == "1");
            Company company = Repository.Get(func);

            Assert.IsNotNull(company);
            Assert.IsFalse(NHibernateUtil.IsInitialized(company.Identifications));
        }

        [Test]
        public void Can_FindPaged_Company()
        {
            Func<IQueryOver<Company, Company>, IQueryOver<Company, Company>> func =
                qry => qry
                           .Where(c => c.Name == "Peopleware NV")
                           .OrderBy(c => c.Name).Asc();
            IPagedList<Company> pagedList = Repository.FindPaged(1, 20, func);

            Assert.IsNotNull(pagedList);
            Assert.IsFalse(pagedList.HasPreviousPage);
            Assert.IsFalse(pagedList.HasNextPage);
            Assert.AreEqual(1, pagedList.Items.Count);
            Assert.AreEqual(1, pagedList.TotalCount);
            Assert.AreEqual(1, pagedList.TotalPages);
        }
    }
}