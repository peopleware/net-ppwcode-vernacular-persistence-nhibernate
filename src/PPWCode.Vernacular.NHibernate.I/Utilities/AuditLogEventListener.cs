// Copyright 2018 by PeopleWare n.v..
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Event;

using PPWCode.Vernacular.Exceptions.II;
using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.Persistence.II;

using Environment = System.Environment;

namespace PPWCode.Vernacular.NHibernate.I.Utilities
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Castle Windsor usage")]
    [Serializable]
    public abstract class AuditLogEventListener<TId, TAuditEntity>
        : IRegisterEventListener,
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
        private readonly bool m_UseUtc;

        protected AuditLogEventListener(IIdentityProvider identityProvider, ITimeProvider timeProvider, bool useUtc)
        {
            m_IdentityProvider = identityProvider;
            m_TimeProvider = timeProvider;
            m_UseUtc = useUtc;
        }

        public IIdentityProvider IdentityProvider => m_IdentityProvider;

        public ITimeProvider TimeProvider => m_TimeProvider;

        public bool UseUtc => m_UseUtc;

        public virtual void Register(Configuration cfg)
        {
            cfg.EventListeners.PostUpdateEventListeners =
                new IPostUpdateEventListener[] { this }
                    .Concat(cfg.EventListeners.PostUpdateEventListeners)
                    .ToArray();
            cfg.EventListeners.PostInsertEventListeners =
                new IPostInsertEventListener[] { this }
                    .Concat(cfg.EventListeners.PostInsertEventListeners)
                    .ToArray();
            cfg.EventListeners.PostDeleteEventListeners =
                new IPostDeleteEventListener[] { this }
                    .Concat(cfg.EventListeners.PostDeleteEventListeners)
                    .ToArray();
        }

        public async Task OnPostInsertAsync(PostInsertEvent @event, CancellationToken cancellationToken)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.CREATE) == AuditLogActionEnum.CREATE)
            {
                if (CanAuditLogFor(@event, auditLogItem, AuditLogActionEnum.CREATE))
                {
                    ICollection<TAuditEntity> auditLogs = GetAuditLogsFor(@event, auditLogItem);
                    await SaveAuditLogsAsync(@event, auditLogs, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public virtual void OnPostInsert(PostInsertEvent @event)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.CREATE) == AuditLogActionEnum.CREATE)
            {
                if (CanAuditLogFor(@event, auditLogItem, AuditLogActionEnum.CREATE))
                {
                    ICollection<TAuditEntity> auditLogs = GetAuditLogsFor(@event, auditLogItem);
                    SaveAuditLogs(@event, auditLogs);
                }
            }
        }

        public async Task OnPostUpdateAsync(PostUpdateEvent @event, CancellationToken cancellationToken)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.UPDATE) == AuditLogActionEnum.UPDATE)
            {
                if (CanAuditLogFor(@event, auditLogItem, AuditLogActionEnum.UPDATE))
                {
                    ICollection<TAuditEntity> auditLogs = GetAuditLogsFor(@event, auditLogItem);
                    await SaveAuditLogsAsync(@event, auditLogs, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public virtual void OnPostUpdate(PostUpdateEvent @event)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.UPDATE) == AuditLogActionEnum.UPDATE)
            {
                if (CanAuditLogFor(@event, auditLogItem, AuditLogActionEnum.UPDATE))
                {
                    ICollection<TAuditEntity> auditLogs = GetAuditLogsFor(@event, auditLogItem);
                    SaveAuditLogs(@event, auditLogs);
                }
            }
        }

        public async Task OnPostDeleteAsync(PostDeleteEvent @event, CancellationToken cancellationToken)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.DELETE) == AuditLogActionEnum.DELETE)
            {
                if (CanAuditLogFor(@event, auditLogItem, AuditLogActionEnum.DELETE))
                {
                    ICollection<TAuditEntity> auditLogs = GetAuditLogsFor(@event);
                    await SaveAuditLogsAsync(@event, auditLogs, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public virtual void OnPostDelete(PostDeleteEvent @event)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.DELETE) == AuditLogActionEnum.DELETE)
            {
                if (CanAuditLogFor(@event, auditLogItem, AuditLogActionEnum.DELETE))
                {
                    ICollection<TAuditEntity> auditLogs = GetAuditLogsFor(@event);
                    SaveAuditLogs(@event, auditLogs);
                }
            }
        }

        protected abstract bool CanAuditLogFor(AbstractEvent @event, AuditLogItem auditLogItem, AuditLogActionEnum requestedLogAction);

        protected virtual TAuditEntity CreateAuditEntity(string entryType, string entityName, string entityId, PpwAuditLog old, PpwAuditLog @new)
        {
            return
                new TAuditEntity
                {
                    EntryType = entryType,
                    EntityName = entityName,
                    EntityId = entityId,
                    PropertyName = @new?.PropertyName ?? old?.PropertyName,
                    OldValue = old?.Value,
                    NewValue = @new?.Value,
                    CreatedBy = IdentityProvider.IdentityName,
                    CreatedAt = UseUtc ? TimeProvider.UtcNow : TimeProvider.Now
                };
        }

        protected virtual ICollection<TAuditEntity> GetAuditLogsFor(PostInsertEvent @event, AuditLogItem auditLogItem)
        {
            string entityName = @event.Entity.GetType().Name;
            string entityId = @event.Id.ToString();

            List<TAuditEntity> auditEntities = new List<TAuditEntity>();
            int length = @event.State.Length;
            for (int fieldIndex = 0; fieldIndex < length; fieldIndex++)
            {
                string propertyName = @event.Persister.PropertyNames[fieldIndex];
                AuditLogActionEnum auditLogAction;
                if (auditLogItem.Properties.TryGetValue(propertyName, out auditLogAction))
                {
                    if ((auditLogAction & AuditLogActionEnum.CREATE) == AuditLogActionEnum.NONE)
                    {
                        PpwAuditLog[] auditLogs =
                            GetValuesFromStateArray(entityName, entityId, propertyName, @event.State, fieldIndex)
                                .Where(l => l != null)
                                .ToArray();
                        auditEntities.AddRange(auditLogs.Select(auditLog => CreateAuditEntity("I", entityName, entityId, null, auditLog)));
                    }
                }
            }

            return auditEntities;
        }

        protected virtual ICollection<TAuditEntity> GetAuditLogsFor(PostUpdateEvent @event, AuditLogItem auditLogItem)
        {
            string entityName = @event.Entity.GetType().Name;
            string entityId = @event.Id.ToString();

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
                string dirtyPropertyName = @event.Persister.PropertyNames[dirtyFieldIndex];
                AuditLogActionEnum auditLogAction;
                if (auditLogItem.Properties.TryGetValue(dirtyPropertyName, out auditLogAction))
                {
                    if ((auditLogAction & AuditLogActionEnum.UPDATE) == AuditLogActionEnum.NONE)
                    {
                        Dictionary<string, PpwAuditLog> oldAuditLogs =
                            GetValuesFromStateArray(entityName, entityId, dirtyPropertyName, @event.OldState, dirtyFieldIndex)
                                .Where(l => l != null)
                                .ToDictionary(l => l.PropertyName);
                        Dictionary<string, PpwAuditLog> newAuditLogs =
                            GetValuesFromStateArray(entityName, entityId, dirtyPropertyName, @event.State, dirtyFieldIndex)
                                .Where(l => l != null)
                                .ToDictionary(l => l.PropertyName);

                        HashSet<string> propertyNames =
                            new HashSet<string>(
                                oldAuditLogs
                                    .Select(al => al.Key)
                                    .Union(newAuditLogs.Select(al => al.Key)));

                        foreach (string propertyName in propertyNames)
                        {
                            PpwAuditLog oldAuditLog;
                            oldAuditLogs.TryGetValue(propertyName, out oldAuditLog);

                            PpwAuditLog newAuditLog;
                            newAuditLogs.TryGetValue(propertyName, out newAuditLog);

                            if (oldAuditLog?.Value != newAuditLog?.Value)
                            {
                                auditLogs.Add(CreateAuditEntity("U", entityName, entityId, oldAuditLog, newAuditLog));
                            }
                        }
                    }
                }
            }

            return auditLogs;
        }

        protected virtual ICollection<TAuditEntity> GetAuditLogsFor(PostDeleteEvent @event)
        {
            string entityName = @event.Entity.GetType().Name;
            string entityId = @event.Id.ToString();

            List<TAuditEntity> auditLogs =
                new List<TAuditEntity>
                {
                    CreateAuditEntity("D", entityName, entityId, null, null)
                };

            return auditLogs;
        }

        protected virtual void SaveAuditLogs(AbstractEvent @event, ICollection<TAuditEntity> auditLogs)
        {
            if (auditLogs.Count > 0)
            {
                using (ISession session = @event.Session.SessionWithOptions().Connection().OpenSession())
                {
                    foreach (TAuditEntity auditLog in auditLogs)
                    {
                        session.Save(auditLog);
                    }

                    session.Flush();
                }
            }
        }

        protected virtual async Task SaveAuditLogsAsync(AbstractEvent @event, ICollection<TAuditEntity> auditLogs, CancellationToken cancellationToken)
        {
            if (auditLogs.Count > 0)
            {
                using (ISession session = @event.Session.SessionWithOptions().Connection().OpenSession())
                {
                    foreach (TAuditEntity auditLog in auditLogs)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await session.SaveAsync(auditLog, cancellationToken).ConfigureAwait(false);
                    }

                    await session.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        protected virtual IEnumerable<PpwAuditLog> GetValuesFromStateArray(string entityName, string entityId, string propertyName, object[] stateArray, int position)
        {
            object value = stateArray[position];
            if (value != null)
            {
                IPersistentObject<TId> persistentObject = value as IPersistentObject<TId>;
                if (persistentObject != null)
                {
                    yield return new PpwAuditLog(propertyName, persistentObject.Id.ToString());
                }
                else
                {
                    IPpwAuditLog ppwAuditLog = value as IPpwAuditLog;
                    if (ppwAuditLog != null)
                    {
                        if (ppwAuditLog.IsMultiLog)
                        {
                            foreach (PpwAuditLog auditLog in ppwAuditLog.GetMultiLogs(propertyName))
                            {
                                yield return auditLog;
                            }
                        }
                        else
                        {
                            yield return ppwAuditLog.GetSingleLog(propertyName);
                        }
                    }
                    else
                    {
                        yield return CreatePpwAuditLog(entityName, entityId, propertyName, value);
                    }
                }
            }
            else
            {
                yield return new PpwAuditLog(propertyName, null);
            }
        }

        protected virtual PpwAuditLog CreatePpwAuditLog(string entityName, string entityId, string propertyName, object value) 
            => new PpwAuditLog(propertyName, value.ToString());

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
                        type =>
                        {
                            result = new AuditLogItem();
                            if (type != typeof(TAuditEntity))
                            {
                                AuditLogAttribute auditLogAttribute =
                                    type
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
