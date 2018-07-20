// Copyright 2018-2018 by PeopleWare n.v..
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

using PPWCode.Util.OddsAndEnds.II.ConfigHelper;
using PPWCode.Vernacular.Exceptions.II;

using Environment = NHibernate.Cfg.Environment;

namespace PPWCode.Vernacular.NHibernate.I.Implementations.DbConstraint
{
    public abstract class DbConstraints : IDbConstraints
    {
        private static readonly ISet<DbConstraintMetadata> _emptyDbConstraintMetadatas =
            new HashSet<DbConstraintMetadata>();

        private volatile IDictionary<string, DbConstraintMetadata> _constraints;
        protected abstract string ProviderInvariantName { get; }

        public ISet<DbConstraintMetadata> Constraints
            => _constraints != null
                   ? new HashSet<DbConstraintMetadata>(_constraints.Values)
                   : _emptyDbConstraintMetadatas;

        public DbConstraintMetadata GetByConstraintName(string constraintName)
        {
            if (_constraints != null)
            {
                DbConstraintMetadata constraint;
                _constraints.TryGetValue(constraintName, out constraint);
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

        protected abstract DbCommand GetCommand(DbConnection connection, DbTransaction transaction);
        protected abstract DbConstraintMetadata GetDbConstraintMetadata(DbDataReader reader);

        protected virtual DbConnection GetConnection(IDictionary<string, string> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            if (ProviderInvariantName != null)
            {
                bool providerExists =
                    DbProviderFactories
                        .GetFactoryClasses()
                        .Rows.Cast<DataRow>()
                        .Any(r => r[2].Equals(ProviderInvariantName));
                if (providerExists)
                {
                    DbProviderFactory factory = DbProviderFactories.GetFactory(ProviderInvariantName);
                    DbConnection dbConnection = factory.CreateConnection();
                    if (dbConnection != null)
                    {
                        string connectionString = properties[Environment.ConnectionString];
                        if (connectionString == null)
                        {
                            string connectionStringName = properties[Environment.ConnectionStringName];
                            connectionString = ConfigHelper.GetConnectionString(connectionStringName);
                        }

                        dbConnection.ConnectionString = connectionString;
                        return dbConnection;
                    }
                }
            }

            throw new ProgrammingError($"Unable to figure out which provider we have to use for {properties}");
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
                                    _constraints.Add(metaData.ConstraintName, metaData);
                                }
                            }
                        }
                    }

                    transaction.Commit();
                }

                connection.Close();
            }
        }
    }
}
