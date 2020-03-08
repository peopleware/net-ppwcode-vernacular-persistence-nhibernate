// Copyright 2017 by PeopleWare n.v..
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using PPWCode.Vernacular.Exceptions.IV;
using PPWCode.Vernacular.NHibernate.III.Async.Interfaces;
using PPWCode.Vernacular.NHibernate.III.Async.Interfaces.Providers;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Async.Implementations
{
    /// <inheritdoc cref="IRepositoryAsync{TRoot,TId}" />
    public abstract class RepositoryAsync<TRoot, TId>
        : RepositoryBase<TRoot, TId>,
          IRepositoryAsync<TRoot, TId>
        where TRoot : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        protected RepositoryAsync([NotNull] ISessionProviderAsync sessionProviderAsync)
            : base(sessionProviderAsync)
        {
            SessionProviderAsync = sessionProviderAsync;
        }

        [NotNull]
        public ISessionProviderAsync SessionProviderAsync { get; }

        [NotNull]
        protected ITransactionProviderAsync TransactionProviderAsync
            => SessionProviderAsync.TransactionProviderAsync;

        [NotNull]
        protected ISafeEnvironmentProviderAsync SafeEnvironmentProviderAsync
            => SessionProviderAsync.SafeEnvironmentProviderAsync;

        /// <inheritdoc />
        public async Task<TRoot> GetByIdAsync(TId id, CancellationToken cancellationToken)
            => await ExecuteAsync(nameof(GetByIdAsync), can => GetByIdInternalAsync(id, can), cancellationToken).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task<TRoot> LoadByIdAsync(TId id, CancellationToken cancellationToken)
            => await ExecuteAsync(nameof(LoadByIdAsync), can => LoadByIdInternalAsync(id, can), cancellationToken).ConfigureAwait(false)
               ?? throw new ProgrammingError($"Should never happen, implementation of {nameof(LoadByIdInternalAsync)} should always return a instance of type {typeof(TRoot).FullName}");

        /// <inheritdoc />
        public async Task<IList<TRoot>> FindAllAsync(CancellationToken cancellationToken)
            => await ExecuteAsync(nameof(FindAllAsync), FindAllInternalAsync, cancellationToken).ConfigureAwait(false)
               ?? throw new ProgrammingError("Should never happen, implementation of {nameof(FindAllInternalAsync)} should always return a IList<TRoot>, where TRoot is of type {typeof(TRoot).FullName}");

        /// <inheritdoc />
        public async Task<IList<TRoot>> FindByIdsAsync(IEnumerable<TId> ids, CancellationToken cancellationToken)
        {
            async Task<IList<TRoot>> WrapperAsync(CancellationToken can)
            {
                List<TRoot> result = new List<TRoot>();
                foreach (TId[] segment in GetSegmentedIds(ids).Where(s => s.Length > 0))
                {
                    result.AddRange(await FindByIdsInternalAsync(segment, cancellationToken).ConfigureAwait(false));
                }

                return result;
            }

            return
                await ExecuteAsync(nameof(FindByIdsAsync), WrapperAsync, cancellationToken).ConfigureAwait(false)
                ?? new List<TRoot>();
        }

        /// <inheritdoc />
        public async Task<TRoot> MergeAsync(TRoot entity, CancellationToken cancellationToken)
            => await ExecuteAsync(nameof(MergeAsync), can => MergeInternalAsync(entity, can), cancellationToken).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task SaveOrUpdateAsync(TRoot entity, CancellationToken cancellationToken)
            => await ExecuteAsync(nameof(SaveOrUpdateAsync), can => SaveOrUpdateInternalAsync(entity, can), cancellationToken).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task DeleteAsync(TRoot entity, CancellationToken cancellationToken)
            => await ExecuteAsync(nameof(DeleteAsync), can => DeleteInternalAsync(entity, can), cancellationToken).ConfigureAwait(false);

        [NotNull]
        [ItemCanBeNull]
        protected virtual async Task<TResult> ExecuteAsync<TResult>(
            [NotNull] string requestDescription,
            [NotNull] Func<CancellationToken, Task<TResult>> lambda,
            CancellationToken cancellationToken)
            => await ExecuteAsync(requestDescription, lambda, null, cancellationToken).ConfigureAwait(false);

        [NotNull]
        [ItemCanBeNull]
        protected virtual async Task<TResult> ExecuteAsync<TResult>(
            [NotNull] string requestDescription,
            [NotNull] Func<CancellationToken, Task<TResult>> lambda,
            [CanBeNull] TRoot entity,
            CancellationToken cancellationToken)
        {
            async Task<TResult> SafeAsync(CancellationToken can)
                => await SafeEnvironmentProviderAsync.RunAsync<TRoot, TId, TResult>(requestDescription, lambda, entity, cancellationToken).ConfigureAwait(false);

            return await TransactionProviderAsync.RunAsync(Session, IsolationLevel, SafeAsync, cancellationToken).ConfigureAwait(false);
        }

        [NotNull]
        protected virtual async Task ExecuteAsync(
            [NotNull] string requestDescription,
            [NotNull] Func<CancellationToken, Task> lambda,
            CancellationToken cancellationToken)
            => await ExecuteAsync(requestDescription, lambda, null, cancellationToken).ConfigureAwait(false);

        [NotNull]
        protected virtual async Task ExecuteAsync(
            [NotNull] string requestDescription,
            [NotNull] Func<CancellationToken, Task> lambda,
            [CanBeNull] TRoot entity,
            CancellationToken cancellationToken)
        {
            async Task SafeAsync(CancellationToken can)
                => await SafeEnvironmentProviderAsync.RunAsync<TRoot, TId>(requestDescription, lambda, entity, cancellationToken).ConfigureAwait(false);

            await TransactionProviderAsync.RunAsync(Session, IsolationLevel, SafeAsync, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc cref="GetByIdAsync" />
        [NotNull]
        [ItemCanBeNull]
        protected virtual async Task<TRoot> GetByIdInternalAsync([NotNull] TId id, CancellationToken cancellationToken)
            => await Session.GetAsync<TRoot>(id, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc cref="LoadByIdAsync" />
        [NotNull]
        [ItemNotNull]
        protected virtual async Task<TRoot> LoadByIdInternalAsync([NotNull] TId id, CancellationToken cancellationToken)
            => await Session.LoadAsync<TRoot>(id, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc cref="FindAllAsync" />
        [NotNull]
        [ItemNotNull]
        protected abstract Task<IList<TRoot>> FindAllInternalAsync(CancellationToken cancellationToken);

        /// <inheritdoc cref="FindByIdsAsync" />
        [NotNull]
        [ItemNotNull]
        protected abstract Task<IList<TRoot>> FindByIdsInternalAsync([NotNull] [ItemNotNull] TId[] segment, CancellationToken cancellationToken);

        /// <inheritdoc cref="MergeAsync" />
        [NotNull]
        [ItemCanBeNull]
        protected virtual async Task<TRoot> MergeInternalAsync([CanBeNull] TRoot entity, CancellationToken cancellationToken)
        {
            if (entity != null)
            {
                // Note: Prevent a CREATE for something that was assumed to be an UPDATE.
                // NHibernate MERGE transforms an UPDATE for a not-found-PK into a CREATE
                if (!entity.IsTransient)
                {
                    TRoot foundEntity =
                        await GetByIdInternalAsync(entity.Id, cancellationToken)
                            .ConfigureAwait(false);
                    if (foundEntity == null)
                    {
                        throw new NotFoundException("Merge executed for an entity that no longer exists in the database.");
                    }
                }

                return await Session.MergeAsync(entity, cancellationToken).ConfigureAwait(false);
            }

            return default;
        }

        /// <inheritdoc cref="SaveOrUpdateAsync" />
        [NotNull]
        protected virtual async Task SaveOrUpdateInternalAsync([CanBeNull] TRoot entity, CancellationToken cancellationToken)
        {
            if (entity != null)
            {
                // Note: Prevent a CREATE for something that was assumed to be an UPDATE.
                if (!entity.IsTransient)
                {
                    TRoot foundEntity =
                        await GetByIdInternalAsync(entity.Id, cancellationToken)
                            .ConfigureAwait(false);
                    if (foundEntity == null)
                    {
                        throw new NotFoundException("SaveOrUpdate executed for an entity that no longer exists in the database.");
                    }
                }

                await Session.SaveOrUpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="DeleteAsync" />
        [NotNull]
        protected virtual async Task DeleteInternalAsync([CanBeNull] TRoot entity, CancellationToken cancellationToken)
        {
            if ((entity != null) && !entity.IsTransient)
            {
                // Check if entity exists
                TRoot fetchedEntity = await Session.GetAsync<TRoot>(entity.Id, cancellationToken).ConfigureAwait(false);
                if (fetchedEntity != null)
                {
                    // Handle stale objects
                    TRoot mergedEntity = await Session.MergeAsync(entity, cancellationToken).ConfigureAwait(false);

                    // finally delete none-transient not stale existing entity
                    await Session.DeleteAsync(mergedEntity, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
