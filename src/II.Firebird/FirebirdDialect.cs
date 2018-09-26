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

using System.Text;

using NHibernate.Exceptions;

namespace PPWCode.Vernacular.NHibernate.II.Firebird
{
    public class FirebirdDialect : global::NHibernate.Dialect.FirebirdDialect
    {
        private IViolatedConstraintNameExtracter _violatedConstraintNameExtracter;

        /// <inheritdoc />
        public override IViolatedConstraintNameExtracter ViolatedConstraintNameExtracter
            => _violatedConstraintNameExtracter
               ?? (_violatedConstraintNameExtracter = new FirebirdViolatedConstraintNameExtracter());

        /// <inheritdoc />
        public override bool SupportsCommentOn
            => true;

        /// <inheritdoc />
        public override bool DropConstraints
            => false;

        /// <inheritdoc />
        public override ISQLExceptionConverter BuildSQLExceptionConverter()
            => new FirebirdExceptionConverter(ViolatedConstraintNameExtracter);

        /// <inheritdoc />
        public override string GetDropTableString(string tableName)
            => new StringBuilder()
                .AppendLine("execute block")
                .AppendLine("as")
                .AppendLine("declare variable relation_name char(31);")
                .AppendLine("declare variable stmt varchar(512);")
                .AppendLine("begin")
                .AppendFormat("  relation_name = '{0}';", tableName).AppendLine()
                .AppendLine("  if (exists(")
                .AppendLine("        select null")
                .AppendLine("          from rdb$relations r")
                .AppendLine("         where r.rdb$relation_name = :relation_name")
                .AppendLine("           and coalesce(r.rdb$system_flag, 0) = 0)) then begin")
                .AppendLine("    stmt = 'drop table ' || \" || :relation_name ||\"")
                .AppendLine("    execute statement stmt;")
                .AppendLine("  end")
                .AppendLine("end")
                .ToString();
    }
}
