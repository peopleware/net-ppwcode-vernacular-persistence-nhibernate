using PPWCode.Vernacular.nHibernate.I.Interfaces;
using PPWCode.Vernacular.nHibernate.I.Tests.Models;

namespace PPWCode.Vernacular.nHibernate.I.Tests
{
    public abstract class CompanyRepositoryTests : NHibernateFixture
    {
        protected IRepository<Company, int> Repository { get; private set; }

        protected override void OnSetup()
        {
            base.OnSetup();

            Repository = new CompanyRepository(Session);
        }
    }
}