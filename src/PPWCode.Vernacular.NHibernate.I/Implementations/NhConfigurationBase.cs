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

using System.Collections.Generic;

using Castle.Core.Logging;

using NHibernate.Cfg;
using NHibernate.Mapping;

using PPWCode.Vernacular.NHibernate.I.Interfaces;

namespace PPWCode.Vernacular.NHibernate.I.Implementations
{
    public abstract class NhConfigurationBase : INhConfiguration
    {
        private readonly object _locker = new object();
        private readonly IMappingAssemblies _mappingAssemblies;
        private readonly INhInterceptor _nhInterceptor;
        private readonly INhProperties _nhProperties;
        private readonly IPpwHbmMapping _ppwHbmMapping;
        private readonly IRegisterEventListener[] _registerEventListeners;
        private volatile IAuxiliaryDatabaseObject[] _auxiliaryDatabaseObjects;
        private volatile Configuration _configuration;
        private ILogger _logger = NullLogger.Instance;

        protected NhConfigurationBase(
            INhInterceptor nhInterceptor,
            INhProperties nhProperties,
            IMappingAssemblies mappingAssemblies,
            IPpwHbmMapping ppwHbmMapping,
            IRegisterEventListener[] registerEventListeners,
            IAuxiliaryDatabaseObject[] auxiliaryDatabaseObjects)
        {
            _nhInterceptor = nhInterceptor;
            _nhProperties = nhProperties;
            _mappingAssemblies = mappingAssemblies;
            _ppwHbmMapping = ppwHbmMapping;
            _registerEventListeners = registerEventListeners;
            _auxiliaryDatabaseObjects = auxiliaryDatabaseObjects;
        }

        protected INhProperties NhProperties
            => _nhProperties;

        protected IEnumerable<IRegisterEventListener> RegisterEventListeners
            => _registerEventListeners;

        protected abstract Configuration Configuration { get; }

        protected INhInterceptor NhInterceptor
            => _nhInterceptor;

        protected IPpwHbmMapping PpwHbmMapping
            => _ppwHbmMapping;

        protected IMappingAssemblies MappingAssemblies
            => _mappingAssemblies;

        protected IAuxiliaryDatabaseObject[] AuxiliaryDatabaseObjects
            => _auxiliaryDatabaseObjects;

        public ILogger Logger
        {
            get { return _logger; }
            set
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (value != null)
                {
                    _logger = value;
                }
            }
        }

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
