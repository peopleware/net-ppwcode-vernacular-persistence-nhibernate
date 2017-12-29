// Copyright 2017 by PeopleWare n.v..
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
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using NHibernate.Mapping.ByCode.Conformist;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Address
    {
        [DataMember]
        [Required]
        [StringLength(128)]
        public virtual string Street { get; set; }

        [DataMember]
        [Required]
        [StringLength(16)]
        public virtual string Number { get; set; }

        [DataMember]
        [StringLength(16)]
        public virtual string Box { get; set; }
    }

    public class AddressMapper : ComponentMapping<Address>
    {
        public AddressMapper()
        {
            Property(a => a.Street);
            Property(a => a.Number);
            Property(a => a.Box);
        }
    }
}
