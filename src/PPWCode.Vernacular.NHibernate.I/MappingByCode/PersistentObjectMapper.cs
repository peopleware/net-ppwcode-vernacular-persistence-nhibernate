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

using NHibernate.Mapping.ByCode.Conformist;

using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.MappingByCode
{
    public abstract class PersistentObjectMapper<T, TId>
        : ClassMapping<T>
        where T : class, IPersistentObject<TId>
        where TId : IEquatable<TId>
    {
        protected const int MaxUserNameLength = 128;

        protected PersistentObjectMapper()
        {
            Id(x => x.Id);
        }
    }
}
