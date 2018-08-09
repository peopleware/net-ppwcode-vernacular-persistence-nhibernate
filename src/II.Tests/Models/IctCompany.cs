﻿// Copyright 2017-2018 by PeopleWare n.v..
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

using NHibernate.Mapping.ByCode.Conformist;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Models
{
    [DataContract(IsReference = true)]
    [Serializable]
    public class IctCompany : Company
    {
        public IctCompany(int id, int persistenceVersion)
            : base(id, persistenceVersion)
        {
        }

        public IctCompany(int id)
            : base(id)
        {
        }

        public IctCompany()
        {
        }

        [DataMember]
        public virtual Address Address { get; set; }
    }

    public class IctCompanyMapper : SubclassMapping<IctCompany>
    {
        public IctCompanyMapper()
        {
            Component(c => c.Address);
        }
    }
}