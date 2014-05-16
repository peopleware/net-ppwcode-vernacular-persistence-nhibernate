using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using NHibernate;
using NHibernate.Engine;
using NHibernate.Persister.Entity;
using NHibernate.Proxy;
using NHibernate.Type;

namespace PPWCode.Vernacular.nHibernate.I.Utilities
{
    public class DirtyCheckingInterceptor : EmptyInterceptor
    {
        private readonly IList<string> m_DirtyProps;
        private ISession m_Session;

        public DirtyCheckingInterceptor(IList<string> dirtyProps)
        {
            if (dirtyProps == null)
            {
                throw new ArgumentNullException("dirtyProps");
            }
            Contract.EndContractBlock();

            m_DirtyProps = dirtyProps;
        }

        public override void SetSession(ISession session)
        {
            m_Session = session;
        }

        public override bool OnFlushDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames, IType[] types)
        {
            string msg = string.Format("Flush Dirty {0}", entity.GetType().FullName);
            m_DirtyProps.Add(msg);
            ListDirtyProperties(entity);
            return false;
        }

        public override bool OnSave(object entity, object id, object[] state, string[] propertyNames, IType[] types)
        {
            string msg = string.Format("Save {0}", entity.GetType().FullName);
            m_DirtyProps.Add(msg);
            return false;
        }

        public override void OnDelete(object entity, object id, object[] state, string[] propertyNames, IType[] types)
        {
            string msg = string.Format("Delete {0}", entity.GetType().FullName);
            m_DirtyProps.Add(msg);
        }

        private void ListDirtyProperties(object entity)
        {
            string className = NHibernateProxyHelper.GuessClass(entity).FullName;
            ISessionImplementor sessionImpl = m_Session.GetSessionImplementation();
            IEntityPersister persister = sessionImpl.Factory.GetEntityPersister(className);
            EntityEntry oldEntry = sessionImpl.PersistenceContext.GetEntry(entity);

            if (oldEntry == null)
            {
                INHibernateProxy proxy = entity as INHibernateProxy;
                object obj = proxy != null ? sessionImpl.PersistenceContext.Unproxy(proxy) : entity;
                oldEntry = sessionImpl.PersistenceContext.GetEntry(obj);
            }

            object[] oldState = oldEntry.LoadedState;
            object[] currentState = persister.GetPropertyValues(entity, sessionImpl.EntityMode);
            int[] dirtyProperties = persister.FindDirty(currentState, oldState, entity, sessionImpl);

            foreach (int index in dirtyProperties)
            {
                string msg = string.Format(
                    "Dirty property {0}.{1} was {2}, is {3}.",
                    className,
                    persister.PropertyNames[index],
                    oldState[index] ?? "null",
                    currentState[index] ?? "null");
                m_DirtyProps.Add(msg);
            }
        }
    }
}