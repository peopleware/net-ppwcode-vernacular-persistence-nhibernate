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
using System.Data;
using System.Linq;

using JetBrains.Annotations;

using NHibernate;

using PPWCode.Vernacular.NHibernate.III.Providers;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III
{
    public abstract class RepositoryBase<TRoot, TId>
        where TRoot : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        protected RepositoryBase([NotNull] ISessionProvider sessionProvider)
        {
            SessionProvider = sessionProvider ?? throw new ArgumentNullException(nameof(sessionProvider));
        }

        [NotNull]
        public ISessionProvider SessionProvider { get; }

        [NotNull]
        protected ISession Session
            => SessionProvider.Session;

        [NotNull]
        protected ITransactionProvider TransactionProvider
            => SessionProvider.TransactionProvider;

        [NotNull]
        protected ISafeEnvironmentProvider SafeEnvironmentProvider
            => SessionProvider.SafeEnvironmentProvider;

        protected IsolationLevel IsolationLevel
            => SessionProvider.IsolationLevel;

        protected virtual int SegmentedBatchSize
            => 320;

        protected virtual void Execute([NotNull] string requestDescription, [NotNull] Action action)
            => Execute(requestDescription, action, null);

        [CanBeNull]
        protected virtual TResult Execute<TResult>([NotNull] string requestDescription, [NotNull] Func<TResult> func, [CanBeNull] TRoot entity)
            => TransactionProvider.Run(Session, IsolationLevel, () => SafeEnvironmentProvider.Run<TRoot, TId, TResult>(requestDescription, func, entity));

        protected virtual void Execute([NotNull] string requestDescription, [NotNull] Action action, [CanBeNull] TRoot entity)
            => TransactionProvider.Run(Session, IsolationLevel, () => SafeEnvironmentProvider.Run<TRoot, TId>(requestDescription, action, entity));

        [CanBeNull]
        protected virtual TResult Execute<TResult>([NotNull] string requestDescription, [NotNull] Func<TResult> func)
            => Execute(requestDescription, func, null);

        [NotNull]
        protected virtual IEnumerable<TId[]> GetSegmentedIds([NotNull] IEnumerable<TId> ids)
        {
            ISet<TId> uniqueIds = new HashSet<TId>(ids);
            int count = uniqueIds.Count;
            int nrSegments = (count / SegmentedBatchSize) + (count % SegmentedBatchSize > 0 ? 1 : 0);
            return count == 0
                       ? Enumerable.Empty<TId[]>()
                       : uniqueIds
                           .OrderBy(id => id)
                           .Segment(nrSegments)
                           .Select(s => s.Select(o => o).ToArray());
        }
    }
}
