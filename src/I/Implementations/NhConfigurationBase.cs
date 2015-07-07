// Copyright 2014 by PeopleWare n.v..
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
using System.Diagnostics.Contracts;

using NHibernate;
using NHibernate.Cfg;

using PPWCode.Vernacular.NHibernate.I.Interfaces;

namespace PPWCode.Vernacular.NHibernate.I.Implementations
{
    public abstract class NhConfigurationBase : INhConfiguration
    {
        private readonly IInterceptor m_Interceptor;
        private readonly object m_Locker = new object();
        private readonly IMappingAssemblies m_MappingAssemblies;
        private readonly INhProperties m_NhProperties;
        private readonly IRegisterEventListener[] m_RegisterEventListeners;
        private volatile Configuration m_Configuration;

        protected NhConfigurationBase(IInterceptor interceptor, INhProperties nhProperties, IMappingAssemblies mappingAssemblies, IRegisterEventListener[] registerEventListeners)
        {
            Contract.Requires(interceptor != null);
            Contract.Requires(nhProperties != null);
            Contract.Requires(mappingAssemblies != null);
            Contract.Requires(registerEventListeners != null);

            m_Interceptor = interceptor;
            m_NhProperties = nhProperties;
            m_MappingAssemblies = mappingAssemblies;
            m_RegisterEventListeners = registerEventListeners;
        }

        protected IMappingAssemblies MappingAssemblies
        {
            get { return m_MappingAssemblies; }
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

        protected IInterceptor Interceptor
        {
            get { return m_Interceptor; }
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