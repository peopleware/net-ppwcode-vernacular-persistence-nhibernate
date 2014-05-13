using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using NHibernate;
using NHibernate.Cfg;

using PPWCode.Vernacular.nHibernate.I.Interfaces;

namespace PPWCode.Vernacular.nHibernate.I.Implementations
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
            if (interceptor == null)
            {
                throw new ArgumentNullException("interceptor");
            }
            if (nhProperties == null)
            {
                throw new ArgumentNullException("nhProperties");
            }
            if (mappingAssemblies == null)
            {
                throw new ArgumentNullException("mappingAssemblies");
            }
            if (registerEventListeners == null)
            {
                throw new ArgumentNullException("registerEventListeners");
            }
            Contract.EndContractBlock();

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
                        m_Configuration.SetInterceptor(Interceptor);
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