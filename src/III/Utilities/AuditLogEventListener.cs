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
using NHibernate.Type;

using PPWCode.Vernacular.Exceptions.IV;
using PPWCode.Vernacular.Persistence.IV;

using Environment = System.Environment;

namespace PPWCode.Vernacular.NHibernate.III
{
    /// <inheritdoc cref="IRegisterEventListener" />
    /// <inheritdoc cref="IPostUpdateEventListener" />
    /// <inheritdoc cref="IPostInsertEventListener" />
    /// <inheritdoc cref="IPostDeleteEventListener" />
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Castle Windsor usage")]
    [Serializable]
    public abstract class AuditLogEventListener<TId, TAuditEntity, TContext>
        : IRegisterEventListener,
          IPostUpdateEventListener,
          IPostInsertEventListener,
          IPostDeleteEventListener
        where TId : IEquatable<TId>
        where TAuditEntity : AuditLog<TId>, new()
        where TContext : AuditLogEventContext
    {
        private static readonly ConcurrentDictionary<Type, AuditLogItem> _domainTypes =
            new ConcurrentDictionary<Type, AuditLogItem>();

        protected AuditLogEventListener(
            [JetBrains.Annotations.NotNull] IIdentityProvider identityProvider,
            [JetBrains.Annotations.NotNull] ITimeProvider timeProvider,
            bool useUtc)
        {
            IdentityProvider = identityProvider;
            TimeProvider = timeProvider;
            UseUtc = useUtc;
        }

        /// <inheritdoc cref="IIdentityProvider" />
        [JetBrains.Annotations.NotNull]
        public IIdentityProvider IdentityProvider { get; }

        /// <inheritdoc cref="ITimeProvider" />
        [JetBrains.Annotations.NotNull]
        public ITimeProvider TimeProvider { get; }

        public bool UseUtc { get; }

        /// <inheritdoc cref="IPostDeleteEventListener.OnPostDeleteAsync" />
        [JetBrains.Annotations.NotNull]
        public async Task OnPostDeleteAsync([JetBrains.Annotations.NotNull] PostDeleteEvent @event, CancellationToken cancellationToken)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.DELETE) == AuditLogActionEnum.DELETE)
            {
                if (CanAuditLogFor(@event, auditLogItem, AuditLogActionEnum.DELETE))
                {
                    TContext context = CreateContext(@event);
                    ICollection<TAuditEntity> auditLogs = GetAuditLogsFor(@event, context);
                    await SaveAuditLogsAsync(@event, auditLogs, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc cref="IPostDeleteEventListener.OnPostDelete" />
        public virtual void OnPostDelete([JetBrains.Annotations.NotNull] PostDeleteEvent @event)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.DELETE) == AuditLogActionEnum.DELETE)
            {
                if (CanAuditLogFor(@event, auditLogItem, AuditLogActionEnum.DELETE))
                {
                    TContext context = CreateContext(@event);
                    ICollection<TAuditEntity> auditLogs = GetAuditLogsFor(@event, context);
                    SaveAuditLogs(@event, auditLogs);
                }
            }
        }

        /// <inheritdoc cref="IPostInsertEventListener.OnPostInsertAsync" />
        [JetBrains.Annotations.NotNull]
        public async Task OnPostInsertAsync([JetBrains.Annotations.NotNull] PostInsertEvent @event, CancellationToken cancellationToken)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.CREATE) == AuditLogActionEnum.CREATE)
            {
                if (CanAuditLogFor(@event, auditLogItem, AuditLogActionEnum.CREATE))
                {
                    TContext context = CreateContext(@event);
                    ICollection<TAuditEntity> auditLogs = GetAuditLogsFor(@event, auditLogItem, context);
                    await SaveAuditLogsAsync(@event, auditLogs, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc cref="IPostInsertEventListener.OnPostInsert" />
        public virtual void OnPostInsert([JetBrains.Annotations.NotNull] PostInsertEvent @event)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.CREATE) == AuditLogActionEnum.CREATE)
            {
                if (CanAuditLogFor(@event, auditLogItem, AuditLogActionEnum.CREATE))
                {
                    TContext context = CreateContext(@event);
                    ICollection<TAuditEntity> auditLogs = GetAuditLogsFor(@event, auditLogItem, context);
                    SaveAuditLogs(@event, auditLogs);
                }
            }
        }

        /// <inheritdoc cref="IPostUpdateEventListener.OnPostUpdateAsync" />
        [JetBrains.Annotations.NotNull]
        public async Task OnPostUpdateAsync([JetBrains.Annotations.NotNull] PostUpdateEvent @event, CancellationToken cancellationToken)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.UPDATE) == AuditLogActionEnum.UPDATE)
            {
                if (CanAuditLogFor(@event, auditLogItem, AuditLogActionEnum.UPDATE))
                {
                    TContext context = CreateContext(@event);
                    ICollection<TAuditEntity> auditLogs = GetAuditLogsFor(@event, auditLogItem, context);
                    await SaveAuditLogsAsync(@event, auditLogs, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc cref="IPostUpdateEventListener.OnPostUpdate" />
        public virtual void OnPostUpdate([JetBrains.Annotations.NotNull] PostUpdateEvent @event)
        {
            AuditLogItem auditLogItem = AuditLogItem.Find(@event.Entity.GetType());
            if ((auditLogItem.AuditLogAction & AuditLogActionEnum.UPDATE) == AuditLogActionEnum.UPDATE)
            {
                if (CanAuditLogFor(@event, auditLogItem, AuditLogActionEnum.UPDATE))
                {
                    TContext context = CreateContext(@event);
                    ICollection<TAuditEntity> auditLogs = GetAuditLogsFor(@event, auditLogItem, context);
                    SaveAuditLogs(@event, auditLogs);
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
            [JetBrains.Annotations.NotNull] AbstractEvent @event,
            [JetBrains.Annotations.NotNull] AuditLogItem auditLogItem,
            AuditLogActionEnum requestedLogAction);

        protected abstract TContext CreateContext(IPostDatabaseOperationEventArgs postDatabaseOperationEventArgs);

        protected abstract void OnAddAuditEntities([JetBrains.Annotations.NotNull] TContext context);

        [JetBrains.Annotations.NotNull]
        protected virtual TAuditEntity CreateAuditEntity(
            [JetBrains.Annotations.NotNull] string entryType,
            [JetBrains.Annotations.NotNull] string entityName,
            [JetBrains.Annotations.NotNull] string entityId,
            [JetBrains.Annotations.NotNull] TContext context,
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

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        protected virtual ICollection<TAuditEntity> GetAuditLogsFor(
            [JetBrains.Annotations.NotNull] PostInsertEvent @event,
            [JetBrains.Annotations.NotNull] AuditLogItem auditLogItem,
            [JetBrains.Annotations.NotNull] TContext context)
        {
            string entityName = @event.Entity.GetType().Name;
            string entityId = @event.Id.ToString();

            List<PpwAuditLog> auditLogs = new List<PpwAuditLog>();
            int length = @event.State.Length;
            for (int fieldIndex = 0; fieldIndex < length; fieldIndex++)
            {
                string propertyName = @event.Persister.PropertyNames[fieldIndex];
                if ((auditLogItem.Properties != null)
                    && auditLogItem.Properties.TryGetValue(propertyName, out AuditLogActionEnum auditLogAction))
                {
                    if ((auditLogAction & AuditLogActionEnum.CREATE) == AuditLogActionEnum.NONE)
                    {
                        object value = @event.State[fieldIndex];
                        if (value != null)
                        {
                            IType valueNHibernateType = @event.Persister.PropertyTypes[fieldIndex];
                            auditLogs.AddRange(CreatePpwAuditLogs(propertyName, value, valueNHibernateType, context));
                        }
                    }
                }
            }

            List<TAuditEntity> auditEntities = new List<TAuditEntity>();
            if (auditLogs.Count > 0)
            {
                OnAddAuditEntities(context);
                auditEntities.AddRange(
                    auditLogs
                        .Select(auditLog => CreateAuditEntity("I", entityName, entityId, context, null, auditLog))
                        .Where(ae => ae.NewValue != null));
            }

            return auditEntities;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        protected virtual ICollection<TAuditEntity> GetAuditLogsFor(
            [JetBrains.Annotations.NotNull] PostUpdateEvent @event,
            [JetBrains.Annotations.NotNull] AuditLogItem auditLogItem,
            [JetBrains.Annotations.NotNull] TContext context)
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

            List<PpwAuditLogPair> auditLogPairs = new List<PpwAuditLogPair>();
            int[] fieldIndices = @event.Persister.FindDirty(@event.State, @event.OldState, @event.Entity, @event.Session);
            if (fieldIndices != null)
            {
                foreach (int dirtyFieldIndex in fieldIndices)
                {
                    string dirtyPropertyName = @event.Persister.PropertyNames[dirtyFieldIndex];
                    if ((auditLogItem.Properties != null)
                        && auditLogItem.Properties.TryGetValue(dirtyPropertyName, out AuditLogActionEnum auditLogAction))
                    {
                        if ((auditLogAction & AuditLogActionEnum.UPDATE) == AuditLogActionEnum.NONE)
                        {
                            object oldValue = @event.OldState[dirtyFieldIndex];
                            IType valueNHibernateType = @event.Persister.PropertyTypes[dirtyFieldIndex];
                            object newValue = @event.State[dirtyFieldIndex];
                            IDictionary<string, PpwAuditLog> oldAuditLogs =
                                CreatePpwAuditLogs(dirtyPropertyName, oldValue, valueNHibernateType, context)
                                    .ToDictionary(l => l.PropertyName);
                            IDictionary<string, PpwAuditLog> newAuditLogs =
                                CreatePpwAuditLogs(dirtyPropertyName, newValue, valueNHibernateType, context)
                                    .ToDictionary(l => l.PropertyName);

                            ISet<string> propertyNames =
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
                                    auditLogPairs.Add(new PpwAuditLogPair(oldAuditLog, newAuditLog));
                                }
                            }
                        }
                    }
                }
            }

            List<TAuditEntity> auditLogs = new List<TAuditEntity>();
            if (auditLogPairs.Count > 0)
            {
                OnAddAuditEntities(context);
                auditLogs.AddRange(auditLogPairs.Select(ap => CreateAuditEntity("U", entityName, entityId, context, ap.Old, ap.New)));
            }

            return auditLogs;
        }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        protected virtual ICollection<TAuditEntity> GetAuditLogsFor(
            [JetBrains.Annotations.NotNull] PostDeleteEvent @event,
            [JetBrains.Annotations.NotNull] TContext context)
        {
            string entityName = @event.Entity.GetType().Name;
            string entityId = @event.Id.ToString();

            OnAddAuditEntities(context);
            List<TAuditEntity> auditLogs =
                new List<TAuditEntity>
                {
                    CreateAuditEntity("D", entityName, entityId, context, null, null)
                };

            return auditLogs;
        }

        protected virtual void SaveAuditLogs(
            [JetBrains.Annotations.NotNull] AbstractEvent @event,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ICollection<TAuditEntity> auditLogs)
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

        [JetBrains.Annotations.NotNull]
        protected virtual async Task SaveAuditLogsAsync(
            [JetBrains.Annotations.NotNull] AbstractEvent @event,
            [JetBrains.Annotations.NotNull] [ItemNotNull] ICollection<TAuditEntity> auditLogs,
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

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        protected virtual IEnumerable<PpwAuditLog> CreatePpwAuditLogs(
            [JetBrains.Annotations.NotNull] string propertyName,
            [CanBeNull] object value,
            [JetBrains.Annotations.NotNull] IType valueNHibernateType,
            [JetBrains.Annotations.NotNull] TContext context)
        {
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
                        yield return CreatePpwAuditLog(propertyName, value, valueNHibernateType, context);
                    }
                }
            }
            else
            {
                yield return new PpwAuditLog(propertyName, null);
            }
        }

        [JetBrains.Annotations.NotNull]
        protected virtual PpwAuditLog CreatePpwAuditLog(
            [JetBrains.Annotations.NotNull] string propertyName,
            [JetBrains.Annotations.NotNull] object value,
            [JetBrains.Annotations.NotNull] IType valueNhibernateType,
            [JetBrains.Annotations.NotNull] TContext context)
            => value is DateTime dateTime
                   ? valueNhibernateType is DateType
                         ? new PpwAuditLog(propertyName, dateTime.ToString("yyyy-MM-dd"))
                         : new PpwAuditLog(propertyName, dateTime.ToString("o"))
                   : new PpwAuditLog(propertyName, value.ToString());

        protected class AuditLogItem
        {
            private AuditLogItem()
            {
                AuditLogAction = AuditLogActionEnum.NONE;
                Properties = null;
            }

            public AuditLogActionEnum AuditLogAction { get; private set; }

            [CanBeNull]
            public IDictionary<string, AuditLogActionEnum> Properties { get; private set; }

            [JetBrains.Annotations.NotNull]
            public static AuditLogItem Find([JetBrains.Annotations.NotNull] Type t)
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

        private class PpwAuditLogPair
        {
            public PpwAuditLogPair(
                [CanBeNull] PpwAuditLog old,
                [CanBeNull] PpwAuditLog @new)
            {
                Old = old;
                New = @new;
            }

            [CanBeNull]
            public PpwAuditLog Old { get; }

            [CanBeNull]
            public PpwAuditLog New { get; }
        }
    }
}
