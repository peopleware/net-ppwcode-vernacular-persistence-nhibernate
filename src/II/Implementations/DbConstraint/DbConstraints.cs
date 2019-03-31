// Copyright 2018 by PeopleWare n.v..
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;

using JetBrains.Annotations;

using PPWCode.Vernacular.Exceptions.IV;

using Environment = NHibernate.Cfg.Environment;

namespace PPWCode.Vernacular.NHibernate.II.DbConstraint
{
    public abstract class DbConstraints : IDbConstraints
    {
        [NotNull]
        private static readonly ISet<DbConstraintMetadata> _emptyDbConstraintMetadatas =
            new HashSet<DbConstraintMetadata>();

        [CanBeNull]
        private volatile IDictionary<string, DbConstraintMetadata> _constraints;

        [CanBeNull]
        protected abstract DbProviderFactory DbProviderFactory { get; }

        public ISet<DbConstraintMetadata> Constraints
            => _constraints != null
                   ? new HashSet<DbConstraintMetadata>(_constraints.Values)
                   : _emptyDbConstraintMetadatas;

        public DbConstraintMetadata GetByConstraintName(string constraintName)
        {
            if (_constraints != null)
            {
                _constraints.TryGetValue(constraintName, out DbConstraintMetadata constraint);
                return constraint;
            }

            return null;
        }

        public void Initialize(IDictionary<string, string> properties)
        {
            if (_constraints == null)
            {
                OnInitialize(properties);
            }
        }

        [NotNull]
        protected abstract DbCommand GetCommand([NotNull] DbConnection connection, [NotNull] DbTransaction transaction);

        [NotNull]
        protected abstract DbConstraintMetadata GetDbConstraintMetadata([NotNull] DbDataReader reader);

        protected virtual DbConnection GetConnection([NotNull] IDictionary<string, string> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            DbConnection dbConnection = DbProviderFactory?.CreateConnection();
            if (dbConnection != null)
            {
                string connectionString =
                    properties.ContainsKey(Environment.ConnectionString)
                        ? properties[Environment.ConnectionString]
                        : null;
                if (connectionString == null)
                {
                    string connectionStringName =
                        properties.ContainsKey(Environment.ConnectionStringName)
                            ? properties[Environment.ConnectionStringName]
                            : null;
                    connectionString =
                        connectionStringName != null
                            ? GetConnectionString(connectionStringName)
                            : null;
                }

                dbConnection.ConnectionString = connectionString;
                return dbConnection;
            }

            throw new ProgrammingError("Unable to get a database-connection using ADO.Net");
        }

        protected virtual void OnInitialize(IDictionary<string, string> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            _constraints = new ConcurrentDictionary<string, DbConstraintMetadata>();

            using (DbConnection connection = GetConnection(properties))
            {
                connection.Open();

                using (DbTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    using (DbCommand command = GetCommand(connection, transaction))
                    {
                        using (DbDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    DbConstraintMetadata metaData = GetDbConstraintMetadata(reader);
                                    _constraints?.Add(metaData.ConstraintName, metaData);
                                }
                            }
                        }
                    }

                    transaction.Commit();
                }

                connection.Close();
            }
        }

        private static string GetConnectionString(string key)
            => ConfigurationManager.ConnectionStrings[key]?.ConnectionString;
    }
}
