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

using NHibernate.Mapping.ByCode;

using PPWCode.Vernacular.NHibernate.I.MappingByCode;
using PPWCode.Vernacular.NHibernate.I.Tests.RepositoryWithDtoMapping.Models;

namespace PPWCode.Vernacular.NHibernate.I.Tests.RepositoryWithDtoMapping.Mappers
{
    public class ShipMapper : PersistentObjectMapper<Ship, int>
    {
        public ShipMapper()
        {
            Property(s => s.Code);

            Set(
                s => s.CargoContainers,
                m =>
                {
                    m.Inverse(true);
                    m.Cascade(Cascade.All | Cascade.Merge);
                },
                r => r.OneToMany());
        }
    }
}
