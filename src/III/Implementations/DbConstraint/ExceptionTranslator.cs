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

using NHibernate;
using NHibernate.Exceptions;

using PPWCode.Vernacular.Exceptions.IV;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.DbConstraint
{
    /// <inheritdoc cref="IExceptionTranslator" />
    public class ExceptionTranslator : IExceptionTranslator
    {
        /// <inheritdoc />
        public Exception Convert(string message, Exception exception)
        {
            if (exception is SemanticException)
            {
                return exception;
            }

            if (exception is ProgrammingError)
            {
                return exception;
            }

            if (exception is StaleObjectStateException staleObjectStateException)
            {
                return
                    new ObjectAlreadyChangedException(
                        staleObjectStateException.Message,
                        staleObjectStateException.EntityName,
                        staleObjectStateException.Identifier);
            }

            if (exception is GenericADOException genericAdoException)
            {
                return new RepositorySqlException(message, genericAdoException.SqlString, genericAdoException.InnerException);
            }

            return new ExternalError(message, exception);
        }
    }
}
