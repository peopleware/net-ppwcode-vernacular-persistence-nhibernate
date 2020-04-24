// Copyright 2020 by PeopleWare n.v..
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
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using JetBrains.Annotations;

using NHibernate.Mapping.ByCode.Conformist;

namespace PPWCode.Vernacular.NHibernate.II.Tests.Model.Common
{
    [DataContract(IsReference = true)]
    [Serializable]
    public class AllRoundIctCompany : IctCompany
    {
        public AllRoundIctCompany(int id, int persistenceVersion)
            : base(id, persistenceVersion)
        {
        }

        public AllRoundIctCompany(int id)
            : base(id)
        {
        }

        public AllRoundIctCompany()
        {
        }

        [DataMember]
        [Required]
        [StringLength(-1)]
        public virtual string AllRound { get; set; }
    }

    [UsedImplicitly]
    public class AllRoundIctCompanyMapper : SubclassMapping<AllRoundIctCompany>
    {
        public AllRoundIctCompanyMapper()
        {
            Property(c => c.AllRound);
        }
    }
}
