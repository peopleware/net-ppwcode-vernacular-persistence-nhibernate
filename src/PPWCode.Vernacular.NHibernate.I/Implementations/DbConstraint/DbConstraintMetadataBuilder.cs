// Copyright 2018-2018 by PeopleWare n.v..
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

using System.Diagnostics;

using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Implementations.DbConstraint
{
    public class DbConstraintMetadataBuilder
    {
        private string _constraintName;
        private DbConstraintTypeEnum _dbConstraintType;
        private string _tableName;
        private string _tableSchema;

        public DbConstraintMetadataBuilder()
        {
        }

        public DbConstraintMetadataBuilder(DbConstraintMetadata dbConstraintMetadata)
        {
            _constraintName = dbConstraintMetadata.ConstraintName;
        }

        [DebuggerStepThrough]
        public DbConstraintMetadataBuilder ConstraintName(string constraintName)
        {
            _constraintName = constraintName;
            return this;
        }

        [DebuggerStepThrough]
        public DbConstraintMetadataBuilder TableName(string tableName)
        {
            _tableName = tableName;
            return this;
        }

        [DebuggerStepThrough]
        public DbConstraintMetadataBuilder TableSchema(string tableSchema)
        {
            _tableSchema = tableSchema;
            return this;
        }

        [DebuggerStepThrough]
        public DbConstraintMetadataBuilder DbConstraintType(DbConstraintTypeEnum dbConstraintType)
        {
            _dbConstraintType = dbConstraintType;
            return this;
        }

        [DebuggerStepThrough]
        public DbConstraintMetadataBuilder DbConstraintType(string constraintType)
        {
            switch (constraintType)
            {
                case "PRIMARY KEY":
                    _dbConstraintType = DbConstraintTypeEnum.PRIMARY_KEY;
                    break;

                case "UNIQUE":
                    _dbConstraintType = DbConstraintTypeEnum.UNIQUE;
                    break;

                case "FOREIGN KEY":
                    _dbConstraintType = DbConstraintTypeEnum.FOREIGN_KEY;
                    break;

                case "CHECK":
                    _dbConstraintType = DbConstraintTypeEnum.CHECK;
                    break;

                default:
                    _dbConstraintType = DbConstraintTypeEnum.UNKNOWN;
                    break;
            }

            return this;
        }

        public static implicit operator DbConstraintMetadata(DbConstraintMetadataBuilder builder)
            => new DbConstraintMetadata(builder._constraintName, builder._tableName, builder._tableSchema, builder._dbConstraintType);
    }
}
