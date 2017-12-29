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

using NHibernate.Mapping.ByCode.Conformist;

namespace PPWCode.Vernacular.NHibernate.I.Tests.DictionariesAndLists.Models
{
    public class Plane
    {
        private Vector3D m_Normal;
        private double m_Translation;

        public virtual Vector3D Normal
        {
            get { return m_Normal; }
            set { m_Normal = value; }
        }

        public virtual double Translation
        {
            get { return m_Translation; }
            set { m_Translation = value; }
        }
    }

    public class PlaneMapper : ComponentMapping<Plane>
    {
        public PlaneMapper()
        {
            Component(p => p.Normal);
            Property(p => p.Translation);
        }
    }
}
