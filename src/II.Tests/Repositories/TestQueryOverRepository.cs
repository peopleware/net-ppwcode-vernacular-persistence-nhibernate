// Copyright 2017-2018 by PeopleWare n.v..
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

using PPWCode.Vernacular.NHibernate.I.Implementations;
using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Repositories
{
    public abstract class TestQueryOverRepository<T>
        : QueryOverRepository<T, int>,
          ITestQueryOverRepository<T>
        where T : class, IIdentity<int>
    {
        protected TestQueryOverRepository(ISessionProvider sessionProvider)
            : base(sessionProvider)
        {
        }
    }
}
