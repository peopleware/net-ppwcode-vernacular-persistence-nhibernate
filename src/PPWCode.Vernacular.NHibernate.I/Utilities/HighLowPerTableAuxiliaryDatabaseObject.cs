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
using System.Linq;
using System.Text;

using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Engine;
using NHibernate.Mapping;

using PPWCode.Vernacular.NHibernate.I.Interfaces;

namespace PPWCode.Vernacular.NHibernate.I.Utilities
{
    public abstract class HighLowPerTableAuxiliaryDatabaseObject
        : AbstractAuxiliaryDatabaseObject
    {
        protected HighLowPerTableAuxiliaryDatabaseObject(IHbmMapping hbmMapping)
        {
            HbmMapping = hbmMapping;
        }

        public IHbmMapping HbmMapping { get; }

        public override string SqlCreateString(Dialect dialect, IMapping p, string defaultCatalog, string defaultSchema)
        {
            bool isSqlserver = dialect is MsSql2000Dialect;

            StringBuilder script = new StringBuilder();

            script.AppendLine($"DELETE FROM {GeneratorTableName};");
            if (isSqlserver)
            {
                script.AppendLine("GO");
            }

            script.AppendLine();
            script.AppendLine($"ALTER TABLE {GeneratorTableName} ADD {GeneratorEntityNameColumnName} VARCHAR(128) NOT NULL;");
            if (isSqlserver)
            {
                script.AppendLine("GO");
            }

            script.AppendLine();

            HbmMapping hbmMapping = HbmMapping.GetHbmMapping();
            HashSet<string> tables =
                new HashSet<string>(
                    hbmMapping
                        .RootClasses
                        .Where(c => !string.Equals(c.Id.generator.@class, "foreign"))
                        .Select(c => c.Name),
                    StringComparer.OrdinalIgnoreCase);
            foreach (string table in tables)
            {
                script.AppendLine(
                    $"INSERT INTO {GeneratorTableName}" + Environment.NewLine +
                    $" ({GeneratorEntityNameColumnName}, {GeneratorNextHiColumnName})" + Environment.NewLine +
                    $"VALUES ('{table}', 0);");
            }

            if (isSqlserver)
            {
                script.AppendLine("GO");
            }

            return script.ToString();
        }

        protected abstract string GeneratorTableName { get; }
        protected abstract string GeneratorEntityNameColumnName { get; }
        protected abstract string GeneratorNextHiColumnName { get; }

        public override string SqlDropString(Dialect dialect, string defaultCatalog, string defaultSchema)
        {
            return string.Empty;
        }
    }
}
