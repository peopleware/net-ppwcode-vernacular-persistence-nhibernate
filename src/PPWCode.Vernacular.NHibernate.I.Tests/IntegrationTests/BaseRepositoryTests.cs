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

using NHibernate.Cfg.MappingSchema;

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.NHibernate.I.Test;
using PPWCode.Vernacular.NHibernate.I.Tests.Models.Mapping;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.IntegrationTests
{
    public abstract class BaseRepositoryTests<T> : BaseRepositoryFixture<T, int>
        where T : class, IIdentity<int>
    {
        protected override string CatalogName
        {
            get { return "Test.PPWCode.Vernacular.NHibernate.I.Tests"; }
        }

        protected override string ConnectionString
        {
            get { return FixedConnectionString; }
        }

        protected override HbmMapping GetHbmMapping()
        {
            IHbmMapping mapper = new TestsSimpleModelMapper(new TestsMappingAssemblies());
            return mapper.GetHbmMapping();
        }

        protected override string IdentityName
        {
            get { return "Test - IdentityName"; }
        }
    }
}
