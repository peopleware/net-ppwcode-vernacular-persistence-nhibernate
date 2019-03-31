// Copyright 2017 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Data;
#if NET461
using System.Data.Sql;
#endif
using System.Data.SqlClient;
#if NET461
using System.Linq;
#endif

using JetBrains.Annotations;

using PPWCode.Vernacular.Exceptions.IV;

namespace PPWCode.Vernacular.NHibernate.II.Test
{
    public static class SqlServerUtils
    {
        [NotNull]
        private static string GetConnectionString(
            [NotNull] string dataSource,
            [CanBeNull] string catalog,
            bool pooling)
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

        [NotNull]
        public static string GetConnectionString([NotNull] string dataSource, [CanBeNull] string catalog)
            => GetConnectionString(dataSource, catalog, true);

        [CanBeNull]
        public static string GetConnectionString([NotNull] string connectionStringKey)
            => ConfigHelper.GetConnectionString(connectionStringKey);

        [NotNull]
        private static SqlConnection GetConnection([NotNull] string dataSource, [CanBeNull] string catalog, bool pooling)
            => new SqlConnection(GetConnectionString(dataSource, catalog, pooling));

        [NotNull]
        public static SqlConnection GetConnection([NotNull] string dataSource, [CanBeNull] string catalog)
            => new SqlConnection(GetConnectionString(dataSource, catalog));

        [CanBeNull]
        public static SqlConnection GetConnection([NotNull] string connectionStringKey)
        {
            string connectionString = GetConnectionString(connectionStringKey);
            return connectionString != null ? new SqlConnection(connectionString) : null;
        }

        private static void ExecuteCommands(
            [NotNull] SqlConnection connection,
            int commandTimeout,
            [NotNull] IEnumerable<string> scripts)
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

        private static void ExecuteCommands(
            [NotNull] string dataSource,
            [CanBeNull] string catalog,
            bool pooling,
            int commandTimeout,
            [NotNull] IEnumerable<string> scripts)
        {
            using (SqlConnection connection = GetConnection(dataSource, catalog, pooling))
            {
                ExecuteCommands(connection, commandTimeout, scripts);
            }
        }

        private static bool DataSourceExists([NotNull] string dataSource)
        {
            bool result = (dataSource == @".") || dataSource.Equals(@"localhost", StringComparison.InvariantCultureIgnoreCase);
#if NET461
            if (!result)
            {
                SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
                DataTable table = instance.GetDataSources();
                string[] items = dataSource.Split('\\');
                if ((items.Length == 1) || (items.Length == 2))
                {
                    string serverName = items[0];
                    string instanceName = items.Length == 1 ? string.Empty : items[1];
                    result =
                        table
                            .AsEnumerable()
                            .Any(r => (r.Field<string>(@"ServerName") ?? string.Empty).Equals(serverName, StringComparison.InvariantCultureIgnoreCase)
                                      && (r.Field<string>(@"Instancename") ?? string.Empty).Equals(instanceName, StringComparison.InvariantCultureIgnoreCase));
                }
            }
#else
            result = true;
#endif
            return result;
        }

        public static bool CatalogExists([NotNull] string dataSource, [CanBeNull] string catalog)
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

        public static void DropCatalog([NotNull] string dataSource, [CanBeNull] string catalog)
        {
            if (!DataSourceExists(dataSource))
            {
                throw new SemanticException($"datasource={dataSource} unknown");
            }

            // Put database in single user mode and force a disconnect of the other users.
            string[] commands =
            {
                $@"exec msdb.dbo.sp_delete_database_backuphistory @database_name=N'{catalog}'",
                $"alter database [{catalog}] set single_user with rollback immediate",
                $"drop database [{catalog}]"
            };
            ExecuteCommands(dataSource, null, false, 0, commands);
        }

        public static void CreateCatalog([NotNull] string dataSource, [CanBeNull] string catalog, bool simpleMode)
        {
            IList<string> commands =
                new List<string>
                {
                    $"create database [{catalog}]"
                };
            if (simpleMode)
            {
                commands.Add($"alter database [{catalog}] set recovery simple with no_wait");
            }

            ExecuteCommands(dataSource, null, false, 0, commands);
        }
    }
}
