// Copyright 2018-2018 by PeopleWare n.v..
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

using System.Data;
using System.Threading;
using System.Threading.Tasks;

using NHibernate;

using PPWCode.Vernacular.NHibernate.I.Interfaces;

namespace PPWCode.Vernacular.NHibernate.I.Implementations.Providers
{
    public class SessionProvider : ISessionProvider
    {
        public SessionProvider(
            ISession session,
            ITransactionProvider transactionProvider,
            ISafeEnvironmentProvider safeEnvironmentProvider,
            IsolationLevel isolationLevel)
        {
            Session = session;
            TransactionProvider = transactionProvider;
            SafeEnvironmentProvider = safeEnvironmentProvider;
            IsolationLevel = isolationLevel;
        }

        public ISession Session { get; }
        public ITransactionProvider TransactionProvider { get; }
        public ISafeEnvironmentProvider SafeEnvironmentProvider { get; }
        public IsolationLevel IsolationLevel { get; }

        public void Flush()
        {
            TransactionProvider
                .Run(Session,
                     IsolationLevel,
                     () => SafeEnvironmentProvider
                         .Run(nameof(Flush), () => Session.Flush()));
        }

        public Task FlushAsync(CancellationToken cancellationToken)
        {
            return
                TransactionProvider
                    .Run(Session,
                         IsolationLevel,
                         () => SafeEnvironmentProvider
                             .Run(nameof(Flush), () => Session.FlushAsync(cancellationToken)));
        }
    }
}
