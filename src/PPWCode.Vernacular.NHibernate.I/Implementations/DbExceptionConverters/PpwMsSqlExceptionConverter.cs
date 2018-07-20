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

using System;
using System.Data.SqlClient;

using NHibernate.Exceptions;

using PPWCode.Vernacular.NHibernate.I.Implementations.DbConstraint;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Implementations.DbExceptionConverters
{
    public class PpwMsSqlExceptionConverter : BaseExceptionConverter
    {
        /// <inheritdoc />
        public PpwMsSqlExceptionConverter(IViolatedConstraintNameExtracter violatedConstraintNameExtracter)
            : base(violatedConstraintNameExtracter)
        {
        }

        protected override Exception OnConvert(AdoExceptionContextInfo adoExceptionContextInfo)
        {
            SqlException sqle = ADOExceptionHelper.ExtractDbException(adoExceptionContextInfo.SqlException) as SqlException;
            if (sqle != null)
            {
                string constraintName = GetConstraintName(adoExceptionContextInfo);
                if (!string.IsNullOrWhiteSpace(constraintName))
                {
                    DbConstraintMetadata metadata = DbConstraints?.GetByConstraintName(constraintName);
                    if (metadata != null)
                    {
                        switch (metadata.ConstraintType)
                        {
                            case DbConstraintTypeEnum.PRIMARY_KEY:
                                return
                                    new DbPrimaryKeyConstraintException(
                                        sqle.Message,
                                        adoExceptionContextInfo.EntityId,
                                        adoExceptionContextInfo.EntityName,
                                        adoExceptionContextInfo.Sql,
                                        constraintName,
                                        null);

                            case DbConstraintTypeEnum.UNIQUE:
                                return
                                    new DbUniqueConstraintException(
                                        sqle.Message,
                                        adoExceptionContextInfo.EntityId,
                                        adoExceptionContextInfo.EntityName,
                                        adoExceptionContextInfo.Sql,
                                        constraintName,
                                        null);

                            case DbConstraintTypeEnum.CHECK:
                                return
                                    new DbCheckConstraintException(
                                        sqle.Message,
                                        adoExceptionContextInfo.EntityId,
                                        adoExceptionContextInfo.EntityName,
                                        adoExceptionContextInfo.Sql,
                                        constraintName,
                                        null);

                            case DbConstraintTypeEnum.FOREIGN_KEY:
                                return
                                    new DbForeignKeyConstraintException(
                                        sqle.Message,
                                        adoExceptionContextInfo.EntityId,
                                        adoExceptionContextInfo.EntityName,
                                        adoExceptionContextInfo.Sql,
                                        constraintName,
                                        null);

                            case DbConstraintTypeEnum.NOT_NULL:
                                return
                                    new DbNotNullConstraintException(
                                        sqle.Message,
                                        adoExceptionContextInfo.EntityId,
                                        adoExceptionContextInfo.EntityName,
                                        adoExceptionContextInfo.Sql,
                                        constraintName,
                                        null);
                        }
                    }
                }
            }

            return SQLStateConverter.HandledNonSpecificException(adoExceptionContextInfo.SqlException, adoExceptionContextInfo.Message, adoExceptionContextInfo.Sql);
        }
    }
}
