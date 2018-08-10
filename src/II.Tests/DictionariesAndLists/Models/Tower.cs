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

using NHibernate.Type;

using PPWCode.Vernacular.NHibernate.II.MappingByCode;
using PPWCode.Vernacular.Persistence.III;

namespace PPWCode.Vernacular.NHibernate.II.Tests.DictionariesAndLists.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Tower : PersistentObject<int>
    {
        public Tower()
        {
        }

        public Tower(int id)
            : base(id)
        {
        }

        [DataMember]
        public virtual IList<Plane> Sections { get; } = new List<Plane>();

        [DataMember]
        public virtual IDictionary<SideEnum, ClippingPlane> Sides { get; } = new Dictionary<SideEnum, ClippingPlane>();
    }

    public class TowerMapper : PersistentObjectMapper<Tower, int>
    {
        public TowerMapper()
        {
            List(
                m => m.Sections,
                c =>
                {
                    c.Index(idx =>
                            {
                                idx.Base(1); // Note: You can not do idx.Base(0).
                                idx.Column("`Index`");
                            });
                    c.Table("ModelSection");
                    c.Key(km => { km.Column("TowerId"); });
                },
                cer =>
                {
                    cer.Component(
                        cp =>
                        {
                            cp.Component(
                                p => p.Normal,
                                pm =>
                                {
                                    pm.Property(pn => pn.X, pnm => { pnm.Column("PlaneNormalX"); });
                                    pm.Property(pn => pn.Y, pnm => { pnm.Column("PlaneNormalY"); });
                                    pm.Property(pn => pn.Z, pnm => { pnm.Column("PlaneNormalZ"); });
                                });
                            cp.Property(
                                p => p.Translation,
                                ptm => { ptm.Column("PlaneTranslation"); });
                        });
                });

            Map(
                m => m.Sides,
                c =>
                {
                    // standard collection options here
                    c.Table("ModelSide");
                    c.Key(sm => { sm.Column("TowerId"); });
                },
                k =>
                {
                    k.Element(
                        e =>
                        {
                            e.Column("Side");
                            e.Type<EnumStringType<SideEnum>>();
                        });
                },
                r =>
                {
                    r.Component(
                        ccp =>
                        {
                            ccp.Component(
                                x => x.Plane,
                                cp =>
                                {
                                    cp.Component(
                                        p => p.Normal,
                                        pm =>
                                        {
                                            pm.Property(pn => pn.X, pnm => { pnm.Column("ClippingPlanePlaneNormalX"); });
                                            pm.Property(pn => pn.Y, pnm => { pnm.Column("ClippingPlanePlaneNormalY"); });
                                            pm.Property(pn => pn.Z, pnm => { pnm.Column("ClippingPlanePlaneNormalZ"); });
                                        });
                                    cp.Property(
                                        p => p.Translation,
                                        ptm => { ptm.Column("ClippingPlanePlaneTranslation"); });
                                });
                            ccp.Component(
                                x => x.MeshTranslation,
                                mt =>
                                {
                                    mt.Property(pn => pn.X, pnm => { pnm.Column("ClippingPlaneMeshTranslationX"); });
                                    mt.Property(pn => pn.Y, pnm => { pnm.Column("ClippingPlaneMeshTranslationY"); });
                                    mt.Property(pn => pn.Z, pnm => { pnm.Column("ClippingPlaneMeshTranslationZ"); });
                                });
                        });
                });
        }
    }
}
