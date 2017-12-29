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
using System.Runtime.Serialization;

using PPWCode.Vernacular.NHibernate.I.MappingByCode;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.RepositoryWithDtoMapping.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class CargoContainer : PersistentObject<int>
    {
        [DataMember]
        private string m_Code;

        [DataMember]
        private int m_Load;

        [DataMember]
        private Ship m_Ship;

        public CargoContainer()
        {
        }

        public CargoContainer(int id)
            : base(id)
        {
        }

        public virtual string Code
        {
            get { return m_Code; }
            set { m_Code = value; }
        }

        public virtual int Load
        {
            get { return m_Load; }
            set { m_Load = value; }
        }

        public virtual Ship Ship
        {
            get { return m_Ship; }

            set
            {
                if (m_Ship != value)
                {
                    if (m_Ship != null)
                    {
                        Ship previousShip = m_Ship;
                        m_Ship = null;
                        previousShip.RemoveCargoContainer(this);
                    }

                    m_Ship = value;
                    if (m_Ship != null)
                    {
                        m_Ship.AddCargoContainer(this);
                    }
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
