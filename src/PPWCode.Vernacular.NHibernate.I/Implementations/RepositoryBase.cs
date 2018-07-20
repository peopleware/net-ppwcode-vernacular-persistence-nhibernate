// Copyright 2017-2018 by PeopleWare n.v..
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
using System.Data;

using NHibernate;

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Implementations
{
    public abstract class RepositoryBase<T, TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        protected RepositoryBase(ISessionProvider sessionProvider)
        {
            if (sessionProvider == null)
            {
                throw new ArgumentNullException(nameof(sessionProvider));
            }

            SessionProvider = sessionProvider;
        }

        public ISessionProvider SessionProvider { get; }

        protected ISession Session
            => SessionProvider.Session;

        protected ITransactionProvider TransactionProvider
            => SessionProvider.TransactionProvider;

        protected ISafeEnvironmentProvider SafeEnvironmentProvider
            => SessionProvider.SafeEnvironmentProvider;

        protected IsolationLevel IsolationLevel
            => SessionProvider.IsolationLevel;

        protected virtual void Execute(string requestDescription, Action action)
        {
            Execute(requestDescription, action, null);
        }

        protected virtual TResult Execute<TResult>(string requestDescription, Func<TResult> func, T entity)
        {
            return
                TransactionProvider
                    .Run(Session,
                         IsolationLevel,
                         () => SafeEnvironmentProvider
                             .Run<T, TId, TResult>(requestDescription, func, entity));
        }

        protected virtual void Execute(string requestDescription, Action action, T entity)
        {
            TransactionProvider
                .Run(Session,
                     IsolationLevel,
                     () => SafeEnvironmentProvider
                         .Run<T, TId>(requestDescription, action, entity));
        }

        protected virtual TResult Execute<TResult>(string requestDescription, Func<TResult> func)
            => Execute(requestDescription, func, null);
    }
}
