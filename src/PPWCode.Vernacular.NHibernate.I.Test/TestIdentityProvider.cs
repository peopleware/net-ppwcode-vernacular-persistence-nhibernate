using PPWCode.Vernacular.NHibernate.I.Interfaces;

namespace PPWCode.Vernacular.NHibernate.I.Test
{
    public abstract partial class NHibernateSqlServerFixture<TId, TAuditEntity>
    {
        public class TestIdentityProvider : IIdentityProvider
        {
            private readonly string m_IdentityName;

            public TestIdentityProvider(string identityName)
            {
                m_IdentityName = identityName;
            }

            public string IdentityName
            {
                get { return m_IdentityName; }
            }
        }
    }
}