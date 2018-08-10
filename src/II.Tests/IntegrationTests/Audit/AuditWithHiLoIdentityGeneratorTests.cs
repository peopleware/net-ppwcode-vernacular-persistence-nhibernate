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

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.II.Tests.IntegrationTests.QueryOver;
using PPWCode.Vernacular.NHibernate.II.Tests.Models;

namespace PPWCode.Vernacular.NHibernate.II.Tests.IntegrationTests.Audit
{
    // ReSharper disable InconsistentNaming
    public class AuditWithHiLoIdentityGeneratorTests : BaseCompanyTests
    {
        [Test]
        public void Created_Audit_Fields_Should_be_Set_After_Save()
        {
            Company company = CreateCompany(CompanyCreationType.NO_CHILDREN);

            Assert.AreEqual(IdentityName, company.CreatedBy);
            Assert.AreEqual(UtcNow, company.CreatedAt);
        }

        [Test]
        public void Created_Audit_Fields_Should_be_Set_After_Save_With_Children()
        {
            Company company = CreateCompany(CompanyCreationType.WITH_2_CHILDREN);

            Assert.AreEqual(IdentityName, company.CreatedBy);
            Assert.AreEqual(UtcNow, company.CreatedAt);

            foreach (CompanyIdentification companyIdentification in company.Identifications)
            {
                Assert.AreEqual(IdentityName, companyIdentification.CreatedBy);
                Assert.AreEqual(UtcNow, companyIdentification.CreatedAt);
            }
        }

        [Test]
        public void LastModified_Audit_Fields_Should_be_Null_After_Save()
        {
            Company company = CreateCompany(CompanyCreationType.NO_CHILDREN);

            Assert.IsNull(company.LastModifiedAt);
            Assert.IsNull(company.LastModifiedBy);
        }

        [Test]
        public void LastModified_Audit_Fields_Should_be_Null_After_Save_With_Children()
        {
            Company company = CreateCompany(CompanyCreationType.WITH_2_CHILDREN);

            Assert.IsNull(company.LastModifiedAt);
            Assert.IsNull(company.LastModifiedBy);

            Assert.AreEqual(2, company.Identifications.Count);
            foreach (CompanyIdentification companyIdentification in company.Identifications)
            {
                Assert.IsNull(companyIdentification.LastModifiedAt);
                Assert.IsNull(companyIdentification.LastModifiedBy);
            }
        }
    }
}
