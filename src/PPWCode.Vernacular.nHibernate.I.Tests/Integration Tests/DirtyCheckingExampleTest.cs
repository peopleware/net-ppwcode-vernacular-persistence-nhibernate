using NHibernate;

using NUnit.Framework;

using PPWCode.Vernacular.nHibernate.I.Tests.Models;
using PPWCode.Vernacular.nHibernate.I.Utilities;

namespace PPWCode.Vernacular.nHibernate.I.Tests
{
    public class DirtyCheckingExampleTests : CompanyRepositoryTests
    {
        [Test]
        public void DirtyCheckingTest()
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
            Repository.Save(company);
            Session.Evict(company);

            /* Set Number to null (but Number is defined as int) */
            /*
            using (ISession session = NhConfigurator.SessionFactory.OpenSession())
            {
                using (ITransaction trans = session.BeginTransaction())
                {
                    session.CreateQuery(@"update CompanyIdentification set Number = null").ExecuteUpdate();
                    trans.Commit();
                }
            }
            */
            new DirtyChecking(NhConfigurator.Configuration, NhConfigurator.SessionFactory, Assert.Fail, Assert.Inconclusive).Test();
        }
    }
}