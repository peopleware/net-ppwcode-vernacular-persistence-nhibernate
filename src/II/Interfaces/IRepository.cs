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
using System.Collections.Generic;

using JetBrains.Annotations;

using NHibernate;

using PPWCode.Vernacular.Persistence.III;

namespace PPWCode.Vernacular.NHibernate.II
{
    public interface IRepository<T, in TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        /// <inheritdoc cref="ISession.Get{T}(object)" />
        /// <remarks>
        ///     Runs in an isolated environment. This ensures a transaction is active and exceptions are being triaged.
        /// </remarks>
        [CanBeNull]
        T GetById(TId id);

        /// <inheritdoc cref="ISession.Load{T}(object)" />
        /// <remarks>
        ///     Runs in an isolated environment. This ensures a transaction is active and exceptions are being triaged.
        /// </remarks>
        [NotNull]
        T LoadById(TId id);

        /// <summary>
        ///     Find all records of type <typeparamref name="T" />.
        /// </summary>
        /// <returns>
        ///     A list of records, the list is <b>never</b> a null-reference.
        /// </returns>
        /// <remarks>
        ///     Runs in an isolated environment. This ensures a transaction is active and exceptions are being triaged.
        /// </remarks>
        [NotNull]
        IList<T> FindAll();

        /// <summary>Gets an list of entities by their ids.</summary>
        /// <param name="ids">The given primary keys.</param>
        /// <returns>
        ///     The entities with the given ids which could be found, if not found no entity with the id is returned.
        /// </returns>
        /// <remarks>
        ///     Runs in an isolated environment. This ensures a transaction is active and exceptions are being triaged.
        /// </remarks>
        [NotNull]
        [ItemNotNull]
        IList<T> FindByIds([NotNull] IEnumerable<TId> ids);

        /// <inheritdoc cref="ISession.Merge{T}(T)" />
        /// <exception cref="NotFoundException">
        ///     The normal behaviour of nHibernate is that the <c>Merge</c> transforms an UPDATE for a not-found-PK into a
        ///     CREATE. We will not do this in our code-base. In this case we will throw an exception.
        /// </exception>
        /// <remarks>
        ///     Runs in an isolated environment. This ensures a transaction is active and exceptions are being triaged.
        /// </remarks>
        [ContractAnnotation("null => null; notnull => notnull")]
        T Merge(T entity);

        /// <inheritdoc cref="ISession.SaveOrUpdate(object)" />
        /// <exception cref="NotFoundException">
        ///     The normal behaviour of nHibernate is that the <c>SaveOrUpdate</c> transforms an UPDATE for a not-found-PK into a
        ///     CREATE. We will not do this in our code-base. In this case we will throw an exception.
        /// </exception>
        /// <remarks>
        ///     Runs in an isolated environment. This ensures a transaction is active and exceptions are being triaged.
        /// </remarks>
        void SaveOrUpdate([CanBeNull] T entity);

        /// <inheritdoc cref="ISession.Delete(object)" />
        /// <remarks>
        ///     Runs in an isolated environment. This ensures a transaction is active and exceptions are being triaged.
        /// </remarks>
        void Delete([CanBeNull] T entity);
    }
}
