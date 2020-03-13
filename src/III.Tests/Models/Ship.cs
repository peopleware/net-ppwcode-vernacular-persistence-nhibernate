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
using System.Collections.Generic;
using System.Runtime.Serialization;

using JetBrains.Annotations;

using NHibernate.Mapping.ByCode;

using PPWCode.Vernacular.NHibernate.III.MappingByCode;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Tests.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Ship : PersistentObject<int>
    {
        [DataMember]
        private readonly ISet<CargoContainer> _cargoContainers = new HashSet<CargoContainer>();

        public Ship()
        {
        }

        public Ship(int id)
            : base(id)
        {
        }

        [DataMember]
        public virtual string Code { get; set; }

        [AuditLogPropertyIgnore]
        public virtual ISet<CargoContainer> CargoContainers
            => _cargoContainers;

        public virtual void AddCargoContainer(CargoContainer cargoContainer)
        {
            if ((cargoContainer != null) && CargoContainers.Add(cargoContainer))
            {
                cargoContainer.Ship = this;
            }
        }

        public virtual void RemoveCargoContainer(CargoContainer cargoContainer)
        {
            if ((cargoContainer != null) && CargoContainers.Remove(cargoContainer))
            {
                cargoContainer.Ship = null;
            }
        }
    }

    [UsedImplicitly]
    public class ShipMapper : PersistentObjectMapper<Ship, int>
    {
        public ShipMapper()
        {
            Property(s => s.Code);

            Set(
                s => s.CargoContainers,
                m => m.Cascade(Cascade.All | Cascade.Merge),
                r => r.OneToMany());
        }
    }
}
