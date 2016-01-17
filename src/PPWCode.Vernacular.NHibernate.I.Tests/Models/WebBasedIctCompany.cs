// Copyright 2016 by PeopleWare n.v..
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
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Models
{
    [DataContract(IsReference = true), Serializable]
    public class WebBasedIctCompany : IctCompany
    {
        [DataMember]
        private string m_WebBased;

        public WebBasedIctCompany(int id, int persistenceVersion)
            : base(id, persistenceVersion)
        {
        }

        public WebBasedIctCompany(int id)
            : base(id)
        {
        }

        public WebBasedIctCompany()
        {
        }

        public virtual string WebBased
        {
            get { return m_WebBased; }
            set
            {
                Contract.Ensures(WebBased == value);

                m_WebBased = value;
            }
        }
    }
}