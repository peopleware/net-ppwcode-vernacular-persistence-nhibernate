// Copyright 2014 by PeopleWare n.v..
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

using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.MappingByCode
{
    public abstract class AuditLogMapping<TId> : PersistentObjectMapper<AuditLog<TId>, TId>
        where TId : IEquatable<TId>
    {
        protected AuditLogMapping()
        {
            Property(x => x.EntryType);
            Property(x => x.EntityName);
            Property(x => x.EntityId);
            Property(x => x.PropertyName);
            Property(x => x.OldValue);
            Property(x => x.NewValue);
            Property(x => x.CreatedAt);
            Property(x => x.CreatedBy);
        }
    }
}