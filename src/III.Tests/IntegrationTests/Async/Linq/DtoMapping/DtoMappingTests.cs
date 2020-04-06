// Copyright 2020 by PeopleWare n.v..
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests.Async.Linq.DtoMapping.Repositories;
using PPWCode.Vernacular.NHibernate.III.Tests.Model.RepositoryWithDtoMapping;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests.Async.Linq.DtoMapping
{
    public class DtoMappingTests : BaseRepositoryTests<Ship>
    {
        public ShipRepository ShipRepository
            => new ShipRepository(SessionProviderAsync);

        private Task GenerateShipAndContainersAsync(CancellationToken cancellationToken)
        {
            Ship ship1 =
                new Ship
                {
                    Code = "X1"
                };

            // first batch of containers "C"
            int i = 0;
            while (i++ < 3)
            {
                // ReSharper disable once ObjectCreationAsStatement
                new CargoContainer
                {
                    Code = $"C{i:D}",
                    Load = 25 * i,
                    Ship = ship1
                };
            }

            // second batch of containers "S"
            i = 0;
            while (i++ < 3)
            {
                // ReSharper disable once ObjectCreationAsStatement
                new CargoContainer
                {
                    Code = $"S{i:D}",
                    Load = 10 * i,
                    Ship = ship1
                };
            }

            Ship ship2 =
                new Ship
                {
                    Code = "X2"
                };

            // third batch of containers "C"
            i = 5;
            while (i++ < 10)
            {
                // ReSharper disable once ObjectCreationAsStatement
                new CargoContainer
                {
                    Code = $"C{i:D}",
                    Load = 15 * i,
                    Ship = ship2
                };
            }

            Ship ship3 =
                new Ship
                {
                    Code = "Z1"
                };

            // fourth batch of containers "S"
            i = 10;
            while (i++ < 13)
            {
                // ReSharper disable once ObjectCreationAsStatement
                new CargoContainer
                {
                    Code = $"S{i:D}",
                    Load = 100 * i,
                    Ship = ship3
                };
            }

            async Task CreateShips(CancellationToken can)
            {
                await ShipRepository.MergeAsync(ship1, can).ConfigureAwait(false);
                await ShipRepository.MergeAsync(ship2, can).ConfigureAwait(false);
                await ShipRepository.MergeAsync(ship3, can).ConfigureAwait(false);
            }

            // persist
            return RunInsideTransactionAsync(CreateShips, true, cancellationToken);
        }

        [Test]
        public async Task TestAddingShipsAndContainers()
            => await GenerateShipAndContainersAsync(CancellationToken).ConfigureAwait(false);

        [Test]
        public async Task TestDtoMappingShipsX()
        {
            await GenerateShipAndContainersAsync(CancellationToken).ConfigureAwait(false);
            IList<ContainerDto> dtos = null;

            async Task Action(CancellationToken cancellationToken)
            {
                dtos = await ShipRepository.FindContainersFromShipsMatchingCodeAsync("X", cancellationToken).ConfigureAwait(false);
            }

            await RunInsideTransactionAsync(Action, true, CancellationToken).ConfigureAwait(false);

            Assert.IsTrue(dtos.Select(d => d.ShipCode).All(c => c.StartsWith("X")));
        }

        [Test]
        public async Task TestDtoMappingShipsXPaged()
        {
            await GenerateShipAndContainersAsync(CancellationToken).ConfigureAwait(false);
            IPagedList<ContainerDto> dtos = null;

            async Task Action(CancellationToken cancellationToken)
            {
                dtos = await ShipRepository.FindContainersFromShipsMatchingCodePagedAsync(2, 10, "X", cancellationToken).ConfigureAwait(false);
            }

            await RunInsideTransactionAsync(Action, true, CancellationToken).ConfigureAwait(false);

            Assert.IsTrue(dtos.Items.Select(d => d.ShipCode).All(c => c.StartsWith("X")));
        }

        [Test]
        public async Task TestDtoMappingShipsZ()
        {
            await GenerateShipAndContainersAsync(CancellationToken).ConfigureAwait(false);
            IList<ContainerDto> dtos = null;

            async Task Action(CancellationToken cancellationToken)
            {
                dtos = await ShipRepository.FindContainersFromShipsMatchingCodeAsync("Z", cancellationToken).ConfigureAwait(false);
            }

            await RunInsideTransactionAsync(Action, true, CancellationToken).ConfigureAwait(false);

            Assert.IsTrue(dtos.Select(d => d.ShipCode).All(c => c.StartsWith("Z")));
            Assert.AreEqual(3, dtos.Count);
            Assert.IsTrue(dtos.Select(d => d.Load).All(l => (l == 1100) || (l == 1200) || (l == 1300)));
            Assert.IsTrue(dtos.Select(d => d.ContainerCode).All(c => (c == "S11") || (c == "S12") || (c == "S13")));
        }
    }
}
