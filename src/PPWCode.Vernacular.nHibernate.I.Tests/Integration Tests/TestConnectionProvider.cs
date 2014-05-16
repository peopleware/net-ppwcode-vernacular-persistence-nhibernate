using System;
using System.Data;

using NHibernate.Connection;

namespace PPWCode.Vernacular.nHibernate.I.Tests
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TestConnectionProvider : DriverConnectionProvider
    {
        [ThreadStatic]
        private static IDbConnection s_Connection;

        public static void CloseDatabase()
        {
            if (s_Connection != null)
            {
                s_Connection.Dispose();
            }
            s_Connection = null;
        }

        public override IDbConnection GetConnection()
        {
            if (s_Connection == null)
            {
                s_Connection = Driver.CreateConnection();
                s_Connection.ConnectionString = ConnectionString;
                s_Connection.Open();
            }
            return s_Connection;
        }

        public override void CloseConnection(IDbConnection conn)
        {
            // Do nothing
        }
    }
}