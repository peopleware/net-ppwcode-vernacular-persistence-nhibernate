using System;

using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Test
{
    public abstract partial class NHibernateSqlServerFixture<TId, TAuditEntity>
    {
        public class TestTimeProvider : ITimeProvider
        {
            private readonly DateTime m_Now;

            public TestTimeProvider(DateTime now)
            {
                m_Now = now;
            }

            public DateTime Now
            {
                get { return m_Now.ToLocalTime(); }
            }

            public DateTime UtcNow
            {
                get { return m_Now.ToUniversalTime(); }
            }
        }
    }
}