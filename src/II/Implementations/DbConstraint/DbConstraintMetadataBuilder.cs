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

using System.Diagnostics;

using JetBrains.Annotations;

using PPWCode.Vernacular.Persistence.III;

namespace PPWCode.Vernacular.NHibernate.II.DbConstraint
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

        public DbConstraintMetadataBuilder([NotNull] DbConstraintMetadata dbConstraintMetadata)
        {
            _constraintName = dbConstraintMetadata.ConstraintName;
        }

        [NotNull]
        [DebuggerStepThrough]
        public DbConstraintMetadataBuilder ConstraintName([NotNull] string constraintName)
        {
            _constraintName = constraintName;
            return this;
        }

        [NotNull]
        [DebuggerStepThrough]
        public DbConstraintMetadataBuilder TableName([NotNull] string tableName)
        {
            _tableName = tableName;
            return this;
        }

        [NotNull]
        [DebuggerStepThrough]
        public DbConstraintMetadataBuilder TableSchema([NotNull] string tableSchema)
        {
            _tableSchema = tableSchema;
            return this;
        }

        [NotNull]
        [DebuggerStepThrough]
        public DbConstraintMetadataBuilder DbConstraintType(DbConstraintTypeEnum dbConstraintType)
        {
            _dbConstraintType = dbConstraintType;
            return this;
        }

        [NotNull]
        [DebuggerStepThrough]
        public DbConstraintMetadataBuilder DbConstraintType([NotNull] string constraintType)
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

                case "NOT NULL":
                    _dbConstraintType = DbConstraintTypeEnum.NOT_NULL;
                    break;

                default:
                    _dbConstraintType = DbConstraintTypeEnum.UNKNOWN;
                    break;
            }

            return this;
        }

        public static implicit operator DbConstraintMetadata([NotNull] DbConstraintMetadataBuilder builder)
            => new DbConstraintMetadata(builder._constraintName, builder._tableName, builder._tableSchema, builder._dbConstraintType);
    }
}
