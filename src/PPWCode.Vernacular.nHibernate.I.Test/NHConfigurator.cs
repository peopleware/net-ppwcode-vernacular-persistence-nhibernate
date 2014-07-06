// Copyright 2014 by PeopleWare n.v..
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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