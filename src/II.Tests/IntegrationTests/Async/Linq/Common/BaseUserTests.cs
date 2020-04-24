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

using JetBrains.Annotations;

using PPWCode.Vernacular.NHibernate.II.Tests.IntegrationTests.Async.Linq.Common.Repositories;
using PPWCode.Vernacular.NHibernate.II.Tests.Model.Common;

namespace PPWCode.Vernacular.NHibernate.II.Tests.IntegrationTests.Async.Linq.Common
{
    public abstract class BaseUserTests : BaseRepositoryTests<User>
    {
        [CanBeNull]
        private IUserRepository _repository;

        [CanBeNull]
        private IRoleRepository _roleRepository;

        protected override void OnTeardown()
        {
            _repository = null;
            _roleRepository = null;

            base.OnTeardown();
        }

        [NotNull]
        protected IUserRepository Repository
            => _repository ?? (_repository = new UserRepository(SessionProviderAsync));

        [NotNull]
        protected IRoleRepository RoleRepository
            => _roleRepository ?? (_roleRepository = new RoleRepository(SessionProviderAsync));
    }
}
