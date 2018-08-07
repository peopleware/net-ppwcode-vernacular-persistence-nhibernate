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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace PPWCode.Vernacular.NHibernate.II.Implementations.DbConstraint
{
    /// <inheritdoc cref="DbConstraints" />
    public abstract class InformationSchemaBasedDbConstraints : DbConstraints
    {
        /// <summary>
        ///     Get all the schema's within our current catalog.
        /// </summary>
        protected abstract IEnumerable<string> Schemas { get; }

        protected virtual string SqlCommand
            => @"
select tc.CONSTRAINT_NAME,
       tc.TABLE_NAME,
       tc.TABLE_SCHEMA,
       tc.CONSTRAINT_TYPE
  from INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
 where tc.CONSTRAINT_CATALOG = @catalog
   and tc.CONSTRAINT_SCHEMA in ({0})";

        /// <inheritdoc />
        protected override DbCommand GetCommand(DbConnection connection, DbTransaction transaction)
        {
            DbCommand command = connection.CreateCommand();
            try
            {
                string sqlSchemasFragment = string.Join(", ", Schemas.Select(s => $"'{s}'"));
                command.CommandText = string.Format(SqlCommand, sqlSchemasFragment);
                command.CommandType = CommandType.Text;
                command.Transaction = transaction;

                // Catalog parameter
                DbParameter catalogParameter = command.CreateParameter();
                catalogParameter.DbType = DbType.AnsiString;
                catalogParameter.Direction = ParameterDirection.Input;
                catalogParameter.ParameterName = "@catalog";
                catalogParameter.IsNullable = false;
                catalogParameter.Value = connection.Database;
                command.Parameters.Add(catalogParameter);
            }
            catch (Exception)
            {
                command.Dispose();
                throw;
            }

            return command;
        }

        /// <inheritdoc />
        protected override DbConstraintMetadata GetDbConstraintMetadata(DbDataReader reader)
        {
            int index = 0;
            string constraintName = GetNullableString(reader, index++);
            string tableName = GetNullableString(reader, index++);
            string tableSchema = GetNullableString(reader, index++);
            string constraintType = GetNullableString(reader, index);

            return
                new DbConstraintMetadataBuilder()
                    .ConstraintName(constraintName)
                    .TableSchema(tableSchema)
                    .TableName(tableName)
                    .DbConstraintType(constraintType);
        }

        private string GetNullableString(DbDataReader reader, int index)
        {
            object value = reader.GetValue(index);
            if ((value == null) || (value == DBNull.Value))
            {
                return null;
            }

            return Convert.ToString(value);
        }
    }
}
