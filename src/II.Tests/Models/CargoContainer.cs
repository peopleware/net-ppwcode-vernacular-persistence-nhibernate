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
using System.Runtime.Serialization;

using PPWCode.Vernacular.NHibernate.II.MappingByCode;
using PPWCode.Vernacular.Persistence.III;

namespace PPWCode.Vernacular.NHibernate.II.Tests.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class CargoContainer : PersistentObject<int>
    {
        [DataMember]
        private Ship _ship;

        public CargoContainer()
        {
        }

        public CargoContainer(int id)
            : base(id)
        {
        }

        [DataMember]
        public virtual string Code { get; set; }

        [DataMember]
        public virtual int Load { get; set; }

        public virtual Ship Ship
        {
            get => _ship;
            set
            {
                if (_ship != value)
                {
                    if (_ship != null)
                    {
                        Ship previousShip = _ship;
                        _ship = null;
                        previousShip.RemoveCargoContainer(this);
                    }

                    _ship = value;
                    _ship?.AddCargoContainer(this);
                }
            }
        }
    }

    public class CargoContainerMapper : PersistentObjectMapper<CargoContainer, int>
    {
        public CargoContainerMapper()
        {
            Property(c => c.Code);
            Property(c => c.Load);

            ManyToOne(c => c.Ship);
        }
    }
}
