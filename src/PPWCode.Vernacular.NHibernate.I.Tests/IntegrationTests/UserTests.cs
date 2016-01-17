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

using System.Linq;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.I.Tests.Models;
using PPWCode.Vernacular.NHibernate.I.Tests.Repositories;

namespace PPWCode.Vernacular.NHibernate.I.Tests.IntegrationTests
{
    public class UserTests : BaseUserTests
    {
        private RoleRepository m_RoleRepository;

        protected RoleRepository RoleRepository
        {
            get { return m_RoleRepository; }
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            m_RoleRepository = new RoleRepository(Session);
        }

        protected override void OnTeardown()
        {
            m_RoleRepository = null;

            base.OnTeardown();
        }

        [Test]
        public void CreateUserWithoutRoles()
        {
            User user =
                new User
                {
                    Name = @"Ruben"
                };
            RunInsideTransaction(() => Repository.Merge(user), true);

            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(1));
        }

        [Test]
        public void CreateUserWithOneRole()
        {
            Role role =
                new Role
                {
                    Name = @"Architect"
                };

            Role savedRole = RunInsideTransaction(() => RoleRepository.Merge(role), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(1));

            User user =
                new User
                {
                    Name = @"Ruben"
                };
            user.AddRole(savedRole);
            RunInsideTransaction(() => Repository.Merge(user), true);

            // A company with 2 children are deleted
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(2));
        }

        [Test]
        public void CreateUserWithTwoRoles()
        {
            Role role1 =
                new Role
                {
                    Name = @"Architect"
                };

            Role savedRole1 = RunInsideTransaction(() => RoleRepository.Merge(role1), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(1));

            Role role2 =
                new Role
                {
                    Name = @"Designer"
                };

            Role savedRole2 = RunInsideTransaction(() => RoleRepository.Merge(role2), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(2));

            User user =
                new User
                {
                    Name = @"Ruben"
                };
            user.AddRole(savedRole1);
            user.AddRole(savedRole2);

            RunInsideTransaction(() => Repository.Merge(user), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(3));
        }

        [Test]
        public void CreateUserWithThreeRoles()
        {
            Role role1 =
                new Role
                {
                    Name = @"Architect"
                };

            Role savedRole1 = RunInsideTransaction(() => RoleRepository.Merge(role1), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(1));

            Role role2 =
                new Role
                {
                    Name = @"Designer"
                };

            Role savedRole2 = RunInsideTransaction(() => RoleRepository.Merge(role2), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(2));

            Role role3 =
                new Role
                {
                    Name = @"Developer"
                };

            Role savedRole3 = RunInsideTransaction(() => RoleRepository.Merge(role3), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(3));

            User user =
                new User
                {
                    Name = @"Ruben"
                };
            user.AddRole(savedRole1);
            user.AddRole(savedRole2);
            user.AddRole(savedRole3);

            RunInsideTransaction(() => Repository.Merge(user), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(4));
        }

        [Test]
        public void CreateUserWithThreeRolesAndRemoveOneRole()
        {
            Role role1 =
                new Role
                {
                    Name = @"Architect"
                };

            Role savedRole1 = RunInsideTransaction(() => RoleRepository.Merge(role1), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(1));

            Role role2 =
                new Role
                {
                    Name = @"Designer"
                };

            Role savedRole2 = RunInsideTransaction(() => RoleRepository.Merge(role2), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(2));

            Role role3 =
                new Role
                {
                    Name = @"Developer"
                };

            Role savedRole3 = RunInsideTransaction(() => RoleRepository.Merge(role3), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(3));

            User user =
                new User
                {
                    Name = @"Ruben"
                };
            user.AddRole(savedRole1);
            user.AddRole(savedRole2);
            user.AddRole(savedRole3);

            RunInsideTransaction(
                () =>
                {
                    User savedUser = Repository.Merge(user);
                    savedUser.RemoveRole(savedUser.Roles.Single(r => r.Name == "Developer"));
                    Repository.Merge(savedUser);
                }, true);
        }
    }
}