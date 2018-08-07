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

using PPWCode.Vernacular.NHibernate.II.Interfaces;
using PPWCode.Vernacular.Persistence.III;

namespace PPWCode.Vernacular.NHibernate.II.Implementations
{
    public abstract class Repository<TRoot, TId>
        : RepositoryBase<TRoot, TId>,
          IRepository<TRoot, TId>
        where TRoot : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        protected Repository([NotNull] ISessionProvider sessionProvider)
            : base(sessionProvider)
        {
        }

        public virtual TRoot GetById(TId id)
            => Execute(nameof(GetById), () => GetByIdInternal(id));

        public virtual IList<TRoot> FindAll()
            => Execute(nameof(FindAll), FindAllInternal) ?? new List<TRoot>();

        public virtual TRoot Merge(TRoot entity)
            => Execute(nameof(Merge), () => MergeInternal(entity));

        public virtual void SaveOrUpdate(TRoot entity)
            => Execute(nameof(SaveOrUpdate), () => SaveOrUpdateInternal(entity));

        public virtual void Delete(TRoot entity)
            => Execute(nameof(Delete), () => DeleteInternal(entity));

        protected virtual TRoot GetByIdInternal(TId id)
            => Session.Get<TRoot>(id);

        [NotNull]
        protected abstract IList<TRoot> FindAllInternal();

        protected virtual TRoot MergeInternal(TRoot entity)
        {
            if (entity != null)
            {
                // Note: Prevent a CREATE for something that was assumed to be an UPDATE.
                // NHibernate MERGE transforms an UPDATE for a not-found-PK into a CREATE
                if (!entity.IsTransient && (GetById(entity.Id) == null))
                {
                    throw new NotFoundException("Merge executed for an entity that no longer exists in the database.");
                }

                return Session.Merge(entity);
            }

            return default(TRoot);
        }

        protected virtual void SaveOrUpdateInternal(TRoot entity)
        {
            if (entity != null)
            {
                // Note: Prevent a CREATE for something that was assumed to be an UPDATE.
                if (!entity.IsTransient && (GetById(entity.Id) == null))
                {
                    throw new NotFoundException("SaveOrUpdate executed for an entity that no longer exists in the database.");
                }

                Session.SaveOrUpdate(entity);
            }
        }

        protected virtual void DeleteInternal(TRoot entity)
        {
            if ((entity != null) && !entity.IsTransient)
            {
                // Check if entity exists
                TRoot fetchedEntity = Session.Get<TRoot>(entity.Id);
                if (fetchedEntity != null)
                {
                    // Handle stale objects
                    TRoot mergedEntity = Session.Merge(entity);

                    // finally delete none-transient not stale existing entity
                    Session.Delete(mergedEntity);
                }
            }
        }
    }
}
