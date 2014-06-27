using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Event;

using PPWCode.Vernacular.Exceptions.II;
using PPWCode.Vernacular.nHibernate.I.Interfaces;
using PPWCode.Vernacular.Persistence.II;

using Environment = System.Environment;

namespace PPWCode.Vernacular.nHibernate.I.Utilities
{
    public abstract class AuditLogEventListener<T, U> :
        IRegisterEventListener,
        IPostUpdateEventListener,
        IPostInsertEventListener,
        IPostDeleteEventListener
        where T : IEquatable<T>
        where U : AuditLog<T>, new()
    {
        private static readonly ConcurrentDictionary<Type, AuditLogItem> s_DomainTypes =
            new ConcurrentDictionary<Type, AuditLogItem>();

        private readonly IIdentityProvider m_IdentityProvider;
        private readonly ITimeProvider m_TimeProvider;

        protected AuditLogEventListener(IIdentityProvider identityProvider, ITimeProvider timeProvider)
        {
            Contract.Requires<ArgumentNullException>(identityProvider != null);
            Contract.Requires<ArgumentNullException>(timeProvider != null);

            m_IdentityProvider = identityProvider;
            m_TimeProvider = timeProvider;
        }

        #region Implementation of IRegisterEventListener

        public void Register(Configuration cfg)
        {
            cfg.EventListeners.PostUpdateEventListeners = new IPostUpdateEventListener[] { this }
                .Concat(cfg.EventListeners.PostUpdateEventListeners)
                .ToArray();
            cfg.EventListeners.PostInsertEventListeners = new IPostInsertEventListener[] { this }
                .Concat(cfg.EventListeners.PostInsertEventListeners)
                .ToArray();
            cfg.EventListeners.PostDeleteEventListeners = new IPostDeleteEventListener[] { this }
                .Concat(cfg.EventListeners.PostDeleteEventListeners)
                .ToArray();
        }

        #endregion

        #region IPostUpdateEventListener Members

        public virtual void OnPostUpdate(PostUpdateEvent @event)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.UPDATE) == AuditLogActionEnum.UPDATE)
            {
                string identityName = m_IdentityProvider.IdentityName;
                DateTime now = m_TimeProvider.Now.ToUniversalTime();
                string entityName = @event.Entity.GetType().Name;

                if (@event.OldState == null)
                {
                    throw new ProgrammingError(
                        string.Format(
                            "No old state available for entity type '{1}'.{0}Make sure you're loading it into Session before modifying and saving it.",
                            Environment.NewLine,
                            entityName));
                }

                List<U> auditLogs = new List<U>();
                int[] fieldIndices = @event.Persister.FindDirty(@event.State, @event.OldState, @event.Entity, @event.Session);
                foreach (int dirtyFieldIndex in fieldIndices)
                {
                    string oldValue = GetStringValueFromStateArray(@event.OldState, dirtyFieldIndex);
                    string newValue = GetStringValueFromStateArray(@event.State, dirtyFieldIndex);

                    if (oldValue != newValue)
                    {
                        string propertyName = @event.Persister.PropertyNames[dirtyFieldIndex];
                        AuditLogActionEnum auditLogAction;
                        if (auditLogItem.Properties.TryGetValue(propertyName, out auditLogAction))
                        {
                            if ((auditLogAction & AuditLogActionEnum.UPDATE) == AuditLogActionEnum.NONE)
                            {
                                auditLogs.Add(
                                    new U
                                    {
                                        EntryType = "U",
                                        EntityName = entityName,
                                        EntityId = @event.Id.ToString(),
                                        PropertyName = propertyName,
                                        OldValue = oldValue,
                                        NewValue = newValue,
                                        CreatedBy = identityName,
                                        CreatedAt = now,
                                    });
                            }
                        }
                    }
                }
                if (auditLogs.Count > 0)
                {
                    ISession session = @event.Session.GetSession(EntityMode.Poco);
                    auditLogs.ForEach(o => session.Save(o));
                }
            }
        }

        #endregion

        #region IPostInsertEventListener Members

        public virtual void OnPostInsert(PostInsertEvent @event)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.CREATE) == AuditLogActionEnum.CREATE)
            {
                string identityName = m_IdentityProvider.IdentityName;
                DateTime now = m_TimeProvider.Now.ToUniversalTime();
                string entityName = @event.Entity.GetType().Name;

                List<U> auditLogs = new List<U>();
                int length = @event.State.Count();
                for (int fieldIndex = 0; fieldIndex < length; fieldIndex++)
                {
                    string newValue = GetStringValueFromStateArray(@event.State, fieldIndex);

                    string propertyName = @event.Persister.PropertyNames[fieldIndex];
                    AuditLogActionEnum auditLogAction;
                    if (auditLogItem.Properties.TryGetValue(propertyName, out auditLogAction))
                    {
                        if ((auditLogAction & AuditLogActionEnum.CREATE) == AuditLogActionEnum.NONE)
                        {
                            auditLogs.Add(
                                new U
                                {
                                    EntryType = "I",
                                    EntityName = entityName,
                                    EntityId = @event.Id.ToString(),
                                    PropertyName = propertyName,
                                    OldValue = null,
                                    NewValue = newValue,
                                    CreatedBy = identityName,
                                    CreatedAt = now,
                                });
                        }
                    }
                }
                if (auditLogs.Count > 0)
                {
                    ISession session = @event.Session.GetSession(EntityMode.Poco);
                    auditLogs.ForEach(o => session.Save(o));
                }
            }
        }

        #endregion

        #region IPostDeleteEventListener Members

        public virtual void OnPostDelete(PostDeleteEvent @event)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.DELETE) == AuditLogActionEnum.DELETE)
            {
                string identityName = m_IdentityProvider.IdentityName;
                DateTime now = m_TimeProvider.Now.ToUniversalTime();
                string entityName = @event.Entity.GetType().Name;

                ISession session = @event.Session.GetSession(EntityMode.Poco);
                session.Save(
                    new U
                    {
                        EntryType = "D",
                        EntityName = entityName,
                        EntityId = @event.Id.ToString(),
                        CreatedBy = identityName,
                        CreatedAt = now,
                    });
            }
        }

        #endregion

        protected static string GetStringValueFromStateArray(object[] stateArray, int position)
        {
            object value = stateArray[position];
            return value == null ? "<null>" : value.ToString();
        }

        protected class AuditLogItem
        {
            private AuditLogItem()
            {
                AuditLogAction = AuditLogActionEnum.NONE;
                Properties = null;
            }

            public AuditLogActionEnum AuditLogAction { get; private set; }
            public Dictionary<string, AuditLogActionEnum> Properties { get; private set; }

            public static AuditLogItem Find(Type t)
            {
                AuditLogItem result = s_DomainTypes
                    .GetOrAdd(
                        t,
                        @type =>
                        {
                            result = new AuditLogItem();
                            if (@type != typeof(U))
                            {
                                AuditLogAttribute auditLogAttribute =
                                    @type
                                        .GetCustomAttributes(true)
                                        .OfType<AuditLogAttribute>()
                                        .FirstOrDefault();
                                if (auditLogAttribute != null)
                                {
                                    result.AuditLogAction = auditLogAttribute.AuditLogAction;
                                    result.Properties = new Dictionary<string, AuditLogActionEnum>();
                                    foreach (PropertyInfo propertyInfo in t.GetProperties().Where(o => o.CanWrite))
                                    {
                                        AuditLogPropertyIgnoreAttribute auditLogPropertyIgnore =
                                            propertyInfo
                                                .GetCustomAttributes(true)
                                                .OfType<AuditLogPropertyIgnoreAttribute>()
                                                .FirstOrDefault();
                                        result
                                            .Properties
                                            .Add(propertyInfo.Name, auditLogPropertyIgnore != null ? auditLogPropertyIgnore.AuditLogAction : AuditLogActionEnum.NONE);
                                    }
                                }
                            }
                            return result;
                        });
                return result;
            }
        }
    }
}