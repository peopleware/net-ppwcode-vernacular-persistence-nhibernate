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

using FirebirdSql.Data.FirebirdClient;

using JetBrains.Annotations;

using NHibernate.Exceptions;

using PPWCode.Vernacular.NHibernate.II.Implementations.DbConstraint;
using PPWCode.Vernacular.NHibernate.II.Implementations.DbExceptionConverters;
using PPWCode.Vernacular.Persistence.III;

namespace PPWCode.Vernacular.NHibernate.II.Firebird
{
    public class PpwFirebirdExceptionConverter : BaseExceptionConverter
    {
        public PpwFirebirdExceptionConverter([NotNull] IViolatedConstraintNameExtracter constraintNameExtracter)
            : base(constraintNameExtracter)
        {
        }

        protected override Exception OnConvert(AdoExceptionContextInfo adoExceptionContextInfo)
        {
            if (ADOExceptionHelper.ExtractDbException(adoExceptionContextInfo.SqlException) is FbException sqle)
            {
                string constraintName = GetConstraintName(adoExceptionContextInfo);
                if (!string.IsNullOrWhiteSpace(constraintName))
                {
                    DbConstraintMetadata metadata = DbConstraints?.GetByConstraintName(constraintName);
                    if (metadata != null)
                    {
                        switch (sqle.ErrorCode)
                        {
                            case 335544349: /* no_dup               */
                            case 335544665: /* unique_key_violation */
                            {
                                DbConstraintException constraintException;
                                if (metadata.ConstraintType == DbConstraintTypeEnum.PRIMARY_KEY)
                                {
                                    constraintException =
                                        new DbPrimaryKeyConstraintException(
                                            sqle.Message,
                                            adoExceptionContextInfo.EntityId,
                                            adoExceptionContextInfo.EntityName,
                                            adoExceptionContextInfo.Sql,
                                            constraintName,
                                            null);
                                }
                                else
                                {
                                    constraintException =
                                        new DbUniqueConstraintException(
                                            sqle.Message,
                                            adoExceptionContextInfo.EntityId,
                                            adoExceptionContextInfo.EntityName,
                                            adoExceptionContextInfo.Sql,
                                            constraintName,
                                            null);
                                }

                                return constraintException;
                            }

                            case 23502: /* NOT YET CORRECT */
                            {
                                return
                                    new DbNotNullConstraintException(
                                        sqle.Message,
                                        adoExceptionContextInfo.EntityId,
                                        adoExceptionContextInfo.EntityName,
                                        adoExceptionContextInfo.Sql,
                                        constraintName,
                                        null);
                            }

                            case 335544466: /* foreign_key */
                            {
                                return
                                    new DbForeignKeyConstraintException(
                                        sqle.Message,
                                        adoExceptionContextInfo.EntityId,
                                        adoExceptionContextInfo.EntityName,
                                        adoExceptionContextInfo.Sql,
                                        constraintName,
                                        null);
                            }

                            case 335544558: /* check_constraint */
                            {
                                return
                                    new DbCheckConstraintException(
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
            }

            return SQLStateConverter.HandledNonSpecificException(adoExceptionContextInfo.SqlException, adoExceptionContextInfo.Message, adoExceptionContextInfo.Sql);
        }
    }
}
