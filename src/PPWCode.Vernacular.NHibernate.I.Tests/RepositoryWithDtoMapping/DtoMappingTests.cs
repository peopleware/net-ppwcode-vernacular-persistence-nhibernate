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

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.NHibernate.I.Tests.IntegrationTests;
using PPWCode.Vernacular.NHibernate.I.Tests.RepositoryWithDtoMapping.Models;
using PPWCode.Vernacular.NHibernate.I.Tests.RepositoryWithDtoMapping.Repositories;

namespace PPWCode.Vernacular.NHibernate.I.Tests.RepositoryWithDtoMapping
{
    public class DtoMappingTests : BaseRepositoryTests<Ship>
    {
        protected override Func<IRepository<Ship, int>> RepositoryFactory
        {
            get { return () => new ShipRepository(Session); }
        }

        public ShipRepository ShipRepository
        {
            get { return new ShipRepository(Session); }
        }

        /// <summary>
        ///     Override this method for setup code that needs to run for each test separately.
        /// </summary>
        protected override void OnSetup()
        {
            base.OnSetup();
            SessionFactory.Statistics.Clear();
        }

        private void GenerateShipAndContainers()
        {
            Ship ship1 = new Ship
                         {
                             Code = "X1"
                         };

            // first batch of containers "C"
            int i = 0;
            while (i++ < 3)
            {
                CargoContainer cargoContainer =
                    new CargoContainer
                    {
                        Code = string.Format("C{0:D}", i),
                        Load = 25 * i,
                        Ship = ship1
                    };
            }

            // second batch of containers "S"
            i = 0;
            while (i++ < 3)
            {
                CargoContainer cargoContainer =
                    new CargoContainer
                    {
                        Code = string.Format("S{0:D}", i),
                        Load = 10 * i,
                        Ship = ship1
                    };
            }

            Ship ship2 = new Ship
                         {
                             Code = "X2"
                         };

            // third batch of containers "C"
            i = 5;
            while (i++ < 10)
            {
                CargoContainer cargoContainer =
                    new CargoContainer
                    {
                        Code = string.Format("C{0:D}", i),
                        Load = 15 * i,
                        Ship = ship2
                    };
            }

            Ship ship3 = new Ship
                         {
                             Code = "Z1"
                         };

            // fourth batch of containers "S"
            i = 10;
            while (i++ < 13)
            {
                CargoContainer cargoContainer =
                    new CargoContainer
                    {
                        Code = string.Format("S{0:D}", i),
                        Load = 100 * i,
                        Ship = ship3
                    };
            }

            // persist
            RunInsideTransaction(
                () =>
                {
                    ShipRepository.Merge(ship1);
                    ShipRepository.Merge(ship2);
                    ShipRepository.Merge(ship3);
                },
                true);
        }

        [Test]
        public void TestAddingShipsAndContainers()
        {
            GenerateShipAndContainers();
        }

        [Test]
        public void TestDtoMappingShipsZ()
        {
            GenerateShipAndContainers();

            IList<ContainerDto> dtos = null;

            RunInsideTransaction(
                () =>
                {
                    dtos = ShipRepository.FindContainersFromShipsMatchingCode("Z");
                },
                true);

            Assert.IsTrue(dtos.Select(d => d.ShipCode).All(c => c.StartsWith("Z")));
            Assert.AreEqual(3, dtos.Count);
            Assert.IsTrue(dtos.Select(d => d.Load).All(l => l == 1100 || l == 1200 || l == 1300));
            Assert.IsTrue(dtos.Select(d => d.ContainerCode).All(c => c == "S11" || c == "S12" || c == "S13"));
        }

        [Test]
        public void TestDtoMappingShipsX()
        {
            GenerateShipAndContainers();

            IList<ContainerDto> dtos = null;

            RunInsideTransaction(
                () =>
                {
                    dtos = ShipRepository.FindContainersFromShipsMatchingCode("X");
                },
                true);

            Assert.IsTrue(dtos.Select(d => d.ShipCode).All(c => c.StartsWith("X")));
        }
    }
}
