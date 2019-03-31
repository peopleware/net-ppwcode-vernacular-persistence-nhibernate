// Copyright 2017 by PeopleWare n.v..
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

using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.II.Test
{
    public abstract class BaseRepositoryFixture<TId, TAuditEntity>
        : NHibernateSqlServerSetUpFixture<TId, TAuditEntity>
        where TId : IEquatable<TId>
        where TAuditEntity : AuditLog<TId>, new()
    {
        private DateTime? _utcNow;

        protected override DateTime UtcNow
        {
            get
            {
                if (_utcNow == null)
                {
                    _utcNow = DateTime.UtcNow;
                }

                return _utcNow.Value;
            }
        }

        protected override bool UseUtc
            => true;

        protected override void OnTeardown()
        {
            _utcNow = null;

            base.OnTeardown();
        }
    }
}
