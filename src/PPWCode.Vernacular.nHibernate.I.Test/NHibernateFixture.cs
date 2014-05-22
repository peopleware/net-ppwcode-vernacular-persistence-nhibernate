using HibernatingRhinos.Profiler.Appender.NHibernate;

using log4net.Config;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Tool.hbm2ddl;

namespace PPWCode.Vernacular.NHibernate.I.Test
{
    public abstract class NHibernateFixture : BaseFixture
    {
        protected ISessionFactory SessionFactory
        {
            get { return NhConfigurator.SessionFactory; }
        }

        protected virtual ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }

        protected ISession Session
        {
            get { return SessionFactory.GetCurrentSession(); }
        }

        protected override void OnSetup()
        {
            XmlConfigurator.Configure();
            NHibernateProfiler.Initialize();
            SetupNHibernateSession();
            base.OnSetup();
        }

        protected override void OnTeardown()
        {
            TearDownNHibernateSession();
            NHibernateProfiler.Stop();
            base.OnTeardown();
        }

        protected void SetupNHibernateSession()
        {
            TestConnectionProvider.CloseDatabase();
            SetupContextualSession();
            BuildSchema();
        }

        protected void TearDownNHibernateSession()
        {
            TearDownContextualSession();
            TestConnectionProvider.CloseDatabase();
        }

        private void SetupContextualSession()
        {
            ISession session = OpenSession();
            CurrentSessionContext.Bind(session);
        }

        private void TearDownContextualSession()
        {
            ISessionFactory sessionFactory = NhConfigurator.SessionFactory;
            ISession session = CurrentSessionContext.Unbind(sessionFactory);
            session.Close();
        }

        private void BuildSchema()
        {
            Configuration cfg = NhConfigurator.Configuration;
            SchemaExport schemaExport = new SchemaExport(cfg);
            schemaExport.Create(false, true);
        }
    }
}