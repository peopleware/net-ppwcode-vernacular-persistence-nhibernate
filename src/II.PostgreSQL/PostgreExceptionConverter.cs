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

using JetBrains.Annotations;

using NHibernate.Exceptions;

using Npgsql;

using PPWCode.Vernacular.NHibernate.II.DbConstraint;
using PPWCode.Vernacular.NHibernate.II.DbExceptionConverters;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.II.PostgreSQL
{
    public class PostgreExceptionConverter : BaseExceptionConverter
    {
        public PostgreExceptionConverter([NotNull] IViolatedConstraintNameExtracter constraintNameExtracter)
            : base(constraintNameExtracter)
        {
        }

        protected override Exception OnConvert(AdoExceptionContextInfo adoExceptionContextInfo)
        {
            if (ADOExceptionHelper.ExtractDbException(adoExceptionContextInfo.SqlException) is PostgresException sqle)
            {
                string constraintName = GetConstraintName(adoExceptionContextInfo);
                if (!string.IsNullOrWhiteSpace(constraintName))
                {
                    string extraInfo = $"{sqle.Severity} : {sqle.Detail}";
                    DbConstraintMetadata metadata = DbConstraints?.GetByConstraintName(constraintName);
                    if (metadata != null)
                    {
                        if (int.TryParse(sqle.SqlState, out int code))
                        {
                            switch (code)
                            {
                                case 23505:
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
                                                extraInfo);
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
                                                extraInfo);
                                    }

                                    return constraintException;
                                }

                                case 23502:
                                {
                                    return
                                        new DbNotNullConstraintException(
                                            sqle.Message,
                                            adoExceptionContextInfo.EntityId,
                                            adoExceptionContextInfo.EntityName,
                                            adoExceptionContextInfo.Sql,
                                            constraintName,
                                            extraInfo);
                                }

                                case 23503:
                                {
                                    return
                                        new DbForeignKeyConstraintException(
                                            sqle.Message,
                                            adoExceptionContextInfo.EntityId,
                                            adoExceptionContextInfo.EntityName,
                                            adoExceptionContextInfo.Sql,
                                            constraintName,
                                            extraInfo);
                                }

                                case 23514:
                                {
                                    return
                                        new DbCheckConstraintException(
                                            sqle.Message,
                                            adoExceptionContextInfo.EntityId,
                                            adoExceptionContextInfo.EntityName,
                                            adoExceptionContextInfo.Sql,
                                            constraintName,
                                            extraInfo);
                                }
                            }
                        }
                    }
                }
            }

            return SQLStateConverter.HandledNonSpecificException(adoExceptionContextInfo.SqlException, adoExceptionContextInfo.Message, adoExceptionContextInfo.Sql);
        }
    }
}
