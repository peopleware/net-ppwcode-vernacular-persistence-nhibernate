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

using NHibernate.Mapping.ByCode.Conformist;

namespace PPWCode.Vernacular.NHibernate.I.Tests.DictionariesAndLists.Models
{
    public class Vector3D
    {
        private double m_X;
        private double m_Y;
        private double m_Z;

        public virtual double X
        {
            get { return m_X; }
            set { m_X = value; }
        }

        public virtual double Y
        {
            get { return m_Y; }
            set { m_Y = value; }
        }

        public virtual double Z
        {
            get { return m_Z; }
            set { m_Z = value; }
        }
    }

    public class Vector3DMapper : ComponentMapping<Vector3D>
    {
        public Vector3DMapper()
        {
            Property(v => v.X);
            Property(v => v.Y);
            Property(v => v.Z);
        }
    }
}
