using System.Collections.Generic;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;

namespace PPWCode.Vernacular.NHibernate.I.Test
{
    public static class NhConfigurator
    {
        private const string ConnectionString = "Data Source=:memory:;Version=3;New=True;";

        private static readonly object s_Locker = new object();

        private static volatile Configuration s_Configuration;
        private static volatile ISessionFactory s_SessionFactory;

        public static Configuration Configuration
        {
            get
            {
                if (s_Configuration == null)
                {
                    lock (s_Locker)
                    {
                        if (s_Configuration == null)
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
                        }
                    }
                }
                return s_Configuration;
            }
        }

        public static ISessionFactory SessionFactory
        {
            get
            {
                if (s_SessionFactory == null)
                {
                    lock (s_Locker)
                    {
                        if (s_SessionFactory == null)
                        {
                            s_SessionFactory = Configuration.BuildSessionFactory();
                        }
                    }
                }
                return s_SessionFactory;
            }
        }
    }
}