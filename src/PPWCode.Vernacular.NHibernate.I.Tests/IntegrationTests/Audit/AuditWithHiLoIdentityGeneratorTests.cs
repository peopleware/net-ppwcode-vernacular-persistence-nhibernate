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

using Moq;

using NHibernate;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.NHibernate.I.Tests.Models;
using PPWCode.Vernacular.NHibernate.I.Utilities;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.IntegrationTests.Audit
{
    public class AuditWithHiLoIdentityGeneratorTests : CompanyRepositoryTests
    {
        private const string UserName = "Danny";
        private readonly DateTime m_Now = DateTime.Now.ToUniversalTime();

        protected override ISession OpenSession()
        {
            Mock<IIdentityProvider> identityProvider = new Mock<IIdentityProvider>();
            identityProvider.Setup(ip => ip.IdentityName).Returns(UserName);

            Mock<ITimeProvider> timeProvider = new Mock<ITimeProvider>();
            timeProvider.Setup(tp => tp.Now).Returns(m_Now);

            AuditInterceptor<int> sessionLocalInterceptor = new AuditInterceptor<int>(identityProvider.Object, timeProvider.Object);
            return SessionFactory.OpenSession(sessionLocalInterceptor);
        }

        [Test]
        public void Created_Audit_Fields_Should_be_Set_After_Save()
        {
            Company company =
                new Company
                {
                    Name = "Peopleware NV"
                };

            Company savedCompany = Repository.Save(company);

            Assert.AreSame(company, savedCompany);
            Assert.AreEqual(UserName, company.CreatedBy);
            Assert.AreEqual(m_Now, company.CreatedAt);
        }

        [Test]
        public void Created_Audit_Fields_Should_be_Set_After_Save_With_Children()
        {
            CompanyIdentification companyIdentification1 =
                new CompanyIdentification
                {
                    Identification = "1"
                };
            CompanyIdentification companyIdentification2 =
                new CompanyIdentification
                {
                    Identification = "2"
                };
            Company company =
                new Company
                {
                    Name = "Peopleware NV",
                    Identifications = new[] { companyIdentification1, companyIdentification2 }
                };

            Company savedCompany = Repository.Save(company);

            Assert.AreSame(company, savedCompany);
            Assert.AreEqual(UserName, company.CreatedBy);
            Assert.AreEqual(m_Now, company.CreatedAt);

            Assert.AreEqual(2, company.Identifications.Count);
            foreach (CompanyIdentification companyIdentification in company.Identifications)
            {
                Assert.AreEqual(UserName, companyIdentification.CreatedBy);
                Assert.AreEqual(m_Now, companyIdentification.CreatedAt);
            }
        }

        [Test]
        public void LastModified_Audit_Fields_Should_be_Null_After_Save()
        {
            Company company =
                new Company
                {
                    Name = "Peopleware NV"
                };

            Company savedCompany = Repository.Save(company);

            Assert.AreSame(company, savedCompany);
            Assert.IsNull(company.LastModifiedAt);
            Assert.IsNull(company.LastModifiedBy);
        }

        [Test]
        public void LastModified_Audit_Fields_Should_be_Null_After_Save_With_Children()
        {
            CompanyIdentification companyIdentification1 =
                new CompanyIdentification
                {
                    Identification = "1"
                };
            CompanyIdentification companyIdentification2 =
                new CompanyIdentification
                {
                    Identification = "2"
                };
            Company company =
                new Company
                {
                    Name = "Peopleware NV",
                    Identifications = new[] { companyIdentification1, companyIdentification2 }
                };

            Company savedCompany = Repository.Save(company);

            Assert.AreSame(company, savedCompany);
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