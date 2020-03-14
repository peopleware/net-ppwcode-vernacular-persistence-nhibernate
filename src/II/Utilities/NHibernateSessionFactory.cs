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

using JetBrains.Annotations;

using NHibernate;
using NHibernate.Cfg;

namespace PPWCode.Vernacular.NHibernate.II
{
    /// <inheritdoc />
    public class NHibernateSessionFactory : INHibernateSessionFactory
    {
        private readonly object _locker = new object();
        private readonly INhConfiguration _nhConfiguration;
        private volatile ISessionFactory _sessionFactory;

        public NHibernateSessionFactory([NotNull] INhConfiguration nhConfiguration)
        {
            _nhConfiguration = nhConfiguration;
        }

        [NotNull]
        protected Configuration Configuration
            => _nhConfiguration.GetConfiguration();

        /// <inheritdoc cref="ISessionFactory" />
        public virtual ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                {
                    lock (_locker)
                    {
                        if (_sessionFactory == null)
                        {
                            _sessionFactory = Configuration.BuildSessionFactory();
                            OnAfterCreateSessionFactory(_sessionFactory);
                        }
                    }
                }

                return _sessionFactory;
            }
        }

        protected virtual void OnAfterCreateSessionFactory([NotNull] ISessionFactory sessionFactory)
        {
        }
    }
}
