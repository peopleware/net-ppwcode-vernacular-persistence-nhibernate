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

using System.Data.Common;

using FirebirdSql.Data.FirebirdClient;

using PPWCode.Vernacular.NHibernate.II.DbConstraint;

namespace PPWCode.Vernacular.NHibernate.II.Firebird
{
    /// <inheritdoc cref="SchemaBasedDbConstraints" />
    public class FirebirdDbConstraints : SchemaBasedDbConstraints
    {
        /// <inheritdoc />
        protected override DbProviderFactory DbProviderFactory
            => FirebirdClientFactory.Instance;

        /// <inheritdoc />
        protected override string CommandText
            => @"
select x.constraint_name,
       x.table_name,
       '' as table_schema,
       x.constraint_type
  from (select trim(both from rc.rdb$constraint_name),
               trim(both from rc.rdb$relation_name),
               trim(both from rc.rdb$constraint_type)
          from rdb$relation_constraints rc
               join rdb$relations r on rc.rdb$relation_name = r.rdb$relation_name
         where coalesce(r.rdb$system_flag, 0) = 0
        union all
        select trim(both from i.rdb$index_name),
               trim(both from i.rdb$relation_name),
               'UNIQUE'
          from rdb$indices i
               join rdb$relations r on i.rdb$relation_name = r.rdb$relation_name
               left join rdb$relation_constraints rc on i.rdb$index_name = rc.rdb$index_name
         where coalesce(r.rdb$system_flag, 0) = 0
           and coalesce(i.rdb$unique_flag, 0) = 1
           and rc.rdb$constraint_name is null) x (constraint_name, table_name, constraint_type)
";
    }
}
