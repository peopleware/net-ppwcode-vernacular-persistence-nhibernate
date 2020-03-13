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

using NHibernate.Mapping.ByCode;

using PPWCode.Vernacular.NHibernate.III.MappingByCode;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Tests.Model.Common
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Role : AuditableVersionedPersistentObject<int, int>
    {
        [DataMember]
        private readonly ISet<User> _users = new HashSet<User>();

        public Role(int id, int persistenceVersion)
            : base(id, persistenceVersion)
        {
        }

        public Role(int id)
            : base(id)
        {
        }

        public Role()
        {
        }

        [DataMember]
        [Required]
        [StringLength(200)]
        public virtual string Name { get; set; }

        [AuditLogPropertyIgnore]
        public virtual ISet<User> Users
            => _users;

        public virtual void AddUser(User user)
        {
            if ((user != null) && Users.Add(user))
            {
                user.AddRole(this);
            }
        }

        public virtual void RemoveUser(User user)
        {
            if ((user != null) && Users.Remove(user))
            {
                user.RemoveRole(this);
            }
        }
    }

    public class RoleMapper : AuditableVersionedPersistentObjectMapper<Role, int, int>
    {
        public RoleMapper()
        {
            Property(r => r.Name);
            Set(
                r => r.Users,
                m =>
                {
                    m.Cascade(Cascade.None);
                    m.Inverse(true);
                },
                c => c.ManyToMany());
        }
    }
}
