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
using System.Linq.Expressions;
using System.Text;

using JetBrains.Annotations;

using NHibernate.Dialect;
using NHibernate.Engine;

namespace PPWCode.Vernacular.NHibernate.II
{
    public abstract class UniqueConstraintsForNullableColumn<TEntity>
        : PpwAuxiliaryDatabaseObject
        where TEntity : class
    {
        protected UniqueConstraintsForNullableColumn([NotNull] IPpwHbmMapping ppwHbmMapping)
            : base(ppwHbmMapping)
        {
        }

        protected Expression<Func<TEntity, object>> ColumnName { get; set; }

        protected virtual void AddUniqueIndexSqlServer(Dialect dialect, StringBuilder sb, string tableName, string columnName)
        {
            sb.AppendLine($"create unique index IX_UQ_{tableName}_{columnName} on {QuoteTableName(dialect, tableName)}({QuoteColumnName(dialect, columnName)})");
            sb.AppendLine($"  where {QuoteColumnName(dialect, columnName)} is not null;");
            sb.AppendLine("GO");
        }

        protected virtual void AddUniqueConstraint(Dialect dialect, StringBuilder sb, string tableName, string columnName)
        {
            sb.AppendLine($"alter table {QuoteTableName(dialect, tableName)}");
            sb.AppendLine($"  add constraint UQ_{tableName}_{columnName} unique({QuoteColumnName(dialect, columnName)});");
        }

        public override string SqlCreateString(Dialect dialect, IMapping mapping, string defaultCatalog, string defaultSchema)
        {
            StringBuilder sb = new StringBuilder();

            if (ColumnName != null)
            {
                bool isSqlServer = dialect is MsSql2000Dialect;

                string tableName = GetTableNameFor(typeof(TEntity));
                string extendedCompanyColumnName = string.Join(",", GetColumnNames(ColumnName));

                if (isSqlServer)
                {
                    AddUniqueIndexSqlServer(dialect, sb, tableName, extendedCompanyColumnName);
                }
                else
                {
                    AddUniqueConstraint(dialect, sb, tableName, extendedCompanyColumnName);
                }
            }

            return sb.ToString();
        }

        public override string SqlDropString(Dialect dialect, string defaultCatalog, string defaultSchema)
            => string.Empty;
    }
}
