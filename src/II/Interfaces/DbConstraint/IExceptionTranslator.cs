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

namespace PPWCode.Vernacular.NHibernate.II.DbConstraint
{
    public interface IExceptionTranslator
    {
        /// <summary>
        ///     This method *has* to convert whatever NHibernate exception to a valid PPWCode exception
        ///     Some hibernate exceptions might be semantic, some might be errors.
        ///     This may depend on the actual product.
        ///     This method translates semantic exceptions in PPWCode.Util.Exception.SemanticException and throws them
        ///     and all other exceptions in PPWCode.Util.Exception.Error and throws them.
        /// </summary>
        /// <param name="message">
        ///     This message will be used in the logging in the case <paramref name="exception" /> is of type
        ///     <see cref="Exceptions.IV.Error" />.
        /// </param>
        /// <param name="exception">The hibernate exception we are triaging.</param>
        /// <returns>
        ///     An exception that is a sub class either from <see cref="Exceptions.IV.SemanticException" />
        ///     or from <see cref="Exceptions.IV.Error" />.
        /// </returns>
        [NotNull]
        Exception Convert([NotNull] string message, [NotNull] Exception exception);
    }
}
