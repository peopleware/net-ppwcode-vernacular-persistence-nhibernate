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
using System.Collections.Generic;
using System.Data.Common;

using NHibernate.Exceptions;

using PPWCode.Vernacular.NHibernate.I.Implementations.DbConstraint;

namespace PPWCode.Vernacular.NHibernate.I.Implementations.DbExceptionConverters
{
    public abstract class BaseExceptionConverter
        : ISQLExceptionConverter,
          IConfigurable
    {
        private IDbConstraints _dbConstraints;

        protected BaseExceptionConverter(IViolatedConstraintNameExtracter violatedConstraintNameExtracter)
        {
            if (violatedConstraintNameExtracter == null)
            {
                throw new ArgumentNullException(nameof(violatedConstraintNameExtracter));
            }

            ViolatedConstraintNameExtracter = violatedConstraintNameExtracter;
        }

        public IViolatedConstraintNameExtracter ViolatedConstraintNameExtracter { get; }

        protected IDictionary<string, string> Configuration { get; private set; }

        protected IDbConstraints DbConstraints
            => _dbConstraints ?? (_dbConstraints = ViolatedConstraintNameExtracter as IDbConstraints);

        void IConfigurable.Configure(IDictionary<string, string> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            Configuration = new Dictionary<string, string>(properties);
            DbConstraints?.Initialize(Configuration);
        }

        Exception ISQLExceptionConverter.Convert(AdoExceptionContextInfo adoExceptionContextInfo)
            => OnConvert(adoExceptionContextInfo);

        protected abstract Exception OnConvert(AdoExceptionContextInfo adoExceptionContextInfo);

        protected virtual string GetConstraintName(AdoExceptionContextInfo adoExceptionContextInfo)
        {
            DbException sqle = adoExceptionContextInfo.SqlException as DbException;
            return
                sqle != null
                    ? ViolatedConstraintNameExtracter.ExtractConstraintName(sqle)
                    : null;
        }
    }
}
