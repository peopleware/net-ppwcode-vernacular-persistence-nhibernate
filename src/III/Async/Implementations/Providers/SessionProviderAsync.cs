// Copyright  by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Data;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using NHibernate;

using PPWCode.Vernacular.NHibernate.III.Async.Interfaces.Providers;
using PPWCode.Vernacular.NHibernate.III.Providers;

namespace PPWCode.Vernacular.NHibernate.III.Async.Implementations.Providers
{
    /// <inheritdoc cref="ISessionProviderAsync" />
    [UsedImplicitly]
    public class SessionProviderAsync
        : SessionProvider,
          ISessionProviderAsync
    {
        public SessionProviderAsync(
            [NotNull] ISession session,
            [NotNull] ITransactionProviderAsync transactionProviderAsync,
            [NotNull] ISafeEnvironmentProviderAsync safeEnvironmentProviderAsync,
            IsolationLevel isolationLevel)
            : base(session, transactionProviderAsync, safeEnvironmentProviderAsync, isolationLevel)
        {
            TransactionProviderAsync = transactionProviderAsync;
            SafeEnvironmentProviderAsync = safeEnvironmentProviderAsync;
        }

        public ITransactionProviderAsync TransactionProviderAsync { get; }
        public ISafeEnvironmentProviderAsync SafeEnvironmentProviderAsync { get; }

        public async Task FlushAsync(CancellationToken cancellationToken)
        {
            async Task NHibernateFlushAsync(CancellationToken can)
                => await Session.FlushAsync(can).ConfigureAwait(false);

            async Task SafeFlushAsync(CancellationToken can)
                => await SafeEnvironmentProviderAsync.RunAsync(nameof(FlushAsync), NHibernateFlushAsync, can).ConfigureAwait(false);

            await TransactionProviderAsync.RunAsync(Session, IsolationLevel, SafeFlushAsync, cancellationToken).ConfigureAwait(false);
        }
    }
}
