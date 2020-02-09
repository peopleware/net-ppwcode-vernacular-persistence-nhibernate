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

using PPWCode.Vernacular.Persistence.III;

namespace PPWCode.Vernacular.NHibernate.II
{
    public interface IRepository<T, in TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        ///     Gets an entity by its id.
        /// </summary>
        /// <param name="id">The given primary key.</param>
        /// <returns>The entity with the given id or null if not found.</returns>
        [CanBeNull]
        T GetById(TId id);

        /// <summary>
        ///     Find all the records.
        /// </summary>
        /// <returns>
        ///     A list of records, the list is <b>never</b> a null-reference.
        /// </returns>
        [NotNull]
        IList<T> FindAll();

        /// <summary>Gets an list of entities by their ids.</summary>
        /// <param name="ids">The given primary keys.</param>
        /// <returns>
        ///     The entities with the given ids which could be found, if not found no entity with the id is returned.
        /// </returns>
        [NotNull]
        [ItemNotNull]
        IList<T> FindByIds([NotNull] IEnumerable<TId> ids);

        /// <summary>
        ///     A record is saved or updated in the DB to represent <paramref name="entity" />.
        ///     An object is returned that represents the new record, this object is always a <e>new</e> object.
        /// </summary>
        /// <param name="entity">The entity to be saved or updated.</param>
        /// <returns>The persistent entity.</returns>
        [ContractAnnotation("null => null; notnull => notnull")]
        T Merge(T entity);

        /// <summary>
        ///     A record is saved or updated in the DB to represent <paramref name="entity" />.
        ///     An object is returned that represents the new record.
        /// </summary>
        /// <param name="entity">An attached entity to be saved or updated.</param>
        void SaveOrUpdate([CanBeNull] T entity);

        /// <summary>
        ///     The record that represents <paramref name="entity" /> is deleted from the DB.
        /// </summary>
        /// <param name="entity">The given entity.</param>
        void Delete([CanBeNull] T entity);
    }
}
