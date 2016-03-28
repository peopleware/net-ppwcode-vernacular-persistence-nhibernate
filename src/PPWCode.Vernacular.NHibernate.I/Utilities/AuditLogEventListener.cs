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
using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.Persistence.II;

using Environment = System.Environment;

namespace PPWCode.Vernacular.NHibernate.I.Utilities
{
    public abstract class AuditLogEventListener<TId, TAuditEntity> :
        IRegisterEventListener,
        IPostUpdateEventListener,
        IPostInsertEventListener,
        IPostDeleteEventListener
        where TId : IEquatable<TId>
        where TAuditEntity : AuditLog<TId>, new()
    {
        private static readonly ConcurrentDictionary<Type, AuditLogItem> s_DomainTypes =
            new ConcurrentDictionary<Type, AuditLogItem>();

        private readonly IIdentityProvider m_IdentityProvider;
        private readonly ITimeProvider m_TimeProvider;

        protected AuditLogEventListener(IIdentityProvider identityProvider, ITimeProvider timeProvider)
        {
            Contract.Requires(identityProvider != null);
            Contract.Requires(timeProvider != null);
            Contract.Ensures(IdentityProvider == identityProvider);
            Contract.Ensures(TimeProvider == timeProvider);

            m_IdentityProvider = identityProvider;
            m_TimeProvider = timeProvider;
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(IdentityProvider != null);
            Contract.Invariant(TimeProvider != null);
        }

        protected IIdentityProvider IdentityProvider
        {
            get
            {
                Contract.Ensures(Contract.Result<IIdentityProvider>() != null);

                return m_IdentityProvider;
            }
        }

        protected ITimeProvider TimeProvider
        {
            get
            {
                Contract.Ensures(Contract.Result<ITimeProvider>() != null);

                return m_TimeProvider;
            }
        }

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

        public virtual void OnPostUpdate(PostUpdateEvent @event)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.UPDATE) == AuditLogActionEnum.UPDATE)
            {
                string identityName = IdentityProvider.IdentityName;
                DateTime now = TimeProvider.Now.ToUniversalTime();
                string entityName = @event.Entity.GetType().Name;

                if (@event.OldState == null)
                {
                    throw new ProgrammingError(
                        string.Format(
                            "No old state available for entity type '{1}'.{0}Make sure you're loading it into Session before modifying and saving it.",
                            Environment.NewLine,
                            entityName));
                }

                List<TAuditEntity> auditLogs = new List<TAuditEntity>();
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
                                    new TAuditEntity
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
                    session.Flush();
                }
            }
        }

        public virtual void OnPostInsert(PostInsertEvent @event)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.CREATE) == AuditLogActionEnum.CREATE)
            {
                string identityName = IdentityProvider.IdentityName;
                DateTime now = TimeProvider.Now.ToUniversalTime();
                string entityName = @event.Entity.GetType().Name;

                List<TAuditEntity> auditLogs = new List<TAuditEntity>();
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
                                new TAuditEntity
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
                    session.Flush();
                }
            }
        }

        public virtual void OnPostDelete(PostDeleteEvent @event)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.DELETE) == AuditLogActionEnum.DELETE)
            {
                string identityName = IdentityProvider.IdentityName;
                DateTime now = TimeProvider.Now.ToUniversalTime();
                string entityName = @event.Entity.GetType().Name;

                ISession session = @event.Session.GetSession(EntityMode.Poco);
                session.Save(
                    new TAuditEntity
                    {
                        EntryType = "D",
                        EntityName = entityName,
                        EntityId = @event.Id.ToString(),
                        CreatedBy = identityName,
                        CreatedAt = now,
                    });
                session.Flush();
            }
        }

        protected static string GetStringValueFromStateArray(object[] stateArray, int position)
        {
            object value = stateArray[position];
            return value == null ? null : value.ToString();
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
                            if (@type != typeof(TAuditEntity))
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