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

using System.Linq;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.I.Tests.Models;
using PPWCode.Vernacular.NHibernate.I.Tests.Repositories;

namespace PPWCode.Vernacular.NHibernate.I.Tests.IntegrationTests.Linq
{
    public class UserTests : BaseUserTests
    {
        private RoleLinqRepository m_RoleRepository;

        protected RoleLinqRepository RoleRepository
        {
            get { return m_RoleRepository; }
        }

        protected IUserLinqRepository UserRepository
        {
            get { return (IUserLinqRepository)Repository; }
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            m_RoleRepository = new RoleLinqRepository(Session);
        }

        protected override void OnTeardown()
        {
            m_RoleRepository = null;

            base.OnTeardown();
        }

        protected User CreateUser(string name = @"Ruben", Gender gender = Gender.MALE)
        {
            return
                new User
                {
                    Name = name,
                    Gender = gender
                };
        }

        protected Role CreateRole(string name)
        {
            return
                new Role
                {
                    Name = name
                };
        }

        [Test]
        public void CreateUserWithoutRoles()
        {
            RunInsideTransaction(() => Repository.Merge(CreateUser()), true);

            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(1));
        }

        [Test]
        public void CreateUserWithOneRole()
        {
            Role role = RunInsideTransaction(() => RoleRepository.Merge(CreateRole(@"Architect")), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(1));

            User user = CreateUser();
            user.AddRole(role);
            RunInsideTransaction(() => Repository.Merge(user), true);

            // A company with 2 children are deleted
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(2));
        }

        [Test]
        public void CreateUserWithTwoRoles()
        {
            Role role1 = RunInsideTransaction(() => RoleRepository.Merge(CreateRole(@"Architect")), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(1));

            Role role2 = RunInsideTransaction(() => RoleRepository.Merge(CreateRole(@"Designer")), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(2));

            User user = CreateUser();
            user.AddRole(role1);
            user.AddRole(role2);

            RunInsideTransaction(() => Repository.Merge(user), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(3));
        }

        [Test]
        public void CreateUserWithThreeRoles()
        {
            Role role1 = RunInsideTransaction(() => RoleRepository.Merge(CreateRole(@"Architect")), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(1));

            Role role2 = RunInsideTransaction(() => RoleRepository.Merge(CreateRole(@"Designer")), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(2));

            Role role3 = RunInsideTransaction(() => RoleRepository.Merge(CreateRole(@"Developer")), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(3));

            User user = CreateUser();
            user.AddRole(role1);
            user.AddRole(role2);
            user.AddRole(role3);

            RunInsideTransaction(() => Repository.Merge(user), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(4));
        }

        [Test]
        public void CreateUserWithThreeRolesAndRemoveOneRole()
        {
            Role role1 = RunInsideTransaction(() => RoleRepository.Merge(CreateRole(@"Architect")), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(1));

            Role role2 = RunInsideTransaction(() => RoleRepository.Merge(CreateRole(@"Designer")), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(2));

            Role role3 = RunInsideTransaction(() => RoleRepository.Merge(CreateRole(@"Developer")), true);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(3));

            User user = CreateUser();
            user.AddRole(role1);
            user.AddRole(role2);
            user.AddRole(role3);

            RunInsideTransaction(
                () =>
                {
                    User savedUser = Repository.Merge(user);
                    savedUser.RemoveRole(savedUser.Roles.Single(r => r.Name == "Developer"));
                    Repository.Merge(savedUser);
                }, true);
        }

        [Test]
        public void FindUserByName()
        {
            User ruben =
                new User
                {
                    Name = "Ruben",
                    Gender = Gender.MALE
                };
            User danny =
                new User
                {
                    Name = "Danny",
                    Gender = Gender.FEMALE
                };
            RunInsideTransaction(() => UserRepository.Merge(ruben), true);
            RunInsideTransaction(() => UserRepository.Merge(danny), true);

            User foundRuben = RunInsideTransaction(() => UserRepository.GetUserByName("Ruben"), true);
            Assert.That(foundRuben, Is.Not.Null);
            Assert.That(foundRuben.Name, Is.EqualTo("Ruben"));

            User jef = RunInsideTransaction(() => UserRepository.GetUserByName("Jef"), true);
            Assert.That(jef, Is.Null);
        }
    }
}
