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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Common.Logging;

using JetBrains.Annotations;

using PPWCode.Vernacular.NHibernate.III.Async.Interfaces.Providers;
using PPWCode.Vernacular.NHibernate.III.DbConstraint;
using PPWCode.Vernacular.NHibernate.III.Providers;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Async.Implementations.Providers
{
    /// <inheritdoc cref="ISafeEnvironmentProviderAsync" />
    [UsedImplicitly]
    public class SafeEnvironmentProviderAsync
        : SafeEnvironmentProvider,
          ISafeEnvironmentProviderAsync
    {
        [NotNull]
        private static readonly ILog _logger = LogManager.GetLogger<SafeEnvironmentProviderAsync>();

        public SafeEnvironmentProviderAsync([NotNull] IExceptionTranslator exceptionTranslator)
            : base(exceptionTranslator)
        {
        }

        /// <inheritdoc />
        public async Task RunAsync(
            string requestDescription,
            Func<CancellationToken, Task> lambda,
            CancellationToken cancellationToken)
        {
            async Task<int> WrapperAsync(CancellationToken can)
            {
                await lambda(can).ConfigureAwait(false);
                return default;
            }

            await RunAsync(requestDescription, WrapperAsync, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TResult> RunAsync<TResult>(
            string requestDescription,
            Func<CancellationToken, Task<TResult>> lambda,
            CancellationToken cancellationToken)
        {
            if (lambda == null)
            {
                throw new ArgumentNullException(nameof(lambda));
            }

            string StartMessage()
                => $"Request {requestDescription} started.";

            string FinishMessage()
                => $"Request {requestDescription} finished.";

            string FailedMessage()
                => $"Request {requestDescription} failed.";

            return await RunAsync(StartMessage, FinishMessage, FailedMessage, lambda, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task RunAsync<TEntity, TId>(
            string requestDescription,
            Func<CancellationToken, Task> lambda,
            TEntity entity,
            CancellationToken cancellationToken)
            where TEntity : class, IIdentity<TId>
            where TId : IEquatable<TId>
        {
            async Task<int> WrapperAsync(CancellationToken can)
            {
                await lambda(can).ConfigureAwait(false);
                return default;
            }

            await RunAsync<TEntity, TId, int>(requestDescription, WrapperAsync, entity, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<TResult> RunAsync<TEntity, TId, TResult>(
            string requestDescription,
            Func<CancellationToken, Task<TResult>> lambda,
            TEntity entity,
            CancellationToken cancellationToken)
            where TEntity : class, IIdentity<TId>
            where TId : IEquatable<TId>
        {
            if (lambda == null)
            {
                throw new ArgumentNullException(nameof(lambda));
            }

            string StartMessage()
                => entity != null
                       ? $"Request {requestDescription} for class {typeof(TEntity).Name}, entity={entity} started"
                       : $"Request {requestDescription} for class {typeof(TEntity).Name} started";

            string FinishMessage()
                => entity != null
                       ? $"Request {requestDescription} for class {typeof(TEntity).Name}, entity={entity} finished"
                       : $"Request {requestDescription} for class {typeof(TEntity).Name} finished";

            string FailedMessage()
                => entity != null
                       ? $"Request {requestDescription} for class {typeof(TEntity).Name}, entity={entity} failed"
                       : $"Request {requestDescription} for class {typeof(TEntity).Name} failed";

            return await RunAsync(StartMessage, FinishMessage, FailedMessage, lambda, cancellationToken).ConfigureAwait(false);
        }

        [NotNull]
        [ItemCanBeNull]
        protected virtual async Task<TResult> RunAsync<TResult>(
            [NotNull] Func<string> startMessage,
            [NotNull] Func<string> finishedMessage,
            [NotNull] Func<string> failedMessage,
            [NotNull] Func<CancellationToken, Task<TResult>> lambda,
            CancellationToken cancellationToken)
        {
            Stopwatch sw = null;
            if (_logger.IsInfoEnabled)
            {
                _logger.Info(startMessage());
                sw = new Stopwatch();
                sw.Start();
            }

            TResult result;
            try
            {
                result = await lambda(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw ExceptionTranslator.Convert(failedMessage() ?? e.Message, e);
            }
            finally
            {
                sw?.Stop();
            }

            if (_logger.IsInfoEnabled)
            {
                _logger.Info(sw != null ? $"{finishedMessage()}, elapsed {sw.ElapsedMilliseconds} ms." : finishedMessage());
            }

            return result;
        }
    }
}
