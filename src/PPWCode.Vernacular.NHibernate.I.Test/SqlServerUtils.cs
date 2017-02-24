// Copyright 2017 by PeopleWare n.v..
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
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Linq;

using PPWCode.Util.OddsAndEnds.II.ConfigHelper;
using PPWCode.Vernacular.Exceptions.II;

namespace PPWCode.Vernacular.NHibernate.I.Test
{
    public static class SqlServerUtils
    {
        private static string GetConnectionString(string dataSource, string catalog, bool pooling)
        {
            SqlConnectionStringBuilder builder =
                new SqlConnectionStringBuilder
                {
                    DataSource = dataSource,
                    InitialCatalog = catalog ?? @"master",
                    IntegratedSecurity = true,
                    Pooling = pooling
                };
            return builder.ConnectionString;
        }

        public static string GetConnectionString(string dataSource, string catalog)
        {
            return GetConnectionString(dataSource, catalog, true);
        }

        public static string GetConnectionString(string connectionString)
        {
            return ConfigHelper.GetConnectionString(connectionString);
        }

        private static SqlConnection GetConnection(string dataSource, string catalog, bool pooling)
        {
            return new SqlConnection(GetConnectionString(dataSource, catalog, pooling));
        }

        public static SqlConnection GetConnection(string dataSource, string catalog)
        {
            return new SqlConnection(GetConnectionString(dataSource, catalog));
        }

        public static SqlConnection GetConnection(string connectionString)
        {
            return new SqlConnection(GetConnectionString(connectionString));
        }

        private static void ExecuteCommands(SqlConnection connection, int commandTimeout, IEnumerable<string> scripts)
        {
            bool wasOpen = connection.State == ConnectionState.Open;
            if (!wasOpen)
            {
                connection.Open();
            }

            try
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    foreach (string script in scripts)
                    {
                        if (commandTimeout > 0)
                        {
                            command.CommandTimeout = commandTimeout;
                        }

                        command.CommandText = script;
                        command.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                if (!wasOpen)
                {
                    connection.Close();
                }
            }
        }

        private static void ExecuteCommands(string dataSource, string catalog, bool pooling, int commandTimeout, IEnumerable<string> scripts)
        {
            using (SqlConnection connection = GetConnection(dataSource, catalog, pooling))
            {
                ExecuteCommands(connection, commandTimeout, scripts);
            }
        }

        private static bool DataSourceExists(string dataSource)
        {
            bool result = dataSource == @"." || dataSource.Equals(@"localhost", StringComparison.InvariantCultureIgnoreCase);
            if (!result)
            {
                SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
                DataTable table = instance.GetDataSources();
                string[] items = dataSource.Split('\\');
                if (items.Length == 1 || items.Length == 2)
                {
                    string serverName = items[0];
                    string instanceName = items.Length == 1 ? string.Empty : items[1];
                    result = table.AsEnumerable()
                                  .Any(r => (r.Field<string>(@"ServerName") ?? string.Empty).Equals(serverName, StringComparison.InvariantCultureIgnoreCase)
                                            && (r.Field<string>(@"Instancename") ?? string.Empty).Equals(instanceName, StringComparison.InvariantCultureIgnoreCase));
                }
            }

            return result;
        }

        public static bool CatalogExists(string dataSource, string catalog)
        {
            const string CmdText = @"select null from master.dbo.sysdatabases where name=@name";

            using (SqlConnection connection = GetConnection(dataSource, null, false))
            {
                connection.Open();
                using (SqlCommand sqlCommand = new SqlCommand(CmdText, connection))
                {
                    SqlParameter param =
                        new SqlParameter
                        {
                            ParameterName = "@name",
                            Value = catalog
                        };
                    sqlCommand.Parameters.Add(param);
                    using (SqlDataReader dataReader = sqlCommand.ExecuteReader())
                    {
                        return dataReader.HasRows;
                    }
                }
            }
        }

        public static void DropCatalog(string dataSource, string catalog)
        {
            if (!DataSourceExists(dataSource))
            {
                throw new SemanticException(string.Format(@"datasource={0} unknown", dataSource));
            }

            // Put database in single user mode and force a disconnect of the other users.
            string[] commands =
            {
                string.Format(@"exec msdb.dbo.sp_delete_database_backuphistory @database_name=N'{0}'", catalog),
                string.Format(@"alter database [{0}] set single_user with rollback immediate", catalog),
                string.Format(@"drop database [{0}]", catalog)
            };
            ExecuteCommands(dataSource, null, false, 0, commands);
        }

        public static void CreateCatalog(string dataSource, string catalog, bool simpleMode)
        {
            IList<string> commands =
                new List<string>
                {
                    string.Format(@"create database [{0}]", catalog)
                };
            if (simpleMode)
            {
                commands.Add(string.Format(@"alter database [{0}] set recovery simple with no_wait", catalog));
            }

            ExecuteCommands(dataSource, null, false, 0, commands);
        }
    }
}
