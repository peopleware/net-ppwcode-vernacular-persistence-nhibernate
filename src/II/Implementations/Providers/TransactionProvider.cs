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

using System;
using System.Transactions;

using NHibernate;

using IsolationLevel = System.Data.IsolationLevel;

namespace PPWCode.Vernacular.NHibernate.II.Providers
{
    public class TransactionProvider : ITransactionProvider
    {
        public void Run(ISession session, IsolationLevel isolationLevel, Action action)
        {
            Func<int> ActionWrapper()
            {
                return () =>
                       {
                           action.Invoke();
                           return default;
                       };
            }

            Run(session, isolationLevel, ActionWrapper());
        }

        public TResult Run<TResult>(ISession session, IsolationLevel isolationLevel, Func<TResult> func)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            if (session.Transaction.IsActive || (Transaction.Current != null))
            {
                return func.Invoke();
            }

            TResult result;
            ITransaction transaction = session.BeginTransaction(isolationLevel);
            try
            {
                result = func.Invoke();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
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
