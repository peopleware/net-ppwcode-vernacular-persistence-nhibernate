// Copyright 2017-2018 by PeopleWare n.v..
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
using System.Data.SqlClient;
using System.Globalization;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Driver;
using NHibernate.Event;
using NHibernate.Mapping;

using PPWCode.Util.OddsAndEnds.II.ConfigHelper;
using PPWCode.Vernacular.NHibernate.I.Implementations.Dialects;
using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.NHibernate.I.Utilities;
using PPWCode.Vernacular.Persistence.II;

using Environment = NHibernate.Cfg.Environment;

namespace PPWCode.Vernacular.NHibernate.I.Test
{
    public abstract partial class NHibernateSqlServerFixture<TId, TAuditEntity> : NHibernateFixture<TId>
        where TId : IEquatable<TId>
        where TAuditEntity : AuditLog<TId>, new()
    {
        public class TestAuditLogEventListener : AuditLogEventListener<TId, TAuditEntity>
        {
            public TestAuditLogEventListener(IIdentityProvider identityProvider, ITimeProvider timeProvider, bool useUtc)
                : base(identityProvider, timeProvider, useUtc)
            {
            }

            protected override bool CanAuditLogFor(AbstractEvent @event, AuditLogItem auditLogItem, AuditLogActionEnum requestedLogAction)
                => true;
        }

        private Configuration _configuration;
        private string _connectionString;

        protected abstract string CatalogName { get; }
        protected abstract bool UseUtc { get; }

        protected virtual string ConnectionString
            => _connectionString ?? (_connectionString = RandomizedConnectionString);

        protected virtual string RandomizedConnectionString
        {
            get
            {
                SqlConnectionStringBuilder builder =
                    new SqlConnectionStringBuilder(FixedConnectionString);
                long ticks = DateTime.Now.Ticks;
                builder.InitialCatalog = $"{builder.InitialCatalog}.{ticks.ToString(CultureInfo.InvariantCulture)}";
                return builder.ToString();
            }
        }

        protected virtual string FixedConnectionString
            => ConfigHelper.GetConnectionString(CatalogName) ?? DefaultFixedConnectionString;

        protected virtual string DefaultFixedConnectionString
        {
            get
            {
                SqlConnectionStringBuilder builder =
                    new SqlConnectionStringBuilder
                    {
                        DataSource = "localhost",
                        InitialCatalog = CatalogName,
                        IntegratedSecurity = true
                    };
                return builder.ToString();
            }
        }

        protected override Configuration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = new Configuration()
                        .DataBaseIntegration(
                            db =>
                            {
                                db.Dialect<PpwMsSqlServerDialect>();
                                db.Driver<Sql2008ClientDriver>();
                                db.ConnectionString = ConnectionString;
                                db.IsolationLevel = IsolationLevel.ReadCommitted;
                                db.BatchSize = 0;
                            })
                        .Configure()
                        .SetProperty(Environment.ShowSql, ShowSql.ToString())
                        .SetProperty(Environment.FormatSql, FormatSql.ToString())
                        .SetProperty(Environment.GenerateStatistics, GenerateStatistics.ToString());

                    IDictionary<string, string> props = _configuration.Properties;
                    if (props.ContainsKey(Environment.ConnectionStringName))
                    {
                        props.Remove(Environment.ConnectionStringName);
                    }

                    IInterceptor interceptor = Interceptor?.GetInterceptor();
                    if (interceptor != null)
                    {
                        _configuration.SetInterceptor(interceptor);
                    }

                    foreach (IRegisterEventListener registerListener in RegisterEventListeners)
                    {
                        registerListener.Register(_configuration);
                    }

                    IPpwHbmMapping ppwHbmMapping = PpwHbmMapping;
                    if (ppwHbmMapping != null)
                    {
                        _configuration.AddMapping(ppwHbmMapping.HbmMapping);
                    }

                    foreach (IAuxiliaryDatabaseObject auxiliaryDatabaseObject in AuxiliaryDatabaseObjects)
                    {
                        IPpwAuxiliaryDatabaseObject ppwAuxiliaryDatabaseObject = auxiliaryDatabaseObject as IPpwAuxiliaryDatabaseObject;
                        ppwAuxiliaryDatabaseObject?.SetConfiguration(_configuration);
                        _configuration.AddAuxiliaryDatabaseObject(auxiliaryDatabaseObject);
                    }
                }

                return _configuration;
            }
        }

        protected virtual void ResetConfiguration()
        {
            _configuration = null;
            _connectionString = null;
        }

        protected virtual IPpwHbmMapping PpwHbmMapping
            => null;

        protected virtual IEnumerable<IAuxiliaryDatabaseObject> AuxiliaryDatabaseObjects
        {
            get { yield break; }
        }

        protected virtual IEnumerable<IRegisterEventListener> RegisterEventListeners
        {
            get
            {
                yield return new CivilizedEventListener();
                yield return new TestAuditLogEventListener(new TestIdentityProvider(IdentityName), new TestTimeProvider(UtcNow), UseUtc);
            }
        }

        protected virtual INhInterceptor Interceptor
            => null;

        protected virtual void CreateCatalog()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);
            if (SqlServerUtils.CatalogExists(builder.DataSource, builder.InitialCatalog))
            {
                SqlServerUtils.DropCatalog(builder.DataSource, builder.InitialCatalog);
            }

            SqlServerUtils.CreateCatalog(builder.DataSource, builder.InitialCatalog, true);
        }

        protected virtual void DropCatalog()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);
            if (SqlServerUtils.CatalogExists(builder.DataSource, builder.InitialCatalog))
            {
                SqlServerUtils.DropCatalog(builder.DataSource, builder.InitialCatalog);
            }
        }
    }
}
