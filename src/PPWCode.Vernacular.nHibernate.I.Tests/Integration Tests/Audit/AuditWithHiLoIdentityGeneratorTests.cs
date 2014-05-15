using System;

using Moq;

using NHibernate;

using NUnit.Framework;

using PPWCode.Vernacular.nHibernate.I.Implementations;
using PPWCode.Vernacular.nHibernate.I.Interfaces;
using PPWCode.Vernacular.nHibernate.I.Tests.Models;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.nHibernate.I.Tests.Audit
{
    // ReSharper disable InconsistentNaming

    public class AuditWithHiLoIdentityGeneratorTests : CompanyRepositoryTests
    {
        private readonly DateTime m_Now = DateTime.Now.ToUniversalTime();
        private const string UserName = "Danny";

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
            Assert.IsNull(company.LastModifiedAt);
            Assert.IsNull(company.LastModifiedBy);
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
    }
}