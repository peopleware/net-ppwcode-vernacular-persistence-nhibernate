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

using JetBrains.Annotations;

using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Providers
{
    public interface ISafeEnvironmentProvider
    {
        void Run([NotNull] string requestDescription, [NotNull] Action action);

        [CanBeNull]
        TResult Run<TResult>([NotNull] string requestDescription, [NotNull] Func<TResult> func);

        void Run<TEntity, TId>([NotNull] string requestDescription, [NotNull] Action action, [CanBeNull] TEntity entity)
            where TEntity : class, IIdentity<TId>
            where TId : IEquatable<TId>;

        [CanBeNull]
        TResult Run<TEntity, TId, TResult>([NotNull] string requestDescription, [NotNull] Func<TResult> func, [CanBeNull] TEntity entity)
            where TEntity : class, IIdentity<TId>
            where TId : IEquatable<TId>;
    }
}
