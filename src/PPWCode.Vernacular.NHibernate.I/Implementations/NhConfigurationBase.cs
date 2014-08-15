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
        private readonly INhInterceptor m_NhInterceptor;
        private readonly object m_Locker = new object();
        private readonly INhProperties m_NhProperties;
        private readonly IMappingAssemblies m_MappingAssemblies;
        private readonly IHbmMapping m_HbmMapping;
        private readonly IRegisterEventListener[] m_RegisterEventListeners;
        private volatile Configuration m_Configuration;

        protected NhConfigurationBase(INhInterceptor nhInterceptor, INhProperties nhProperties, IMappingAssemblies mappingAssemblies, IHbmMapping hbmMapping, IRegisterEventListener[] registerEventListeners)
        {
            Contract.Requires(nhInterceptor != null);
            Contract.Requires(nhProperties != null);
            Contract.Requires(mappingAssemblies != null);
            Contract.Requires(hbmMapping != null);
            Contract.Requires(registerEventListeners != null);

            m_NhInterceptor = nhInterceptor;
            m_NhProperties = nhProperties;
            m_MappingAssemblies = mappingAssemblies;
            m_HbmMapping = hbmMapping;
            m_RegisterEventListeners = registerEventListeners;
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

        protected IHbmMapping HbmMapping
        {
            get { return m_HbmMapping; }
        }

        protected IMappingAssemblies MappingAssemblies
        {
            get { return m_MappingAssemblies; }
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
                        IInterceptor interceptor = NhInterceptor.GetInterceptor();
                        if (interceptor != null)
                        {
                            m_Configuration.SetInterceptor(interceptor);
                        }

                        foreach (IRegisterEventListener registerListener in RegisterEventListeners)
                        {
                            registerListener.Register(m_Configuration);
                        }
                    }
                }
            }

            return m_Configuration;
        }
    }
}