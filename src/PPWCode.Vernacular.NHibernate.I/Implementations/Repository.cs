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
using System.Collections.Generic;

using NHibernate;

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Implementations
{
    public abstract class Repository<T, TId>
        : RepositoryBase<T, TId>,
          IRepository<T, TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        protected Repository(ISession session)
            : base(session)
        {
        }

        public virtual T GetById(TId id)
        {
            return Execute("GetById", () => GetByIdInternal(id));
        }

        public virtual IList<T> FindAll()
        {
            return Execute("FindAll", FindAllInternal);
        }

        public virtual T Merge(T entity)
        {
            return Execute("Merge", () => MergeInternal(entity));
        }

        public virtual void SaveOrUpdate(T entity)
        {
            Execute("SaveOrUpdate", () => SaveOrUpdateInternal(entity));
        }

        public virtual void Delete(T entity)
        {
            Execute("Delete", () => DeleteInternal(entity));
        }

        protected virtual T GetByIdInternal(TId id)
        {
            T result = Session.Get<T>(id);

            return result;
        }

        protected abstract IList<T> FindAllInternal();

        protected virtual T MergeInternal(T entity)
        {
            // Note: Prevent a CREATE for something that was assumed to be an UPDATE.
            // NHibernate MERGE transforms an UPDATE for a not-found-PK into a CREATE
            if (entity != null && !entity.IsTransient && GetById(entity.Id) == null)
            {
                throw new NotFoundException("Merge executed for an entity that no longer exists in the database.");
            }

            T result = Session.Merge(entity);

            return result;
        }

        protected virtual void SaveOrUpdateInternal(T entity)
        {
            // Note: Prevent a CREATE for something that was assumed to be an UPDATE.
            if (entity != null && !entity.IsTransient && GetById(entity.Id) == null)
            {
                throw new NotFoundException("SaveOrUpdate executed for an entity that no longer exists in the database.");
            }

            Session.SaveOrUpdate(entity);
        }

        protected virtual void DeleteInternal(T entity)
        {
            if (!entity.IsTransient)
            {
                // Check if entity exists
                T fetchedEntity = Session.Get<T>(entity.Id);
                if (fetchedEntity != null)
                {
                    // Handle stale objects
                    Session.Merge(entity);
                    // finally delete none-transient not stale existing entity
                    Session.Delete(fetchedEntity);
                }
            }
        }
    }
}
