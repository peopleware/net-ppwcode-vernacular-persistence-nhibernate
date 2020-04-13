// Copyright 2020 by PeopleWare n.v..
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
    /// <inheritdoc />
    [UsedImplicitly]
    public abstract class PpwAuxiliaryIndex<TEntity> : PpwAuxiliaryDatabaseObject
        where TEntity : class
    {
        protected PpwAuxiliaryIndex(IPpwHbmMapping ppwHbmMapping)
            : base(ppwHbmMapping)
        {
        }

        [CanBeNull]
        protected Func<IEnumerable<Column>>[] ColumnDefinitions { get; set; }

        [CanBeNull]
        protected Func<IEnumerable<Column>>[] CoveringColumnDefinitions { get; set; }

        [CanBeNull]
        protected virtual string Filter
            => null;

        [NotNull]
        protected abstract string GetIndexName();

        protected abstract bool IsUnique();

        [CanBeNull]
        protected virtual PersistentClass GetPersistentClassFor()
            => GetPersistentClassFor(typeof(TEntity));

        [NotNull]
        [ItemNotNull]
        protected virtual Column[] GetColumns(Expression<Func<TEntity, object>> propertyLambda)
            => base.GetColumns(propertyLambda);

        [NotNull]
        [ItemNotNull]
        protected virtual Column[] GetDiscriminatorColumnsFor([NotNull] Type type)
            => GetPersistentClassFor(type)
                   ?.Discriminator
                   ?.ColumnIterator
                   .OfType<Column>()
                   .ToArray()
               ?? EmptyColumnArray;

        [NotNull]
        [ItemNotNull]
        protected virtual string[] GetDiscriminatorValues()
            => GetDiscriminatorValuesFor(typeof(TEntity));

        public override string SqlCreateString(
            Dialect dialect,
            IMapping mapping,
            string defaultCatalog,
            string defaultSchema)
        {
            Context context = new Context(this, dialect, mapping, defaultCatalog, defaultCatalog);

            string script;
            if (ColumnDefinitions != null)
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

            sb.Append(
                IsUnique()
                    ? $"create unique index {context.IndexName} on {context.TableName}({context.ColumnNames})"
                    : $"create index {context.IndexName} on {context.TableName}({context.ColumnNames})");

            if (context.CoveringColumnNames != null)
            {
                sb.AppendLine();
                sb.Append($"  include ({context.CoveringColumnNames})");
            }

            if (context.Filter != null)
            {
                sb.AppendLine();
                sb.Append($"  where {context.Filter}");
            }

            sb.AppendLine(";");
            sb.Append("GO");
            return sb.ToString();
        }

        protected virtual string SqlCreateStringFirebird(Context context)
            => SqlCreateStringGeneric(context);

        protected virtual string SqlCreateStringPostgreSQL(Context context)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(
                IsUnique()
                    ? $"create unique index {context.IndexName} on {context.TableName}({context.ColumnNames})"
                    : $"create index {context.IndexName} on {context.TableName}({context.ColumnNames})");

            if (context.CoveringColumnNames != null)
            {
                sb.AppendLine();
                sb.Append($"  include ({context.CoveringColumnNames})");
            }

            if (context.Filter != null)
            {
                sb.AppendLine();
                sb.Append($"  where {context.Filter}");
            }

            sb.AppendLine(";");
            return sb.ToString();
        }

        protected virtual string SqlCreateStringGeneric(Context context)
        {
            StringBuilder sb = new StringBuilder();

            if (IsUnique())
            {
                sb.AppendLine($"alter table {context.TableName}");
                sb.AppendLine($"  add constraint {context.IndexName} unique({context.ColumnNames});");
            }
            else
            {
                sb.AppendLine($"create index {context.IndexName}");
                sb.AppendLine($"  on {context.TableName} ({context.ColumnNames});");
            }

            if (context.CoveringColumnNames != null)
            {
                throw new ProgrammingError($"Covering columns, {context.CoveringColumnNames}, specified for generic generation of unique constraint {context.IndexName}.");
            }

            if (context.Filter != null)
            {
                throw new ProgrammingError($"Filter, {context.Filter}, specified for generic generation of unique constraint {context.IndexName}.");
            }

            return sb.ToString();
        }

        public override string SqlDropString(Dialect dialect, string defaultCatalog, string defaultSchema)
            => string.Empty;

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "reviewed")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "reviewed")]
        protected class Context
        {
            public Context(
                [NotNull] PpwAuxiliaryIndex<TEntity> auxiliaryDatabaseObject,
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

                Table = auxiliaryDatabaseObject.GetPersistentClassFor()?.Table
                        ?? throw new ProgrammingError($"Unable to determine physical table information for entity type {typeof(TEntity).FullName}");
                string schema = defaultSchema ?? Table.Schema;
                Schema =
                    schema != null
                        ? auxiliaryDatabaseObject.QuoteSchemaName(Dialect, schema)
                        : null;

                TableName = auxiliaryDatabaseObject.QuoteTableName(Dialect, Table.Name);
                if (Schema != null)
                {
                    TableName = $"{Schema}.{TableName}";
                }

                IndexName = auxiliaryDatabaseObject.GetIndexName();

                List<Column> columns = auxiliaryDatabaseObject.ColumnDefinitions?.SelectMany(x => x()).ToList();
                if ((columns == null) || !columns.Any())
                {
                    throw new ProgrammingError($"Unable to determine the physical column(s) needed for creating a unique key for entity type {typeof(TEntity).FullName}.");
                }

                Columns = columns;
                IEnumerable<string> columnNames = Columns.Select(c => auxiliaryDatabaseObject.QuoteColumnName(dialect, c.Name));
                ColumnNames = string.Join(",", columnNames);

                CoveringColumns = auxiliaryDatabaseObject.CoveringColumnDefinitions?.SelectMany(x => x()).ToList();
                if (CoveringColumns != null)
                {
                    IEnumerable<string> coveringColumnNames = CoveringColumns.Select(c => auxiliaryDatabaseObject.QuoteColumnName(dialect, c.Name));
                    CoveringColumnNames = string.Join(",", coveringColumnNames);
                }

                Filter = auxiliaryDatabaseObject.Filter;
            }

            [NotNull]
            public PpwAuxiliaryIndex<TEntity> AuxiliaryDatabaseObject { get; }

            [NotNull]
            public Dialect Dialect { get; }

            [NotNull]
            public IMapping Mapping { get; }

            [CanBeNull]
            public string DefaultCatalog { get; }

            [CanBeNull]
            public string DefaultSchema { get; }

            [CanBeNull]
            public string Schema { get; }

            [NotNull]
            public Table Table { get; }

            [NotNull]
            public List<Column> Columns { get; }

            [CanBeNull]
            public List<Column> CoveringColumns { get; }

            [NotNull]
            public string TableName { get; }

            [NotNull]
            public string IndexName { get; }

            [NotNull]
            public string ColumnNames { get; }

            [CanBeNull]
            public string CoveringColumnNames { get; }

            [CanBeNull]
            public string Filter { get; }
        }
    }
}
