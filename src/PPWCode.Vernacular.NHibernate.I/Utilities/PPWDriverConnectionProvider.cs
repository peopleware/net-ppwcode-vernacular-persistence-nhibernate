// Copyright 2017 by PeopleWare n.v..
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
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using NHibernate;
using NHibernate.Connection;

namespace PPWCode.Vernacular.NHibernate.I.Utilities
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Castle Windsor usage")]
    [Serializable]
    public class PPWDriverConnectionProvider : DriverConnectionProvider
    {
        private static readonly IInternalLogger s_Log = LoggerProvider.LoggerFor(typeof(PPWDriverConnectionProvider));

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
        public override void CloseConnection(IDbConnection conn)
        {
            if (conn != null)
            {
                base.CloseConnection(conn);
            }
            else if (s_Log.IsWarnEnabled)
            {
                StringBuilder sb = new StringBuilder()
                    .AppendLine("CloseConnection called with <null> conn.")
                    .AppendLine()
                    .AppendLine("Stack Trace :")
                    .AppendLine()
                    .AppendLine(Environment.StackTrace)
                    .AppendLine();
                s_Log.Warn(sb.ToString());
            }
        }
    }
}
