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
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using NHibernate;

using PPWCode.Vernacular.NHibernate.II.Providers;

namespace PPWCode.Vernacular.NHibernate.II.Async.Interfaces.Providers
{
    /// <inheritdoc />
    public interface ITransactionProviderAsync : ITransactionProvider
    {
        [NotNull]
        Task RunAsync(
            [NotNull] ISession session,
            IsolationLevel isolationLevel,
            Func<CancellationToken, Task> lambda,
            CancellationToken cancellationToken);

        [NotNull]
        [ItemCanBeNull]
        Task<TResult> RunAsync<TResult>(
            [NotNull] ISession session,
            IsolationLevel isolationLevel,
            [NotNull] Func<CancellationToken, Task<TResult>> lambda,
            CancellationToken cancellationToken);
    }
}
