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

using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

using NHibernate.Exceptions;

using PPWCode.Vernacular.NHibernate.I.Implementations.DbConstraint;

namespace PPWCode.Vernacular.NHibernate.I.Implementations.ViolatedConstraintNameExtracters
{
    public class PpwSqlServerViolatedConstraintNameExtracter
        : IViolatedConstraintNameExtracter,
          IDbConstraints
    {
        private readonly IDbConstraints _dbConstraints;

        public PpwSqlServerViolatedConstraintNameExtracter()
        {
            _dbConstraints = new PpwSqlServerDbConstraints();
        }

        /// <inheritdoc />
        public DbConstraintMetadata GetByConstraintName(string constraintName)
            => _dbConstraints.GetByConstraintName(constraintName);

        /// <inheritdoc />
        public void Initialize(IDictionary<string, string> properties)
        {
            _dbConstraints.Initialize(properties);
        }

        /// <inheritdoc />
        public ISet<DbConstraintMetadata> Constraints
            => _dbConstraints.Constraints;

        /// <inheritdoc />
        public string ExtractConstraintName(DbException dbException)
        {
            string constraintName;

            SqlException sqle = ADOExceptionHelper.ExtractDbException(dbException) as SqlException;
            if (sqle != null)
            {
                DbConstraintMetadata dbConstraint =
                    _dbConstraints
                        .Constraints
                        .FirstOrDefault(c => sqle.Message.Contains(c.ConstraintName));
                constraintName = dbConstraint?.ConstraintName;
            }
            else
            {
                constraintName = null;
            }

            return constraintName;
        }
    }
}
