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

using System;
using System.Data;

using NHibernate.Connection;

namespace PPWCode.Vernacular.NHibernate.I.Test
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
                s_Connection = null;
            }
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