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
    public class ClippingPlane
    {
        private Plane m_Plane;
        private Vector3D m_MeshTranslation;

        public virtual Plane Plane
        {
            get { return m_Plane; }
            set { m_Plane = value; }
        }

        public virtual Vector3D MeshTranslation
        {
            get { return m_MeshTranslation; }
            set { m_MeshTranslation = value; }
        }
    }

    public class ClippingPlaneMapper : ComponentMapping<ClippingPlane>
    {
        public ClippingPlaneMapper()
        {
            Component(p => p.Plane);
            Component(p => p.MeshTranslation);
        }
    }
}
