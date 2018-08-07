// Copyright 2017 by PeopleWare n.v..
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

using JetBrains.Annotations;

using NHibernate.Cfg;
using NHibernate.Mapping;

using PPWCode.Vernacular.NHibernate.II.Interfaces;

namespace PPWCode.Vernacular.NHibernate.II.Implementations
{
    public abstract class NhConfigurationBase : INhConfiguration
    {
        private readonly object _locker = new object();
        private volatile Configuration _configuration;

        protected NhConfigurationBase(
            [NotNull] INhInterceptor nhInterceptor,
            [NotNull] INhProperties nhProperties,
            [NotNull] IMappingAssemblies mappingAssemblies,
            [NotNull] IPpwHbmMapping ppwHbmMapping,
            [NotNull] IRegisterEventListener[] registerEventListeners,
            [NotNull] IAuxiliaryDatabaseObject[] auxiliaryDatabaseObjects)
        {
            NhInterceptor = nhInterceptor;
            NhProperties = nhProperties;
            MappingAssemblies = mappingAssemblies;
            PpwHbmMapping = ppwHbmMapping;
            RegisterEventListeners = registerEventListeners;
            AuxiliaryDatabaseObjects = auxiliaryDatabaseObjects;
        }

        [NotNull]
        protected INhProperties NhProperties { get; }

        [NotNull]
        protected IEnumerable<IRegisterEventListener> RegisterEventListeners { get; }

        [NotNull]
        protected abstract Configuration Configuration { get; }

        [NotNull]
        protected INhInterceptor NhInterceptor { get; }

        [NotNull]
        protected IPpwHbmMapping PpwHbmMapping { get; }

        [NotNull]
        protected IMappingAssemblies MappingAssemblies { get; }

        [NotNull]
        protected IAuxiliaryDatabaseObject[] AuxiliaryDatabaseObjects { get; }

        public Configuration GetConfiguration()
        {
            if (_configuration == null)
            {
                lock (_locker)
                {
                    if (_configuration == null)
                    {
                        _configuration = Configuration;
                    }
                }
            }

            return _configuration;
        }
    }
}
