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

using NHibernate;
using NHibernate.Exceptions;

using PPWCode.Vernacular.Exceptions.II;
using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Implementations.DbConstraint
{
    /// <inheritdoc cref="IExceptionTranslator" />
    public class ExceptionTranslator : IExceptionTranslator
    {
        /// <inheritdoc />
        public Exception Convert(string message, Exception exception)
        {
            StaleObjectStateException staleObjectStateException = exception as StaleObjectStateException;
            if (staleObjectStateException != null)
            {
                return
                    new ObjectAlreadyChangedException(
                        staleObjectStateException.Message,
                        staleObjectStateException.EntityName,
                        staleObjectStateException.Identifier);
            }

            Exception result;

            HibernateException hibernateException = exception as HibernateException;
            if (hibernateException != null)
            {
                GenericADOException genericAdoException = exception as GenericADOException;
                if (genericAdoException != null)
                {
                    result = new RepositorySqlException(message, genericAdoException.SqlString, genericAdoException.InnerException);
                }
                else
                {
                    result = new ExternalError(message, exception);
                }
            }
            else
            {
                result = new ExternalError(message, exception);
            }

            return result;
        }
    }
}
