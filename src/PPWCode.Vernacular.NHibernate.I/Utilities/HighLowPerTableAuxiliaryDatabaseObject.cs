// Copyright 2018 by PeopleWare n.v..
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
using System.Linq;
using System.Text;

using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Engine;

using PPWCode.Vernacular.NHibernate.I.Interfaces;

namespace PPWCode.Vernacular.NHibernate.I.Utilities
{
    public abstract class HighLowPerTableAuxiliaryDatabaseObject : PpwAuxiliaryDatabaseObject
    {
        protected HighLowPerTableAuxiliaryDatabaseObject(IPpwHbmMapping ppwHbmMapping)
            : base(ppwHbmMapping)
        {
        }

        public override string SqlCreateString(Dialect dialect, IMapping p, string defaultCatalog, string defaultSchema)
        {
            bool isSqlserver = dialect is MsSql2000Dialect;

            StringBuilder script = new StringBuilder();

            foreach (string schemaName in SchemaNames)
            {
                string generatorTableName = GetTableName(dialect, schemaName, GeneratorTableName);

                script.AppendLine($"DELETE FROM {generatorTableName};");
                if (isSqlserver)
                {
                    script.AppendLine("GO");
                }

                script.AppendLine();
                script.AppendLine($"ALTER TABLE {generatorTableName} ADD {GeneratorEntityNameColumnName} VARCHAR(128) NOT NULL;");
                if (isSqlserver)
                {
                    script.AppendLine("GO");
                }

                script.AppendLine($"ALTER TABLE {generatorTableName} ADD {GeneratorTableNameColumnName} VARCHAR(128) NOT NULL;");
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

                    script.AppendLine(
                        $"INSERT INTO {generatorTableName}" + Environment.NewLine +
                        $" ({GeneratorEntityNameColumnName}, {GeneratorNextHiColumnName}, {GeneratorTableNameColumnName})" + Environment.NewLine +
                        $"VALUES ('{fullClassName}', 0, '{tableName}');");
                }

                if (isSqlserver)
                {
                    script.AppendLine("GO");
                }
            }

            return script.ToString();
        }

        public override string SqlDropString(Dialect dialect, string defaultCatalog, string defaultSchema)
        {
            return string.Empty;
        }

        protected abstract string GeneratorTableName { get; }

        protected abstract string GeneratorEntityNameColumnName { get; }

        protected abstract string GeneratorNextHiColumnName { get; }

        protected abstract string GeneratorTableNameColumnName { get; }

        protected virtual IEnumerable<string> SchemaNames
        {
            get { return HbmClasses.Select(c => c.schema).Distinct(); }
        }

        protected virtual IEnumerable<HbmClass> HbmClasses
        {
            get
            {
                return
                    PpwHbmMapping
                        .HbmMapping
                        .RootClasses
                        .Where(c => !string.Equals(c.Id.generator.@class, "foreign"));
            }
        }

        private string RemoveBackTicks(string identifier)
        {
            return identifier?.Replace("`", string.Empty);
        }

        private string GetTableName(Dialect dialect, string schemaName, string tableName)
        {
            return
                string.IsNullOrWhiteSpace(schemaName)
                    ? QuoteTableName(dialect, RemoveBackTicks(tableName))
                    : $"{QuoteSchemaName(dialect, RemoveBackTicks(schemaName))}.{QuoteTableName(dialect, RemoveBackTicks(tableName))}";
        }
    }
}
