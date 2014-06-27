using System;
using System.Diagnostics.Contracts;

using NHibernate;
using NHibernate.Cfg;

using PPWCode.Vernacular.nHibernate.I.Interfaces;

namespace PPWCode.Vernacular.nHibernate.I.Utilities
{
    public class NHibernateSessionFactory : INHibernateSessionFactory
    {
        private readonly object m_Locker = new object();
        private readonly INhConfiguration m_NhConfiguration;
        private volatile ISessionFactory m_SessionFactory;

        public NHibernateSessionFactory(INhConfiguration nhConfiguration)
        {
            Contract.Requires<ArgumentNullException>(nhConfiguration != null);

            m_NhConfiguration = nhConfiguration;
        }

        protected Configuration Configuration
        {
            get { return m_NhConfiguration.GetConfiguration(); }
        }

        public virtual ISessionFactory SessionFactory
        {
            get
            {
                if (m_SessionFactory == null)
                {
                    lock (m_Locker)
                    {
                        if (m_SessionFactory == null)
                        {
                            m_SessionFactory = Configuration.BuildSessionFactory();
                            OnAfterCreateSessionFactory(m_SessionFactory);
                        }
                    }
                }
                return m_SessionFactory;
            }
        }

        protected virtual void OnAfterCreateSessionFactory(ISessionFactory sessionFactory)
        {
        }
    }
}