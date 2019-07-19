// Copyright 2019 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
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

using JetBrains.Annotations;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Event;

using PPWCode.Vernacular.Exceptions.IV;
using PPWCode.Vernacular.Persistence.IV;

using Environment = System.Environment;

namespace PPWCode.Vernacular.NHibernate.III
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
        private static readonly ConcurrentDictionary<Type, AuditLogItem> _domainTypes =
            new ConcurrentDictionary<Type, AuditLogItem>();

        protected AuditLogEventListener(
            [NotNull] IIdentityProvider identityProvider,
            [NotNull] ITimeProvider timeProvider,
            bool useUtc)
        {
            IdentityProvider = identityProvider;
            TimeProvider = timeProvider;
            UseUtc = useUtc;
        }

        [NotNull]
        public IIdentityProvider IdentityProvider { get; }

        [NotNull]
        public ITimeProvider TimeProvider { get; }

        public bool UseUtc { get; }

        [NotNull]
        public async Task OnPostDeleteAsync([NotNull] PostDeleteEvent @event, CancellationToken cancellationToken)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.DELETE) == AuditLogActionEnum.DELETE)
            {
                if (CanAuditLogFor(@event, auditLogItem, AuditLogActionEnum.DELETE))
                {
                    object context = null;
                    ICollection<TAuditEntity> auditLogs = GetAuditLogsFor(@event, ref context);
                    await SaveAuditLogsAsync(@event, auditLogs, context, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public virtual void OnPostDelete([NotNull] PostDeleteEvent @event)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.DELETE) == AuditLogActionEnum.DELETE)
            {
                if (CanAuditLogFor(@event, auditLogItem, AuditLogActionEnum.DELETE))
                {
                    object context = null;
                    ICollection<TAuditEntity> auditLogs = GetAuditLogsFor(@event, ref context);
                    SaveAuditLogs(@event, auditLogs, context);
                }
            }
        }

        [NotNull]
        public async Task OnPostInsertAsync([NotNull] PostInsertEvent @event, CancellationToken cancellationToken)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.CREATE) == AuditLogActionEnum.CREATE)
            {
                if (CanAuditLogFor(@event, auditLogItem, AuditLogActionEnum.CREATE))
                {
                    object context = null;
                    ICollection<TAuditEntity> auditLogs = GetAuditLogsFor(@event, auditLogItem, ref context);
                    await SaveAuditLogsAsync(@event, auditLogs, context, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public virtual void OnPostInsert([NotNull] PostInsertEvent @event)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.CREATE) == AuditLogActionEnum.CREATE)
            {
                if (CanAuditLogFor(@event, auditLogItem, AuditLogActionEnum.CREATE))
                {
                    object context = null;
                    ICollection<TAuditEntity> auditLogs = GetAuditLogsFor(@event, auditLogItem, ref context);
                    SaveAuditLogs(@event, auditLogs, context);
                }
            }
        }

        [NotNull]
        public async Task OnPostUpdateAsync([NotNull] PostUpdateEvent @event, CancellationToken cancellationToken)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.UPDATE) == AuditLogActionEnum.UPDATE)
            {
                if (CanAuditLogFor(@event, auditLogItem, AuditLogActionEnum.UPDATE))
                {
                    object context = null;
                    ICollection<TAuditEntity> auditLogs = GetAuditLogsFor(@event, auditLogItem, ref context);
                    await SaveAuditLogsAsync(@event, auditLogs, context, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public virtual void OnPostUpdate([NotNull] PostUpdateEvent @event)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.UPDATE) == AuditLogActionEnum.UPDATE)
            {
                if (CanAuditLogFor(@event, auditLogItem, AuditLogActionEnum.UPDATE))
                {
                    object context = null;
                    ICollection<TAuditEntity> auditLogs = GetAuditLogsFor(@event, auditLogItem, ref context);
                    SaveAuditLogs(@event, auditLogs, context);
                }
            }
        }

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

        protected abstract bool CanAuditLogFor(
            [NotNull] AbstractEvent @event,
            [NotNull] AuditLogItem auditLogItem,
            AuditLogActionEnum requestedLogAction);

        [NotNull]
        protected virtual TAuditEntity CreateAuditEntity(
            [NotNull] string entryType,
            [NotNull] string entityName,
            [NotNull] string entityId,
            [CanBeNull] PpwAuditLog old,
            [CanBeNull] PpwAuditLog @new)
            => new TAuditEntity
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

        [NotNull]
        protected virtual ICollection<TAuditEntity> GetAuditLogsFor([NotNull] PostInsertEvent @event, AuditLogItem auditLogItem, ref object context)
        {
            string entityName = @event.Entity.GetType().Name;
            string entityId = @event.Id.ToString();

            List<TAuditEntity> auditEntities = new List<TAuditEntity>();
            int length = @event.State.Length;
            for (int fieldIndex = 0; fieldIndex < length; fieldIndex++)
            {
                string propertyName = @event.Persister.PropertyNames[fieldIndex];
                if (auditLogItem.Properties.TryGetValue(propertyName, out AuditLogActionEnum auditLogAction))
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

        [NotNull]
        protected virtual ICollection<TAuditEntity> GetAuditLogsFor([NotNull] PostUpdateEvent @event, AuditLogItem auditLogItem, ref object context)
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
                if (auditLogItem.Properties.TryGetValue(dirtyPropertyName, out AuditLogActionEnum auditLogAction))
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
                            oldAuditLogs.TryGetValue(propertyName, out PpwAuditLog oldAuditLog);
                            newAuditLogs.TryGetValue(propertyName, out PpwAuditLog newAuditLog);
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

        [NotNull]
        protected virtual ICollection<TAuditEntity> GetAuditLogsFor([NotNull] PostDeleteEvent @event, ref object context)
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

        protected virtual void SaveAuditLogs([NotNull] AbstractEvent @event, [NotNull] ICollection<TAuditEntity> auditLogs, object context)
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

        [NotNull]
        protected virtual async Task SaveAuditLogsAsync(
            [NotNull] AbstractEvent @event,
            [NotNull] ICollection<TAuditEntity> auditLogs,
            object context,
            CancellationToken cancellationToken)
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

        [NotNull]
        protected virtual IEnumerable<PpwAuditLog> GetValuesFromStateArray(
            [NotNull] string entityName,
            [NotNull] string entityId,
            [NotNull] string propertyName,
            [NotNull] object[] stateArray,
            int position)
        {
            object value = stateArray[position];
            if (value != null)
            {
                if (value is IPersistentObject<TId> persistentObject)
                {
                    yield return new PpwAuditLog(propertyName, persistentObject.Id.ToString());
                }
                else
                {
                    if (value is IPpwAuditLog ppwAuditLog)
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

        [NotNull]
        protected virtual PpwAuditLog CreatePpwAuditLog(
            [NotNull] string entityName,
            [NotNull] string entityId,
            [NotNull] string propertyName,
            [NotNull] object value)
            => value is DateTime dt
                   ? new PpwAuditLog(propertyName, dt.ToString("u"))
                   : new PpwAuditLog(propertyName, value.ToString());

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
                AuditLogItem result =
                    _domainTypes
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
                                                .Add(propertyInfo.Name, auditLogPropertyIgnore?.AuditLogAction ?? AuditLogActionEnum.NONE);
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
