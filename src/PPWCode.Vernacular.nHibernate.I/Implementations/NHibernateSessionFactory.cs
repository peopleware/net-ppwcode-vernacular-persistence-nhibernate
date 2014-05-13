using System;
using System.Diagnostics.Contracts;

using NHibernate;

using PPWCode.Vernacular.nHibernate.I.Interfaces;

namespace PPWCode.Vernacular.nHibernate.I.Implementations
{
    public sealed class NHibernateSessionFactory
    {
        private readonly object m_Locker = new object();
        private readonly INhConfiguration m_NhConfiguration;
        private volatile ISessionFactory m_SessionFactory;

        public NHibernateSessionFactory(INhConfiguration nhConfiguration)
        {
            if (nhConfiguration == null)
            {
                throw new ArgumentNullException("nhConfiguration");
            }
            Contract.EndContractBlock();

            m_NhConfiguration = nhConfiguration;
        }

        public ISessionFactory SessionFactory
        {
            get
            {
                if (m_SessionFactory == null)
                {
                    lock (m_Locker)
                    {
                        if (m_SessionFactory == null)
                        {
                            m_SessionFactory = m_NhConfiguration
                                .GetConfiguration()
                                .BuildSessionFactory();
                        }
                    }
                }
                return m_SessionFactory;
            }
        }
    }
}