using System.Text;

using NHibernate.Dialect;
using NHibernate.Util;

namespace PPWCode.Vernacular.NHibernate.I.Test
{
    /// <summary>
    /// Own implementation of SQLiteDialect, that ignores the schema
    /// </summary>
    // ReSharper disable once InconsistentNaming
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