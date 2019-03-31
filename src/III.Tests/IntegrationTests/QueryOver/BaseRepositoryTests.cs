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

using System;

using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests.QueryOver
{
    public abstract class BaseRepositoryTests<T> : BaseQueryTests
        where T : class, IIdentity<int>
    {
        protected abstract Func<IQueryOverRepository<T, int>> RepositoryFactory { get; }

        protected IQueryOverRepository<T, int> Repository { get; private set; }

        protected override void OnSetup()
        {
            base.OnSetup();

            Repository = RepositoryFactory();
            SessionFactory.Statistics.Clear();
        }

        protected override void OnTeardown()
        {
            Repository = null;

            base.OnTeardown();
        }
    }
}
