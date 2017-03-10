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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Models
{
    public class Role : AuditableVersionedPersistentObject<int, int>
    {
        private readonly ISet<User> m_Users = new HashSet<User>();
        private string m_Name;

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

        [Required, StringLength(200)]
        public virtual string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public virtual ISet<User> Users
        {
            get { return m_Users; }
        }

        public virtual void AddUser(User user)
        {
            if (user != null && m_Users.Add(user))
            {
                user.AddRole(this);
            }
        }

        public virtual void RemoveUser(User user)
        {
            if (user != null && m_Users.Remove(user))
            {
                user.RemoveRole(this);
            }
        }
    }
}
