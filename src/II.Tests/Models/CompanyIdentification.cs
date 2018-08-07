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

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using PPWCode.Vernacular.NHibernate.I.MappingByCode;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class CompanyIdentification : AuditablePersistentObject<int>
    {
        [DataMember]
        private Company m_Company;

        [DataMember]
        private string m_Identification;

        [DataMember]
        private int m_Number;

        [Required]
        [StringLength(256)]
        public virtual string Identification
        {
            get { return m_Identification; }
            set { m_Identification = value; }
        }

        public virtual int Number
        {
            get { return m_Number; }
            set { m_Number = value; }
        }

        [Required]
        public virtual Company Company
        {
            get { return m_Company; }
            set
            {
                if (m_Company != value)
                {
                    if (m_Company != null)
                    {
                        Company previousCompany = m_Company;
                        m_Company = null;
                        previousCompany.RemoveIdentification(this);
                    }

                    m_Company = value;
                    if (m_Company != null)
                    {
                        m_Company.AddIdentification(this);
                    }
                }
            }
        }
    }

    public class CompanyIdentificationMapper : AuditablePersistentObjectMapper<CompanyIdentification, int>
    {
        public CompanyIdentificationMapper()
        {
            Property(ci => ci.Identification);
            Property(ci => ci.Number);
            ManyToOne(ci => ci.Company);
        }
    }
}
