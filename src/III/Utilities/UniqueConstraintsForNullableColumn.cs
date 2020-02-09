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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using JetBrains.Annotations;

using NHibernate.Dialect;
using NHibernate.Engine;
using NHibernate.Mapping;

using PPWCode.Vernacular.Exceptions.IV;

namespace PPWCode.Vernacular.NHibernate.III
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

        public override string SqlCreateString(Dialect dialect, IMapping mapping, string defaultCatalog, string defaultSchema)
        {
            Context context = new Context(this, dialect, mapping, defaultCatalog, defaultCatalog);

            string script;
            if (ColumnName != null)
            {
                if (dialect is MsSql2000Dialect)
                {
                    script = SqlCreateStringSqlServer(context);
                }
                else if (dialect is FirebirdDialect)
                {
                    script = SqlCreateStringFirebird(context);
                }
                else if (dialect is PostgreSQLDialect)
                {
                    script = SqlCreateStringPostgreSQL(context);
                }
                else
                {
                    script = SqlCreateStringGeneric(context);
                }
            }
            else
            {
                script = string.Empty;
            }

            return script;
        }

        protected virtual string SqlCreateStringSqlServer(Context context)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(
                context.QuotedSchemaName != null
                    ? $"create unique index IX_UQ_{context.TableName}_{context.ColumnName} on {context.QuotedSchemaName}.{context.QuotedTableName}({context.QuotedColumnName})"
                    : $"create unique index IX_UQ_{context.TableName}_{context.ColumnName} on {context.QuotedTableName}({context.QuotedColumnName})");
            sb.AppendLine($"  where {context.QuotedColumnName} is not null;");

            return sb.ToString();
        }

        protected virtual string SqlCreateStringFirebird(Context context)
            => SqlCreateStringGeneric(context);

        protected virtual string SqlCreateStringPostgreSQL(Context context)
            => SqlCreateStringGeneric(context);

        protected virtual string SqlCreateStringGeneric(Context context)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(
                context.QuotedSchemaName != null
                    ? $"alter table {context.QuotedSchemaName}.{context.QuotedTableName}"
                    : $"alter table {context.QuotedTableName}");
            sb.AppendLine($"  add constraint UQ_{context.TableName}_{context.ColumnName} unique({context.QuotedColumnName});");

            return sb.ToString();
        }

        public override string SqlDropString(Dialect dialect, string defaultCatalog, string defaultSchema)
            => string.Empty;

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "reviewed")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "reviewed")]
        protected class Context
        {
            public Context(
                [NotNull] UniqueConstraintsForNullableColumn<TEntity> auxiliaryDatabaseObject,
                [NotNull] Dialect dialect,
                [NotNull] IMapping mapping,
                [CanBeNull] string defaultCatalog,
                [CanBeNull] string defaultSchema)
            {
                Dialect = dialect;
                Mapping = mapping;
                DefaultCatalog = defaultCatalog;
                DefaultSchema = defaultSchema;
                AuxiliaryDatabaseObject = auxiliaryDatabaseObject;

                Table = auxiliaryDatabaseObject.GetPersistentClassFor(typeof(TEntity))?.Table
                        ?? throw new ProgrammingError($"Unable to determine physical table information for entity type {typeof(TEntity).FullName}");

                SchemaName = Table.Schema;
                QuotedSchemaName = auxiliaryDatabaseObject.QuoteSchemaName(Dialect, SchemaName);
                TableName = Table.Name;
                QuotedTableName = auxiliaryDatabaseObject.QuoteTableName(Dialect, TableName);

                Column[] columns = auxiliaryDatabaseObject.GetColumns(auxiliaryDatabaseObject.ColumnName);
                if (columns.Length != 1)
                {
                    string foundedColumnNames = string.Join(",", columns.Select(c => c.Name));
                    throw new ProgrammingError($"Unable to determine the physical column needed for creating a unique key for entity type {typeof(TEntity).FullName}, found following columns: {foundedColumnNames}");
                }

                Column = columns[0];
                ColumnName = Column.Name;
                QuotedColumnName = auxiliaryDatabaseObject.QuoteColumnName(Dialect, ColumnName);
            }

            [NotNull]
            public UniqueConstraintsForNullableColumn<TEntity> AuxiliaryDatabaseObject { get; }

            [NotNull]
            public Dialect Dialect { get; }

            [NotNull]
            public IMapping Mapping { get; }

            [CanBeNull]
            public string DefaultCatalog { get; }

            [CanBeNull]
            public string DefaultSchema { get; }

            [CanBeNull]
            public string SchemaName { get; }

            [CanBeNull]
            public string QuotedSchemaName { get; }

            [NotNull]
            public Table Table { get; set; }

            [NotNull]
            public Column Column { get; set; }

            [NotNull]
            public string TableName { get; }

            [NotNull]
            public string QuotedTableName { get; }

            [NotNull]
            public string ColumnName { get; }

            [NotNull]
            public string QuotedColumnName { get; }
        }
    }
}
