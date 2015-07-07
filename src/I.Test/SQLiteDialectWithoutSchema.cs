// Copyright 2014 by PeopleWare n.v..
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

using System.Text;

using NHibernate.Dialect;
using NHibernate.Util;

namespace PPWCode.Vernacular.NHibernate.I.Test
{
    /// <summary>
    ///     Own implementation of SQLiteDialect, that ignores the schema.
    ///     This was necessary for making the behavior of the NHibernate dialect
    ///     consistent with the behavior of the FluentMigrator SQLite dialect.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedMember.Global
    public class SQLiteDialectWithoutSchema
        : SQLiteDialect
    {
        public override string Qualify(string catalog, string schema, string table)
        {
            StringBuilder qualifiedName = new StringBuilder();
            bool quoted = false;

            if (!string.IsNullOrEmpty(catalog))
            {
                if (catalog.StartsWith(OpenQuote.ToString()))
                {
                    catalog = catalog.Substring(1, catalog.Length - 1);
                    quoted = true;
                }

                if (catalog.EndsWith(CloseQuote.ToString()))
                {
                    catalog = catalog.Substring(0, catalog.Length - 1);
                    quoted = true;
                }

                qualifiedName.Append(catalog).Append(StringHelper.Underscore);
            }

            if (table.StartsWith(OpenQuote.ToString()))
            {
                table = table.Substring(1, table.Length - 1);
                quoted = true;
            }

            if (table.EndsWith(CloseQuote.ToString()))
            {
                table = table.Substring(0, table.Length - 1);
                quoted = true;
            }

            string name = qualifiedName.Append(table).ToString();
            if (quoted)
            {
                return OpenQuote + name + CloseQuote;
            }

            return name;
        }
    }
}