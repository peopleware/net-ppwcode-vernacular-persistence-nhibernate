using System;

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.IntegrationTests.Linq
{
    public abstract class BaseRepositoryTests<T> : BaseQueryTests
        where T : class, IIdentity<int>
    {
        private ILinqRepository<T, int> m_Repository;

        protected abstract Func<ILinqRepository<T, int>> RepositoryFactory { get; }

        protected ILinqRepository<T, int> Repository
        {
            get { return m_Repository; }
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            m_Repository = RepositoryFactory();
            SessionFactory.Statistics.Clear();
        }

        protected override void OnTeardown()
        {
            m_Repository = null;

            base.OnTeardown();
        }
    }
}