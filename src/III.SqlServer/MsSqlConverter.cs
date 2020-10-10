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
using System.Data.SqlClient;

using JetBrains.Annotations;

using NHibernate.Exceptions;

using PPWCode.Vernacular.NHibernate.III.DbConstraint;
using PPWCode.Vernacular.NHibernate.III.DbExceptionConverters;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.SqlServer
{
    public class MsSqlConverter : BaseExceptionConverter
    {
        public MsSqlConverter([NotNull] IViolatedConstraintNameExtracter violatedConstraintNameExtracter)
            : base(violatedConstraintNameExtracter)
        {
        }

        protected override Exception OnConvert(AdoExceptionContextInfo adoExceptionContextInfo)
        {
            if (ADOExceptionHelper.ExtractDbException(adoExceptionContextInfo.SqlException) is SqlException sqle)
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

                // check if one of the underlying errors indicates an update conflict
                foreach (SqlError sqlError in sqle.Errors)
                {
                    // update conflict
                    if ((sqlError.Number == 3960) || (sqlError.Number == 3961))
                    {
                        return new DbUpdateConflictException(
                            sqle.Message,
                            adoExceptionContextInfo.EntityId,
                            adoExceptionContextInfo.EntityName,
                            adoExceptionContextInfo.Sql,
                            sqle);
                    }
                }
            }

            return SQLStateConverter.HandledNonSpecificException(adoExceptionContextInfo.SqlException, adoExceptionContextInfo.Message, adoExceptionContextInfo.Sql);
        }
    }
}
