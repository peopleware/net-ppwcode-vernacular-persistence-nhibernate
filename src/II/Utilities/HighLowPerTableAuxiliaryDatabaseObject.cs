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

using System.Collections.Generic;
using System.Linq;
using System.Text;

using JetBrains.Annotations;

using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Engine;
using NHibernate.Mapping.ByCode;

namespace PPWCode.Vernacular.NHibernate.II
{
    public abstract class HighLowPerTableAuxiliaryDatabaseObject : PpwAuxiliaryDatabaseObject
    {
        protected HighLowPerTableAuxiliaryDatabaseObject([NotNull] IPpwHbmMapping ppwHbmMapping)
            : base(ppwHbmMapping)
        {
        }

        [NotNull]
        protected abstract string GeneratorTableName { get; }

        [NotNull]
        protected abstract string GeneratorEntityNameColumnName { get; }

        [NotNull]
        protected abstract string GeneratorNextHiColumnName { get; }

        [NotNull]
        protected abstract string GeneratorTableNameColumnName { get; }

        [NotNull]
        protected virtual IEnumerable<IGeneratorDef> GeneratorDefs
        {
            get { yield return Generators.HighLow; }
        }

        [NotNull]
        protected virtual IEnumerable<string> SchemaNames
        {
            get { return HbmClasses.Select(c => c.schema).Distinct(); }
        }

        [NotNull]
        protected virtual IEnumerable<HbmClass> HbmClasses
        {
            get
            {
                ISet<string> generatorClasses = new HashSet<string>(GeneratorDefs.Select(g => g.Class));

                return
                    PpwHbmMapping
                        .HbmMapping
                        .RootClasses
                        .Where(c => generatorClasses.Contains(c.Id.generator.@class));
            }
        }

        protected abstract int GeneratorEntityNameColumnLength([NotNull] Dialect dialect);

        protected abstract int GeneratorTableNameColumnLength([NotNull] Dialect dialect);

        [NotNull]
        public override string SqlCreateString(
            [NotNull] Dialect dialect,
            [NotNull] IMapping mapping,
            [CanBeNull] string defaultCatalog,
            [CanBeNull] string defaultSchema)
        {
            bool isSqlserver = dialect is MsSql2000Dialect;
            bool isFirebird = dialect is FirebirdDialect;

            StringBuilder script = new StringBuilder();

            if (isFirebird)
            {
                script.AppendLine("execute block");
                script.AppendLine("as");
                script.AppendLine("begin");
            }

            foreach (string schemaName in SchemaNames)
            {
                string generatorTableName = GetTableName(dialect, schemaName, GeneratorTableName);

                script.AppendLine($"DELETE FROM {generatorTableName};");
                if (isSqlserver)
                {
                    script.AppendLine("GO");
                }

                script.AppendLine();
                script.AppendLine($"ALTER TABLE {generatorTableName} ADD {GeneratorEntityNameColumnName} VARCHAR({GeneratorEntityNameColumnLength(dialect)}) NOT NULL;");
                if (isSqlserver)
                {
                    script.AppendLine("GO");
                }

                script.AppendLine($"ALTER TABLE {generatorTableName} ADD {GeneratorTableNameColumnName} VARCHAR({GeneratorTableNameColumnLength(dialect)}) NOT NULL;");
                if (isSqlserver)
                {
                    script.AppendLine("GO");
                }

                script.AppendLine();

                HashSet<HbmClass> hbmClasses = new HashSet<HbmClass>(HbmClasses.Where(c => c.schema == schemaName));
                foreach (HbmClass hbmClass in hbmClasses)
                {
                    string[] segments = hbmClass.Name.Split(',');
                    string fullClassName = segments[0];
                    string tableName = RemoveBackTicks(hbmClass.table);

                    script.AppendLine($"INSERT INTO {generatorTableName} ({GeneratorEntityNameColumnName}, {GeneratorNextHiColumnName}, {GeneratorTableNameColumnName})");
                    script.AppendLine($"VALUES ('{fullClassName}', 0, '{tableName}');");
                }

                if (isSqlserver)
                {
                    script.AppendLine("GO");
                }

                if (isFirebird)
                {
                    script.AppendLine("end");
                }
            }

            return script.ToString();
        }

        [NotNull]
        public override string SqlDropString(
            [NotNull] Dialect dialect,
            [CanBeNull] string defaultCatalog,
            [CanBeNull] string defaultSchema)
            => string.Empty;

        [ContractAnnotation("null => null; notnull => notnull")]
        private string RemoveBackTicks(string identifier)
            => identifier?.Replace("`", string.Empty);

        [CanBeNull]
        private string GetTableName(
            [NotNull] Dialect dialect,
            [CanBeNull] string schemaName,
            [CanBeNull] string tableName)
            => string.IsNullOrWhiteSpace(schemaName)
                   ? QuoteTableName(dialect, RemoveBackTicks(tableName))
                   : $"{QuoteSchemaName(dialect, RemoveBackTicks(schemaName))}.{QuoteTableName(dialect, RemoveBackTicks(tableName))}";
    }
}
