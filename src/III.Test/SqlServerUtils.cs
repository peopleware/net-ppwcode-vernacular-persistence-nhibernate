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

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using JetBrains.Annotations;

using PPWCode.Vernacular.Exceptions.IV;

namespace PPWCode.Vernacular.NHibernate.III.Test
{
    public static class SqlServerUtils
    {
        [NotNull]
        private static string GetConnectionString(
            [NotNull] string sqlConnectionString,
            [CanBeNull] string catalog,
            bool pooling)
        {
            SqlConnectionStringBuilder builder =
                new SqlConnectionStringBuilder(sqlConnectionString)
                {
                    InitialCatalog = catalog ?? @"master",
                    Pooling = pooling
                };
            return builder.ConnectionString;
        }

        [NotNull]
        private static SqlConnection GetConnection(
            [NotNull] string sqlConnectionString,
            [CanBeNull] string catalog,
            bool pooling)
            => new SqlConnection(GetConnectionString(sqlConnectionString, catalog, pooling));

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
            [NotNull] string sqlConnectionString,
            [CanBeNull] string catalog,
            bool pooling,
            int commandTimeout,
            [NotNull] IEnumerable<string> scripts)
        {
            using (SqlConnection connection = GetConnection(sqlConnectionString, catalog, pooling))
            {
                ExecuteCommands(connection, commandTimeout, scripts);
            }
        }

        private static bool DataSourceExists(
            [NotNull] string sqlConnectionString)
            => true;

        public static bool CatalogExists(
            [NotNull] string sqlConnectionString,
            [CanBeNull] string catalog)
        {
            const string CmdText = @"select null from master.dbo.sysdatabases where name=@name";

            using (SqlConnection connection = GetConnection(sqlConnectionString, null, false))
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

        public static void DropCatalog(
            [NotNull] string sqlConnectionString,
            [CanBeNull] string catalog)
        {
            if (!DataSourceExists(sqlConnectionString))
            {
                string dataSource = new SqlConnectionStringBuilder(sqlConnectionString).DataSource;
                throw new SemanticException($"datasource={dataSource} unknown");
            }

            // Put database in single user mode and force a disconnect of the other users.
            string[] commands =
            {
                $@"exec msdb.dbo.sp_delete_database_backuphistory @database_name=N'{catalog}'",
                $"alter database [{catalog}] set single_user with rollback immediate",
                $"drop database [{catalog}]"
            };
            ExecuteCommands(sqlConnectionString, null, false, 0, commands);
        }

        public static void CreateCatalog(
            [NotNull] string sqlConnectionString,
            [CanBeNull] string catalog,
            bool simpleMode)
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

            ExecuteCommands(sqlConnectionString, null, false, 0, commands);
        }
    }
}
