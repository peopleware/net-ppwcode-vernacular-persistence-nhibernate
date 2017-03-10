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
using System.Data.SqlClient;
using System.Globalization;

using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Driver;

using PPWCode.Util.OddsAndEnds.II.ConfigHelper;
using PPWCode.Vernacular.NHibernate.I.Utilities;

using Environment = NHibernate.Cfg.Environment;

namespace PPWCode.Vernacular.NHibernate.I.Test
{
    public abstract class NHibernateSqlServerFixture<TId> : NHibernateFixture<TId>
        where TId : IEquatable<TId>
    {
        private Configuration m_Configuration;
        private string m_ConnectionString;

        protected abstract string CatalogName { get; }

        protected virtual string ConnectionString
        {
            get
            {
                if (m_ConnectionString == null)
                {
                    m_ConnectionString = RandomizedConnectionString;
                }

                return m_ConnectionString;
            }
        }

        protected virtual string RandomizedConnectionString
        {
            get
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(FixedConnectionString);
                long ticks = DateTime.Now.Ticks;
                builder.InitialCatalog = string.Format("{0}.{1}", builder.InitialCatalog, ticks.ToString(CultureInfo.InvariantCulture));
                return builder.ToString();
            }
        }

        protected virtual string FixedConnectionString
        {
            get { return ConfigHelper.GetConnectionString(CatalogName) ?? DefaultFixedConnectionString; }
        }

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
                if (m_Configuration == null)
                {
                    m_Configuration = new Configuration()
                        .DataBaseIntegration(
                            db =>
                            {
                                db.Dialect<MsSql2008Dialect>();
                                db.Driver<Sql2008ClientDriver>();
                            })
                        .Configure()
                        .DataBaseIntegration(
                            db =>
                            {
                                db.Dialect<MsSql2008Dialect>();
                                db.Driver<Sql2008ClientDriver>();
                                db.ConnectionString = ConnectionString;
                                db.IsolationLevel = IsolationLevel.ReadCommitted;
                            })
                        .SetProperty(Environment.ShowSql, ShowSql.ToString())
                        .SetProperty(Environment.FormatSql, FormatSql.ToString())
                        .SetProperty(Environment.GenerateStatistics, GenerateStatistics.ToString());

                    IDictionary<string, string> props = m_Configuration.Properties;
                    if (props.ContainsKey(Environment.ConnectionStringName))
                    {
                        props.Remove(Environment.ConnectionStringName);
                    }

                    HbmMapping hbmMapping = GetHbmMapping();
                    if (hbmMapping != null)
                    {
                        m_Configuration.AddMapping(hbmMapping);
                    }

                    new CivilizedEventListener().Register(m_Configuration);
                }

                return m_Configuration;
            }
        }

        protected virtual void ResetConfiguration()
        {
            m_Configuration = null;
            m_ConnectionString = null;
        }

        protected virtual HbmMapping GetHbmMapping()
        {
            return null;
        }

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
