// Copyright 2017 by PeopleWare n.v..
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
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using Common.Logging;

using JetBrains.Annotations;

using NHibernate.Connection;

namespace PPWCode.Vernacular.NHibernate.II
{
    /// <inheritdoc />
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Castle Windsor usage")]
    [Serializable]
    public class PPWDriverConnectionProvider : DriverConnectionProvider
    {
        [NotNull]
        private static readonly ILog _logger = LogManager.GetLogger<PPWDriverConnectionProvider>();

        /// <summary>
        ///     Closes and Disposes of the <see cref="T:System.Data.IDbConnection" />.
        ///     In some cases we have seen that the <paramref name="conn" /> was not known (aka null), the result was
        ///     that the exception <see cref="NullReferenceException" /> was thrown in the abstract class
        ///     <see cref="ConnectionProvider" />.
        ///     Usage :
        ///     <property name="connection.provider">
        ///         PPWCode.Vernacular.NHibernate.I.Utilities.PPWDriverConnectionProvider,
        ///         PPWCode.Vernacular.NHibernate.I
        ///     </property>
        /// </summary>
        /// <param name="conn">The <see cref="T:System.Data.IDbConnection" /> to clean up.</param>
        public override void CloseConnection([CanBeNull] DbConnection conn)
        {
            if (conn != null)
            {
                base.CloseConnection(conn);
            }
            else if (_logger.IsWarnEnabled)
            {
                StringBuilder sb =
                    new StringBuilder()
                        .AppendLine("CloseConnection called with <null> conn.")
                        .AppendLine()
                        .AppendLine("Stack Trace :")
                        .AppendLine()
                        .AppendLine(Environment.StackTrace)
                        .AppendLine();
                _logger.Warn(sb.ToString());
            }
        }
    }
}
