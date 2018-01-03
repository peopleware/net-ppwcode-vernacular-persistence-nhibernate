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

using System.Collections.Generic;

using Castle.Core.Logging;

using NHibernate.Cfg;
using NHibernate.Mapping;

using PPWCode.Vernacular.NHibernate.I.Interfaces;

namespace PPWCode.Vernacular.NHibernate.I.Implementations
{
    public abstract class NhConfigurationBase : INhConfiguration
    {
        private readonly INhInterceptor m_NhInterceptor;
        private readonly object m_Locker = new object();
        private readonly INhProperties m_NhProperties;
        private readonly IMappingAssemblies m_MappingAssemblies;
        private readonly IPpwHbmMapping m_PpwHbmMapping;
        private readonly IRegisterEventListener[] m_RegisterEventListeners;
        private volatile IAuxiliaryDatabaseObject[] m_AuxiliaryDatabaseObjects;
        private volatile Configuration m_Configuration;
        private ILogger m_Logger = NullLogger.Instance;

        protected NhConfigurationBase(
            INhInterceptor nhInterceptor, 
            INhProperties nhProperties, 
            IMappingAssemblies mappingAssemblies, 
            IPpwHbmMapping ppwHbmMapping, 
            IRegisterEventListener[] registerEventListeners,
            IAuxiliaryDatabaseObject[] auxiliaryDatabaseObjects)
        {
            m_NhInterceptor = nhInterceptor;
            m_NhProperties = nhProperties;
            m_MappingAssemblies = mappingAssemblies;
            m_PpwHbmMapping = ppwHbmMapping;
            m_RegisterEventListeners = registerEventListeners;
            m_AuxiliaryDatabaseObjects = auxiliaryDatabaseObjects;
        }

        protected INhProperties NhProperties
        {
            get { return m_NhProperties; }
        }

        protected IEnumerable<IRegisterEventListener> RegisterEventListeners
        {
            get { return m_RegisterEventListeners; }
        }

        protected abstract Configuration Configuration { get; }

        protected INhInterceptor NhInterceptor
        {
            get { return m_NhInterceptor; }
        }

        protected IPpwHbmMapping PpwHbmMapping
        {
            get { return m_PpwHbmMapping; }
        }

        protected IMappingAssemblies MappingAssemblies
        {
            get { return m_MappingAssemblies; }
        }

        protected IAuxiliaryDatabaseObject[] AuxiliaryDatabaseObjects
        {
            get { return m_AuxiliaryDatabaseObjects; }
        }

        public ILogger Logger
        {
            get { return m_Logger; }
            set
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (value != null)
                {
                    m_Logger = value;
                }
            }
        }

        public Configuration GetConfiguration()
        {
            if (m_Configuration == null)
            {
                lock (m_Locker)
                {
                    if (m_Configuration == null)
                    {
                        m_Configuration = Configuration;
                    }
                }
            }

            return m_Configuration;
        }
    }
}
