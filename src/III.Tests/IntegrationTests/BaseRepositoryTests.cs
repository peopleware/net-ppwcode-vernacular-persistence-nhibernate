// Copyright 2020 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Threading;

using JetBrains.Annotations;

using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests
{
    public abstract class BaseRepositoryTests<T> : BaseQueryTests
        where T : class, IIdentity<int>
    {
        [CanBeNull]
        private CancellationTokenSource _cancellationTokenSource;

        protected override void OnSetup()
        {
            base.OnSetup();

            SessionFactory.Statistics.Clear();
        }

        /// <inheritdoc />
        protected override void OnTeardown()
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            base.OnTeardown();
        }

        [NotNull]
        protected CancellationTokenSource CancellationTokenSource
            => _cancellationTokenSource ?? (_cancellationTokenSource = new CancellationTokenSource());

        protected CancellationToken CancellationToken
            => CancellationTokenSource.Token;
    }
}
