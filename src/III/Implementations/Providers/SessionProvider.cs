// Copyright 2018 by PeopleWare n.v..
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

using JetBrains.Annotations;

using NHibernate;

namespace PPWCode.Vernacular.NHibernate.III.Providers
{
    /// <inheritdoc />
    public class SessionProvider : ISessionProvider
    {
        public SessionProvider(
            [NotNull] ISession session,
            [NotNull] ITransactionProvider transactionProvider,
            [NotNull] ISafeEnvironmentProvider safeEnvironmentProvider,
            IsolationLevel isolationLevel)
        {
            Session = session;
            TransactionProvider = transactionProvider;
            SafeEnvironmentProvider = safeEnvironmentProvider;
            IsolationLevel = isolationLevel;
        }

        /// <inheritdoc />
        public ISession Session { get; }

        /// <inheritdoc />
        public ITransactionProvider TransactionProvider { get; }

        /// <inheritdoc />
        public ISafeEnvironmentProvider SafeEnvironmentProvider { get; }

        /// <inheritdoc />
        public IsolationLevel IsolationLevel { get; }

        /// <inheritdoc />
        public void Flush()
            => TransactionProvider.Run(Session, IsolationLevel, () => SafeEnvironmentProvider.Run(nameof(Flush), () => Session.Flush()));
    }
}
