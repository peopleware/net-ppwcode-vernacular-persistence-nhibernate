using System;

using FirebirdSql.Data.FirebirdClient;

using JetBrains.Annotations;

using NHibernate.Exceptions;

using PPWCode.Vernacular.NHibernate.II.DbConstraint;
using PPWCode.Vernacular.NHibernate.II.DbExceptionConverters;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.II.Firebird
{
    public class FirebirdExceptionConverter : BaseExceptionConverter
    {
        public FirebirdExceptionConverter([NotNull] IViolatedConstraintNameExtracter constraintNameExtracter)
            : base(constraintNameExtracter)
        {
        }

        protected override Exception OnConvert(AdoExceptionContextInfo adoExceptionContextInfo)
        {
            if (ADOExceptionHelper.ExtractDbException(adoExceptionContextInfo.SqlException) is FbException sqle)
            {
                string constraintName = GetConstraintName(adoExceptionContextInfo);
                switch (sqle.ErrorCode)
                {
                    case 335544349: /* no_dup               */
                    case 335544665: /* unique_key_violation */
                    {
                        DbConstraintMetadata metadata = null;
                        if ((DbConstraints != null) && (constraintName != null))
                        {
                            metadata = DbConstraints.GetByConstraintName(constraintName);
                        }

                        DbConstraintException constraintException;
                        if ((metadata != null)
                            && (metadata.ConstraintType == DbConstraintTypeEnum.PRIMARY_KEY))
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

                    case 335544347: /* NOT NULL */
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

            return SQLStateConverter.HandledNonSpecificException(adoExceptionContextInfo.SqlException, adoExceptionContextInfo.Message, adoExceptionContextInfo.Sql);
        }
    }
}
