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

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Test
{
    public abstract class BaseRepositoryFixture<T, TId> : NHibernateSqlServerSetUpFixture<TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        private IRepository<T, TId> m_Repository;
        private DateTime? m_UtcNow;

        protected abstract Func<IRepository<T, TId>> RepositoryFactory { get; }

        protected IRepository<T, TId> Repository
        {
            get { return m_Repository; }
        }

        protected override DateTime UtcNow
        {
            get
            {
                if (m_UtcNow == null)
                {
                    m_UtcNow = DateTime.UtcNow;
                }

                return m_UtcNow.Value;
            }
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            m_Repository = RepositoryFactory();
        }

        protected override void OnTeardown()
        {
            m_UtcNow = null;
            m_Repository = null;

            base.OnTeardown();
        }
    }
}
