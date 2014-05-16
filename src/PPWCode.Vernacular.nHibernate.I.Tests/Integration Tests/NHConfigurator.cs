using System.Collections.Generic;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;

namespace PPWCode.Vernacular.nHibernate.I.Tests
{
    public static class NhConfigurator
    {
        private const string ConnectionString = "Data Source=:memory:;Version=3;New=True;";

        private static readonly Configuration s_Configuration;
        private static readonly ISessionFactory s_SessionFactory;

        static NhConfigurator()
        {
            s_Configuration = new Configuration()
                .Configure()
                .DataBaseIntegration(
                    db =>
                    {
                        db.Dialect<SQLiteDialect>();
                        db.Driver<SQLite20Driver>();
                        db.ConnectionProvider<TestConnectionProvider>();
                        db.ConnectionString = ConnectionString;
                    })
                .SetProperty(Environment.CurrentSessionContextClass, "thread_static");

            IDictionary<string, string> props = s_Configuration.Properties;
            if (props.ContainsKey(Environment.ConnectionStringName))
            {
                props.Remove(Environment.ConnectionStringName);
            }

            s_SessionFactory = s_Configuration.BuildSessionFactory();
        }

        public static Configuration Configuration
        {
            get { return s_Configuration; }
        }

        public static ISessionFactory SessionFactory
        {
            get { return s_SessionFactory; }
        }
    }
}