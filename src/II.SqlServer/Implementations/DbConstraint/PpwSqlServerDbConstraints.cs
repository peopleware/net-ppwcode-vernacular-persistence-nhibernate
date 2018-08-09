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

using PPWCode.Vernacular.NHibernate.II.Implementations.DbConstraint;

namespace PPWCode.Vernacular.NHibernate.II.SqlServer.Implementations.DbConstraint
{
    /// <inheritdoc cref="InformationSchemaBasedDbConstraints" />
    public class PpwSqlServerDbConstraints : InformationSchemaBasedDbConstraints
    {
        /// <inheritdoc />
        protected override IEnumerable<string> Schemas
        {
            get { yield return "dbo"; }
        }

        /// <inheritdoc />
        protected override string ProviderInvariantName
            => "System.Data.SqlClient";

        /// <inheritdoc />
        protected override string SqlCommand
            => @"
select tc.CONSTRAINT_NAME,
       tc.TABLE_NAME,
       tc.TABLE_SCHEMA,
       tc.CONSTRAINT_TYPE
  from INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
 where tc.CONSTRAINT_CATALOG = 'Phoenix'
   and tc.CONSTRAINT_SCHEMA in ('dbo')
union all
select i.[name] as CONSTRAINT_NAME,
       o.[name] as TABLE_NAME,
       schema_name(o.schema_id) as TABLE_SCHEMA,
       'UNIQUE' as CONSTRAINT_TYPE
  from sys.indexes i
       join sys.objects o on i.object_id = o.object_id 
 where i.is_unique = 1
   and i.is_unique_constraint = 0
   and i.is_primary_key = 0
   and o.type = 'U'";
    }
}
