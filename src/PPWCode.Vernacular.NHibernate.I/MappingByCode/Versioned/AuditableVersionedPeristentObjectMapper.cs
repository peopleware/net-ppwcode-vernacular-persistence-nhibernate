// Copyright 2015 by PeopleWare n.v..
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

using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.MappingByCode
{
    public abstract class AuditableVersionedPeristentObjectMapper<T, TId, TVersion>
        : VersionedPersistentObjectMapper<T, TId, TVersion>
        where T : class, IVersionedPersistentObject<TId, TVersion>, IAuditable
        where TId : IEquatable<TId>
    {
        protected AuditableVersionedPeristentObjectMapper()
        {
            // Satisfy IInsertAuditable
            Property(
                x => x.CreatedAt,
                m =>
                {
                    m.Type(NHibernateUtil.UtcDateTime);
                    m.Update(false);
                    m.NotNullable(true);
                });
            Property(
                x => x.CreatedBy,
                m =>
                {
                    m.Length(MaxUserNameLength);
                    m.Update(false);
                    m.NotNullable(true);
                });

            // Satisfy IUpdateAuditable
            Property(
                x => x.LastModifiedAt,
                m =>
                {
                    m.Type(NHibernateUtil.UtcDateTime);
                    m.Insert(false);
                });
            Property(
                x => x.LastModifiedBy,
                m =>
                {
                    m.Length(MaxUserNameLength);
                    m.Insert(false);
                });
        }
    }
}