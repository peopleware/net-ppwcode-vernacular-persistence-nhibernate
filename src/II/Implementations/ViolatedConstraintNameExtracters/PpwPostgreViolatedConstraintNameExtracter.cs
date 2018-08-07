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
using System.Data.Common;

using JetBrains.Annotations;

using NHibernate.Exceptions;

using Npgsql;

using PPWCode.Vernacular.NHibernate.II.Implementations.DbConstraint;
using PPWCode.Vernacular.NHibernate.II.Interfaces;

namespace PPWCode.Vernacular.NHibernate.II.Implementations.ViolatedConstraintNameExtracters
{
    public class PpwPostgreViolatedConstraintNameExtracter
        : IViolatedConstraintNameExtracter,
          IDbConstraints
    {
        [NotNull]
        private readonly IDbConstraints _dbConstraints;

        public PpwPostgreViolatedConstraintNameExtracter()
        {
            _dbConstraints = new PpwPostgreDbConstraints();
        }

        /// <inheritdoc />
        public DbConstraintMetadata GetByConstraintName(string constraintName)
            => _dbConstraints.GetByConstraintName(constraintName);

        /// <inheritdoc />
        public void Initialize(IDictionary<string, string> connectionStringSettings)
        {
            _dbConstraints.Initialize(connectionStringSettings);
        }

        /// <inheritdoc />
        public ISet<DbConstraintMetadata> Constraints
            => _dbConstraints.Constraints;

        /// <inheritdoc />
        [CanBeNull]
        public string ExtractConstraintName([NotNull] DbException dbException)
        {
            PostgresException sqle = ADOExceptionHelper.ExtractDbException(dbException) as PostgresException;
            return sqle?.ConstraintName;
        }
    }
}
