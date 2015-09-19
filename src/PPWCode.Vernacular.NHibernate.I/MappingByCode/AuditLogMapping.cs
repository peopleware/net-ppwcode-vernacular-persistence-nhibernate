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
using NHibernate.Type;

using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.MappingByCode
{
    public abstract class AuditLogMapping<T, TId> : PersistentObjectMapper<T, TId>
        where T : AuditLog<TId>
        where TId : IEquatable<TId>
    {
        protected AuditLogMapping()
        {
            Property(x => x.EntryType, m => m.NotNullable(true));
            Property(x => x.EntityName, m => m.NotNullable(true));
            Property(x => x.EntityId, m => m.NotNullable(true));
            Property(x => x.PropertyName, m => m.NotNullable(true));
            Property(x => x.OldValue, m => m.Type(NHibernateUtil.StringClob));
            Property(x => x.NewValue, m => m.Type(NHibernateUtil.StringClob));
            Property(
                x => x.CreatedAt,
                m =>
                {
                    m.Type(NHibernateUtil.UtcDateTime);
                    m.NotNullable(true);
                });
            Property(x => x.CreatedBy, m => m.NotNullable(true));
        }
    }
}