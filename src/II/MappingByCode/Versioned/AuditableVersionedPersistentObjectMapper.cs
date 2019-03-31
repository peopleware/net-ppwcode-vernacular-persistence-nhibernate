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

using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.II.MappingByCode
{
    public abstract class AuditableVersionedPersistentObjectMapper<T, TId, TVersion>
        : VersionedPersistentObjectMapper<T, TId, TVersion>
        where T : class, IVersionedPersistentObject<TId, TVersion>, IAuditable
        where TId : IEquatable<TId>
        where TVersion : IEquatable<TVersion>
    {
        protected AuditableVersionedPersistentObjectMapper()
        {
            // Satisfy IInsertAuditable
            Property(x => x.CreatedAt, m => m.Update(false));
            Property(
                x => x.CreatedBy,
                m =>
                {
                    m.Length(MaxUserNameLength);
                    m.Update(false);
                });

            // Satisfy IUpdateAuditable
            Property(x => x.LastModifiedAt, m => m.Insert(false));
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
