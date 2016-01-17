// Copyright 2016 by PeopleWare n.v..
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

using NHibernate.Mapping.ByCode;
using NHibernate.Type;

using PPWCode.Vernacular.NHibernate.I.MappingByCode;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Models.Mapping
{
    public class UserMapper : AuditableVersionedPeristentObjectMapper<User, int, int>
    {
        public UserMapper()
        {
            Property(u => u.Name);
            Property(u => u.Gender, m => m.Type<EnumStringType<Gender>>());
            Set(
                u => u.Roles,
                m => m.Cascade(Cascade.None),
                c => c.ManyToMany());
        }
    }
}