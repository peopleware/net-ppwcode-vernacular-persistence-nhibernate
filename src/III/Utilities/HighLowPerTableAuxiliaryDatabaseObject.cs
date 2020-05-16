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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using JetBrains.Annotations;

using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Engine;
using NHibernate.Mapping.ByCode;

namespace PPWCode.Vernacular.NHibernate.III
{
    /// <inheritdoc />
    public abstract class HighLowPerTableAuxiliaryDatabaseObject : PpwAuxiliaryDatabaseObject
    {
        protected HighLowPerTableAuxiliaryDatabaseObject([JetBrains.Annotations.NotNull] IPpwHbmMapping ppwHbmMapping)
            : base(ppwHbmMapping)
        {
        }

        [JetBrains.Annotations.NotNull]
        protected abstract string GeneratorTableName { get; }

        [JetBrains.Annotations.NotNull]
        protected abstract string GeneratorEntityNameColumnName { get; }

        [JetBrains.Annotations.NotNull]
        protected abstract string GeneratorNextHiColumnName { get; }

        [JetBrains.Annotations.NotNull]
        protected abstract string GeneratorTableNameColumnName { get; }

        [JetBrains.Annotations.NotNull]
        protected virtual IEnumerable<IGeneratorDef> GeneratorDefs
        {
            get { yield return Generators.HighLow; }
        }

        [JetBrains.Annotations.NotNull]
        protected virtual IEnumerable<string> SchemaNames
        {
            get { return HbmClasses.Select(c => c.schema).Distinct(); }
        }

        [JetBrains.Annotations.NotNull]
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

        protected abstract int GeneratorEntityNameColumnLength([JetBrains.Annotations.NotNull] Dialect dialect);

        protected abstract int GeneratorTableNameColumnLength([JetBrains.Annotations.NotNull] Dialect dialect);

        [JetBrains.Annotations.NotNull]
        public override string SqlCreateString(
            [JetBrains.Annotations.NotNull] Dialect dialect,
            [JetBrains.Annotations.NotNull] IMapping mapping,
            [CanBeNull] string defaultCatalog,
            [CanBeNull] string defaultSchema)
        {
            Context context = new Context(this, dialect, mapping, defaultCatalog, defaultCatalog);

            string script;
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

            return script;
        }

        [JetBrains.Annotations.NotNull]
        public virtual string SqlCreateStringSqlServer([JetBrains.Annotations.NotNull] Context context)
        {
            StringBuilder script = new StringBuilder();
            foreach (string schemaName in SchemaNames)
            {
                string generatorTableName = context.GetTableName(schemaName);
                script.AppendLine($"DELETE FROM {generatorTableName};");
                script.AppendLine("GO");
                script.AppendLine($"ALTER TABLE {generatorTableName} ADD {context.EntityNameColumnName} VARCHAR({GeneratorEntityNameColumnLength(context.Dialect)}) NOT NULL;");
                script.AppendLine("GO");
                script.AppendLine($"ALTER TABLE {generatorTableName} ADD {context.TableNameColumnName} VARCHAR({GeneratorTableNameColumnLength(context.Dialect)}) NOT NULL;");
                script.AppendLine("GO");
                script.AppendLine($"ALTER TABLE {generatorTableName} ADD PRIMARY KEY ({context.EntityNameColumnName});");
                script.AppendLine("GO");

                HashSet<HbmClass> hbmClasses = new HashSet<HbmClass>(HbmClasses.Where(c => c.schema == schemaName));
                foreach (HbmClass hbmClass in hbmClasses)
                {
                    string[] segments = hbmClass.Name.Split(',');
                    string fullClassName = segments[0];
                    string tableName = RemoveBackTicks(hbmClass.table);

                    script.AppendLine($"INSERT INTO {generatorTableName} ({context.NextHiColumnName}, {context.EntityNameColumnName}, {context.TableNameColumnName})");
                    script.AppendLine($"VALUES (0, '{fullClassName}', '{tableName}');");
                }

                script.AppendLine("GO");
            }

            return script.ToString();
        }

        [JetBrains.Annotations.NotNull]
        public virtual string SqlCreateStringFirebird([JetBrains.Annotations.NotNull] Context context)
        {
            StringBuilder script = new StringBuilder();

            script.AppendLine("execute block");
            script.AppendLine("as");
            script.AppendLine("begin");

            foreach (string schemaName in SchemaNames)
            {
                string generatorTableName = context.GetTableName(schemaName);
                script.AppendLine($"  DELETE FROM {generatorTableName};");
                script.AppendLine($"  execute statement 'ALTER TABLE {generatorTableName} ADD {context.EntityNameColumnName} VARCHAR({GeneratorEntityNameColumnLength(context.Dialect)}) NOT NULL';");
                script.AppendLine($"  execute statement 'ALTER TABLE {generatorTableName} ADD {context.TableNameColumnName} VARCHAR({GeneratorTableNameColumnLength(context.Dialect)}) NOT NULL';");
                script.AppendLine($"  execute statement 'ALTER TABLE {generatorTableName} ADD PRIMARY KEY ({context.EntityNameColumnName})';");
            }

            script.AppendLine("end");
            return script.ToString();
        }

        [JetBrains.Annotations.NotNull]
        public virtual string SqlCreateStringPostgreSQL([JetBrains.Annotations.NotNull] Context context)
            => SqlCreateStringGeneric(context);

        [JetBrains.Annotations.NotNull]
        public virtual string SqlCreateStringGeneric([JetBrains.Annotations.NotNull] Context context)
        {
            StringBuilder script = new StringBuilder();
            foreach (string schemaName in SchemaNames)
            {
                string generatorTableName = context.GetTableName(schemaName);
                script.AppendLine($"DELETE FROM {generatorTableName};");
                script.AppendLine($"ALTER TABLE {generatorTableName} ADD {context.EntityNameColumnName} VARCHAR({GeneratorEntityNameColumnLength(context.Dialect)}) NOT NULL;");
                script.AppendLine($"ALTER TABLE {generatorTableName} ADD {context.TableNameColumnName} VARCHAR({GeneratorTableNameColumnLength(context.Dialect)}) NOT NULL;");
                script.AppendLine($"ALTER TABLE {generatorTableName} ADD PRIMARY KEY ({context.EntityNameColumnName});");

                HashSet<HbmClass> hbmClasses = new HashSet<HbmClass>(HbmClasses.Where(c => c.schema == schemaName));
                foreach (HbmClass hbmClass in hbmClasses)
                {
                    string[] segments = hbmClass.Name.Split(',');
                    string fullClassName = segments[0];
                    string tableName = RemoveBackTicks(hbmClass.table);

                    script.AppendLine($"INSERT INTO {generatorTableName} ({context.NextHiColumnName}, {context.EntityNameColumnName}, {context.TableNameColumnName})");
                    script.AppendLine($"VALUES (0, '{fullClassName}', '{tableName}');");
                }
            }

            return script.ToString();
        }

        [JetBrains.Annotations.NotNull]
        public override string SqlDropString(
            [JetBrains.Annotations.NotNull] Dialect dialect,
            [CanBeNull] string defaultCatalog,
            [CanBeNull] string defaultSchema)
            => string.Empty;

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "reviewed")]
        public class Context
        {
            public Context(
                [JetBrains.Annotations.NotNull] HighLowPerTableAuxiliaryDatabaseObject auxiliaryDatabaseObject,
                [JetBrains.Annotations.NotNull] Dialect dialect,
                [JetBrains.Annotations.NotNull] IMapping mapping,
                [CanBeNull] string defaultCatalog,
                [CanBeNull] string defaultSchema)
            {
                Dialect = dialect;
                Mapping = mapping;
                DefaultCatalog = defaultCatalog;
                DefaultSchema = defaultSchema;
                AuxiliaryDatabaseObject = auxiliaryDatabaseObject;
                NextHiColumnName = auxiliaryDatabaseObject.PpwHbmMapping.GetIdentifier(auxiliaryDatabaseObject.GeneratorNextHiColumnName);
                EntityNameColumnName = auxiliaryDatabaseObject.PpwHbmMapping.GetIdentifier(auxiliaryDatabaseObject.GeneratorEntityNameColumnName);
                TableNameColumnName = auxiliaryDatabaseObject.PpwHbmMapping.GetIdentifier(auxiliaryDatabaseObject.GeneratorTableNameColumnName);
            }

            [JetBrains.Annotations.NotNull]
            public HighLowPerTableAuxiliaryDatabaseObject AuxiliaryDatabaseObject { get; }

            [JetBrains.Annotations.NotNull]
            public Dialect Dialect { get; }

            [JetBrains.Annotations.NotNull]
            public IMapping Mapping { get; }

            [CanBeNull]
            public string DefaultCatalog { get; }

            [CanBeNull]
            public string DefaultSchema { get; }

            [JetBrains.Annotations.NotNull]
            public string NextHiColumnName { get; }

            [JetBrains.Annotations.NotNull]
            public string EntityNameColumnName { get; }

            [JetBrains.Annotations.NotNull]
            public string TableNameColumnName { get; }

            [JetBrains.Annotations.NotNull]
            public string GetTableName(string schemaName)
            {
                string quotedSchemaName = AuxiliaryDatabaseObject.QuoteSchemaName(Dialect, AuxiliaryDatabaseObject.RemoveBackTicks(schemaName));
                string quotedGeneratorTableName = AuxiliaryDatabaseObject.QuoteTableName(Dialect, AuxiliaryDatabaseObject.PpwHbmMapping.GetIdentifier(AuxiliaryDatabaseObject.GeneratorTableName));
                return
                    string.IsNullOrEmpty(quotedSchemaName)
                        ? quotedGeneratorTableName
                        : $"{quotedSchemaName}.{quotedGeneratorTableName}";
            }
        }
    }
}
