using System.Linq;

using NHibernate;

using PPWCode.Vernacular.NHibernate.I.Tests.Models;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Repositories
{
    public class UserLinqRepository
        : TestLinqRepository<User>,
          IUserLinqRepository
    {
        public UserLinqRepository(ISession session)
            : base(session)
        {
        }

        public User GetUserByName(string name)
        {
            return Get(qry => qry.Where(u => u.Name == name));
        }
    }
}