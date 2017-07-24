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

using System.Collections.Generic;

using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.DictionariesAndLists.Models
{
    public class Tower : PersistentObject<int>
    {
        private IList<Plane> m_Sections = new List<Plane>();
        private IDictionary<SideEnum, ClippingPlane> m_Sides = new Dictionary<SideEnum, ClippingPlane>();

        public Tower()
        {
        }

        public Tower(int id)
            : base(id)
        {
        }

        public virtual IList<Plane> Sections
        {
            get { return m_Sections; }
            set { m_Sections = value; }
        }

        public virtual IDictionary<SideEnum, ClippingPlane> Sides
        {
            get { return m_Sides; }
            set { m_Sides = value; }
        }
    }
}