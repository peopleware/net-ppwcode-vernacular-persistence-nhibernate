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
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using JetBrains.Annotations;

using NHibernate.Mapping.ByCode;
using NHibernate.Type;

using PPWCode.Vernacular.NHibernate.II.MappingByCode;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.II.Tests.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class User : AuditableVersionedPersistentObject<int, int>
    {
        [DataMember]
        private readonly ISet<Role> _roles = new HashSet<Role>();

        public User(int id, int persistenceVersion)
            : base(id, persistenceVersion)
        {
        }

        public User(int id)
            : base(id)
        {
        }

        public User()
        {
        }

        [DataMember]
        [Required]
        [StringLength(200)]
        public virtual string Name { get; set; }

        [DataMember]
        [Required]
        public virtual Gender? Gender { get; set; }

        [DataMember]
        public virtual bool HasBlueEyes { get; set; }

        [AuditLogPropertyIgnore]
        public virtual ISet<Role> Roles
            => _roles;

        public virtual void AddRole(Role role)
        {
            if ((role != null) && Roles.Add(role))
            {
                role.AddUser(this);
            }
        }

        public virtual void RemoveRole(Role role)
        {
            if ((role != null) && Roles.Remove(role))
            {
                role.RemoveUser(this);
            }
        }
    }

    [UsedImplicitly]
    public class UserMapper : AuditableVersionedPersistentObjectMapper<User, int, int>
    {
        public UserMapper()
        {
            // User is most of the time a reserved word
            Table("`User`");
            Property(
                u => u.Name,
                m => m.Unique(true));
            Property(u => u.Gender, m => m.Type<EnumStringType<Gender>>());
            Property(u => u.HasBlueEyes, m => m.Type<YesNoType>());
            Set(
                u => u.Roles,
                m => m.Cascade(Cascade.None),
                c => c.ManyToMany());
        }
    }
}
