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

using System;

using Castle.Core.Logging;

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Implementations.Providers
{
    public class SafeEnvironmentProvider : ISafeEnvironmentProvider
    {
        public SafeEnvironmentProvider(IExceptionTranslator exceptionTranslator)
        {
            if (exceptionTranslator == null)
            {
                throw new ArgumentNullException(nameof(exceptionTranslator));
            }

            ExceptionTranslator = exceptionTranslator;
        }

        public IExceptionTranslator ExceptionTranslator { get; }

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public void Run(string requestDescription, Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            Run(requestDescription,
                () =>
                {
                    action.Invoke();
                    return default(int);
                });
        }

        public TResult Run<TResult>(string requestDescription, Func<TResult> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            string startMessage =
                Logger.IsInfoEnabled
                    ? $"Request {requestDescription} started."
                    : null;
            string finishMessage =
                Logger.IsInfoEnabled
                    ? $"Request {requestDescription} finished."
                    : null;

            return Run(startMessage, finishMessage, null, func);
        }

        public void Run<TEntity, TId>(string requestDescription, Action action, TEntity entity)
            where TEntity : class, IIdentity<TId>
            where TId : IEquatable<TId>
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            Run<TEntity, TId, int>(
                requestDescription,
                () =>
                {
                    action.Invoke();
                    return default(int);
                },
                entity);
        }

        public TResult Run<TEntity, TId, TResult>(string requestDescription, Func<TResult> func, TEntity entity)
            where TEntity : class, IIdentity<TId>
            where TId : IEquatable<TId>
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            string startMessage;
            if (Logger.IsInfoEnabled)
            {
                startMessage =
                    entity != null
                        ? $"Request {requestDescription} for class {typeof(TEntity).Name}, entity={entity} started."
                        : $"Request {requestDescription} for class {typeof(TEntity).Name} started.";
            }
            else
            {
                startMessage = null;
            }

            string finishMessage;
            if (Logger.IsInfoEnabled)
            {
                finishMessage =
                    entity != null
                        ? $"Request {requestDescription} for class {typeof(TEntity).Name}, entity={entity} finished."
                        : $"Request {requestDescription} for class {typeof(TEntity).Name} finished.";
            }
            else
            {
                finishMessage = null;
            }

            string failedMessage;
            if (Logger.IsInfoEnabled)
            {
                failedMessage =
                    entity != null
                        ? $"Request {requestDescription} for class {typeof(TEntity).Name}, entity={entity} failed."
                        : $"Request {requestDescription} for class {typeof(TEntity).Name} failed.";
            }
            else
            {
                failedMessage = null;
            }

            return Run(startMessage, finishMessage, failedMessage, func);
        }

        private TResult Run<TResult>(string startMessage, string finishedMessage, string failedMessage, Func<TResult> func)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info(startMessage);
            }

            TResult result;
            try
            {
                result = func.Invoke();
            }
            catch (Exception e)
            {
                Exception triagedException = ExceptionTranslator.Convert(failedMessage, e);
                if (triagedException != null)
                {
                    throw triagedException;
                }

                throw;
            }

            if (Logger.IsInfoEnabled)
            {
                Logger.Info(finishedMessage);
            }

            return result;
        }
    }
}
