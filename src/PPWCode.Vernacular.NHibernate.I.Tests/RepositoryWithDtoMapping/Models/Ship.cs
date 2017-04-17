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
using System.Collections.Generic;
using System.Runtime.Serialization;

using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.RepositoryWithDtoMapping.Models
{
    [Serializable, DataContract(IsReference = true)]
    public class Ship : PersistentObject<int>
    {
        [DataMember]
        private string m_Code;

        [DataMember]
        private ISet<CargoContainer> m_CargoContainers = new HashSet<CargoContainer>();

        public Ship()
        {
        }

        public Ship(int id)
            : base(id)
        {
        }

        public virtual string Code
        {
            get { return m_Code; }
            set { m_Code = value; }
        }

        public virtual ISet<CargoContainer> CargoContainers
        {
            get { return m_CargoContainers; }
        }

        public virtual void AddCargoContainer(CargoContainer cargoContainer)
        {
            if (cargoContainer != null && m_CargoContainers.Add(cargoContainer))
            {
                cargoContainer.Ship = this;
            }
        }

        public virtual void RemoveCargoContainer(CargoContainer cargoContainer)
        {
            if (cargoContainer != null && m_CargoContainers.Remove(cargoContainer))
            {
                cargoContainer.Ship = null;
            }
        }
    }
}
