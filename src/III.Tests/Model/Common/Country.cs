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
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using JetBrains.Annotations;

using PPWCode.Vernacular.NHibernate.III.MappingByCode;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Tests.Model.Common
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Country : AuditableVersionedPersistentObject<int, int>
    {
        public Country(int id, int persistenceVersion)
            : base(id, persistenceVersion)
        {
        }

        public Country(int id)
            : base(id)
        {
        }

        public Country()
        {
        }

        [DataMember]
        [Required]
        public virtual string Name { get; set; }
    }

    [UsedImplicitly]
    public class CountryMapper : AuditableVersionedPersistentObjectMapper<Country, int, int>
    {
        public CountryMapper()
        {
            Property(c => c.Name);
        }
    }
}
