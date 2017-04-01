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

using NHibernate;
using NHibernate.Transform;

using PPWCode.Vernacular.NHibernate.I.Tests.Repositories;
using PPWCode.Vernacular.NHibernate.I.Tests.RepositoryWithDtoMapping.Models;

namespace PPWCode.Vernacular.NHibernate.I.Tests.RepositoryWithDtoMapping.Repositories
{
    public class ShipRepository : TestRepository<Ship>, ITestRepository<Ship>
    {
        public ShipRepository(ISession session)
            : base(session)
        {
        }

        public IList<ContainerDto> FindContainersFromShipsMatchingCode(string code)
        {
            Ship ship = null;
            CargoContainer cargoContainer = null;
            ContainerDto dto = null;

            return Find<ContainerDto>(
                () => ship,
                qry => qry
                    .JoinAlias(() => ship.CargoContainers, () => cargoContainer)
                    .WhereRestrictionOn(() => ship.Code).IsLike(code + "%")
                    .SelectList(list => list
                        .SelectGroup(() => ship.Code).WithAlias(() => dto.ShipCode)
                        .SelectGroup(() => cargoContainer.Code).WithAlias(() => dto.ContainerCode)
                        .SelectGroup(() => cargoContainer.Load).WithAlias(() => dto.Load))
                    .TransformUsing(Transformers.AliasToBean<ContainerDto>()));
        }
    }
}