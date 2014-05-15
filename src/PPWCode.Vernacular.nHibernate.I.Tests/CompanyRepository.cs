using Castle.Core.Logging;

using NHibernate;

using PPWCode.Vernacular.nHibernate.I.Implementations;
using PPWCode.Vernacular.nHibernate.I.Tests.Models;

namespace PPWCode.Vernacular.nHibernate.I.Tests
{
    public class CompanyRepository : Repository<Company, int>
    {
        public CompanyRepository(ISession session)
            : base(new NullLogger(), session)
        {
        }
    }
}