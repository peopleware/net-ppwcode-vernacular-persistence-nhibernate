// Copyright 2018 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;

using NHibernate;
using NHibernate.Transform;

using PPWCode.Vernacular.NHibernate.II.Providers;
using PPWCode.Vernacular.NHibernate.II.Tests.IntegrationTests.Sync.QueryOver.Common.Repositories;
using PPWCode.Vernacular.NHibernate.II.Tests.Model.RepositoryWithDtoMapping;
using PPWCode.Vernacular.Persistence.III;

namespace PPWCode.Vernacular.NHibernate.II.Tests.IntegrationTests.Sync.QueryOver.DtoMapping.Repositories
{
    public class ShipRepository : TestRepository<Ship>
    {
        public ShipRepository(ISessionProvider sessionProvider)
            : base(sessionProvider)
        {
        }

        public IList<ContainerDto> FindContainersFromShipsMatchingCode(string code)
            => Execute(
                nameof(FindContainersFromShipsMatchingCode),
                () => FindContainersFromShipsMatchingCodeQuery(code).List<ContainerDto>());

        public PagedList<ContainerDto> FindContainersFromShipsMatchingCodePaged(int pageIndex, int pageSize, string code)
            => Execute(
                nameof(FindContainersFromShipsMatchingCode),
                () => FindPagedInternal<ContainerDto>(pageIndex, pageSize, () => FindContainersFromShipsMatchingCodeQuery(code)));

        protected virtual IQueryOver<Ship, Ship> FindContainersFromShipsMatchingCodeQuery(string code)
        {
            CargoContainer cargoContainer = null;
            ContainerDto dto = null;

            return
                CreateQueryOver()
                    .JoinAlias(ship => ship.CargoContainers, () => cargoContainer)
                    .WhereRestrictionOn(ship => ship.Code).IsLike(code + "%")
                    .SelectList(
                        list =>
                            list
                                .SelectGroup(ship => ship.Code).WithAlias(() => dto.ShipCode)
                                .SelectGroup(ship => cargoContainer.Code).WithAlias(() => dto.ContainerCode)
                                .SelectGroup(ship => cargoContainer.Load).WithAlias(() => dto.Load))
                    .TransformUsing(Transformers.AliasToBean<ContainerDto>());
        }
    }
}
