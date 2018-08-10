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

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.II.Interfaces;
using PPWCode.Vernacular.NHibernate.II.Tests.DictionariesAndLists.Models;
using PPWCode.Vernacular.NHibernate.II.Tests.DictionariesAndLists.Repositories;
using PPWCode.Vernacular.NHibernate.II.Tests.IntegrationTests.QueryOver;

namespace PPWCode.Vernacular.NHibernate.II.Tests.DictionariesAndLists
{
    public class DictionariesAndListsTest : BaseRepositoryTests<Tower>
    {
        protected override Func<IQueryOverRepository<Tower, int>> RepositoryFactory
        {
            get { return () => new TowerRepository(SessionProvider); }
        }

        /// <summary>
        ///     Override this method for setup code that needs to run for each test separately.
        /// </summary>
        protected override void OnSetup()
        {
            base.OnSetup();
            SessionFactory.Statistics.Clear();
        }

        [Test]
        public void TestBiDirectionalOneToManyWithMerge()
        {
            RunInsideTransaction(
                () =>
                {
                    Tower tower = new Tower();

                    tower.Sections.Add(
                        new Plane
                        {
                            Normal =
                                new Vector3DBuilder()
                                    .X(0.123)
                                    .Y(0.456)
                                    .Z(0.789),
                            Translation = 0.197
                        });
                    tower.Sides[SideEnum.NORTH] =
                        new ClippingPlane
                        {
                            Plane =
                                new Plane
                                {
                                    Normal =
                                        new Vector3DBuilder()
                                            .X(0.123)
                                            .Y(0.456)
                                            .Z(0.789),
                                    Translation = 0.197
                                },
                            MeshTranslation =
                                new Vector3DBuilder()
                                    .X(0.321)
                                    .Y(0.654)
                                    .Z(0.987)
                        };
                },
                true);
        }
    }
}
