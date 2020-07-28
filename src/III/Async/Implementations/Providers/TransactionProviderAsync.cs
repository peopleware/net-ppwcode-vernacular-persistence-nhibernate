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

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

using JetBrains.Annotations;

using NHibernate;

using PPWCode.Vernacular.NHibernate.III.Async.Interfaces.Providers;
using PPWCode.Vernacular.NHibernate.III.Providers;

using IsolationLevel = System.Data.IsolationLevel;

namespace PPWCode.Vernacular.NHibernate.III.Async.Implementations.Providers
{
    /// <inheritdoc cref="ITransactionProviderAsync" />
    [UsedImplicitly]
    public class TransactionProviderAsync
        : TransactionProvider,
          ITransactionProviderAsync
    {
        /// <inheritdoc />
        public Task RunAsync(
            ISession session,
            IsolationLevel isolationLevel,
            Func<CancellationToken, Task> lambda,
            CancellationToken cancellationToken)
        {
            async Task<int> WrapperFunc(CancellationToken can)
            {
                await lambda(can).ConfigureAwait(false);
                return default;
            }

            return RunAsync(session, isolationLevel, WrapperFunc, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TResult> RunAsync<TResult>(
            ISession session,
            IsolationLevel isolationLevel,
            Func<CancellationToken, Task<TResult>> lambda,
            CancellationToken cancellationToken)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (lambda == null)
            {
                throw new ArgumentNullException(nameof(lambda));
            }

            if ((session.GetCurrentTransaction()?.IsActive == true) || (Transaction.Current != null))
            {
                return await lambda(cancellationToken).ConfigureAwait(false);
            }

            TResult result;
            ITransaction transaction = session.BeginTransaction(isolationLevel);
            try
            {
                result = await lambda(cancellationToken).ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                throw;
            }
            finally
            {
                transaction.Dispose();
            }

            return result;
        }
    }
}
