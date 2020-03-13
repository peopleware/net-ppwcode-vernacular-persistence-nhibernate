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

using JetBrains.Annotations;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.III.Tests.Models;

namespace PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests.Async.Linq
{
    public class UserTests : BaseUserTests
    {
        protected User CreateUserModel(string name = "Ruben", Gender gender = Gender.MALE)
            => new User
               {
                   Name = name,
                   Gender = gender
               };

        protected Role CreateRoleModel(string name)
            => new Role
               {
                   Name = name
               };

        protected async Task<IList<Role>> CreateRolesAsync(
            [NotNull] [ItemNotNull] IEnumerable<string> roleNames,
            bool clearSession,
            CancellationToken cancellationToken)
        {
            long previousEntityInsertCount = SessionFactory.Statistics.EntityInsertCount;
            List<Role> roles = new List<Role>();

            async Task Action(CancellationToken can)
            {
                foreach (string roleName in roleNames)
                {
                    roles.Add(await RoleRepository.MergeAsync(CreateRoleModel(roleName), can));
                }
            }

            await RunInsideTransactionAsync(Action, clearSession, cancellationToken);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(previousEntityInsertCount + roleNames.Count()));
            return roles;
        }

        protected async Task<Role> CreateRoleAsync(
            [NotNull] string roleName,
            bool clearSession,
            CancellationToken cancellationToken)
            => (await CreateRolesAsync(new[] { roleName }, clearSession, cancellationToken)
                    .ConfigureAwait(false))
                .Single();

        [Test]
        public async Task CreateUserWithOneRole()
        {
            Role role = await CreateRoleAsync("Architect", true, CancellationToken);
            User user = CreateUserModel();
            user.AddRole(role);
            await RunInsideTransactionAsync(can => Repository.MergeAsync(user, can), true, CancellationToken);

            // A company with 2 children are deleted
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(2));
        }

        [Test]
        public async Task CreateUserWithoutRoles()
        {
            await RunInsideTransactionAsync(can => Repository.MergeAsync(CreateUserModel(), can), true, CancellationToken);

            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(1));
        }

        [Test]
        public async Task CreateUserWithThreeRoles()
        {
            User user = CreateUserModel();
            IList<Role> roles =
                await CreateRolesAsync(
                    new[] { "Architect", "Designer", "Developer" },
                    true,
                    CancellationToken);
            foreach (Role role in roles)
            {
                user.AddRole(role);
            }

            await RunInsideTransactionAsync(can => Repository.MergeAsync(user, can), true, CancellationToken);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(4));
        }

        [Test]
        public async Task CreateUserWithThreeRolesAndRemoveOneRole()
        {
            User user = CreateUserModel();
            IList<Role> roles =
                await CreateRolesAsync(
                    new[] { "Architect", "Designer", "Developer" },
                    true,
                    CancellationToken);
            foreach (Role role in roles)
            {
                user.AddRole(role);
            }

            await RunInsideTransactionAsync(
                async can =>
                {
                    User savedUser = await Repository.MergeAsync(user, can);
                    savedUser.RemoveRole(savedUser.Roles.Single(r => r.Name == "Developer"));
                    await Repository.MergeAsync(savedUser, can);
                },
                true,
                CancellationToken);
        }

        [Test]
        public async Task CreateUserWithTwoRoles()
        {
            User user = CreateUserModel();
            IList<Role> roles =
                await CreateRolesAsync(
                    new[] { "Architect", "Designer" },
                    true,
                    CancellationToken);
            foreach (Role role in roles)
            {
                user.AddRole(role);
            }

            await RunInsideTransactionAsync(can => Repository.MergeAsync(user, can), true, CancellationToken);
            Assert.That(SessionFactory.Statistics.EntityInsertCount, Is.EqualTo(3));
        }

        [Test]
        public async Task FindUserByName()
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
            await RunInsideTransactionAsync(can => Repository.MergeAsync(ruben, can), true, CancellationToken);
            await RunInsideTransactionAsync(can => Repository.MergeAsync(danny, can), true, CancellationToken);

            User foundRuben = await RunInsideTransactionAsync(can => Repository.GetUserByNameAsync("Ruben", can), true, CancellationToken);
            Assert.That(foundRuben, Is.Not.Null);
            Assert.That(foundRuben.Name, Is.EqualTo("Ruben"));

            User jef = await RunInsideTransactionAsync(can => Repository.GetUserByNameAsync("Jef", can), true, CancellationToken);
            Assert.That(jef, Is.Null);
        }
    }
}
