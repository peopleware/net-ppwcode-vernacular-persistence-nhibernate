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

using PPWCode.Vernacular.NHibernate.II.Tests.IntegrationTests.Sync.QueryOver.Repositories;
using PPWCode.Vernacular.NHibernate.II.Tests.Models;

namespace PPWCode.Vernacular.NHibernate.II.Tests.IntegrationTests.Sync.QueryOver
{
    public abstract class BaseUserTests : BaseRepositoryTests<User>
    {
        protected override void OnSetup()
        {
            base.OnSetup();

            Repository = new UserRepository(SessionProvider);
        }

        protected override void OnTeardown()
        {
            Repository = null;

            base.OnTeardown();
        }

        protected UserRepository Repository { get; private set; }
    }
}
