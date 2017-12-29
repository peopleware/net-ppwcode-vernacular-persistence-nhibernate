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

using NHibernate.Mapping.ByCode;

using PPWCode.Vernacular.Exceptions.II;
using PPWCode.Vernacular.NHibernate.I.MappingByCode;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Models
{
    [DataContract(IsReference = true)]
    [Serializable]
    [AuditLog(AuditLogAction = AuditLogActionEnum.ALL)]
    public class FailedCompany : InsertAuditablePersistentObject<int>
    {
        [DataMember]
        private DateTime m_FailingDate;

        [DataMember]
        private Company m_Company;

        public virtual DateTime FailingDate
        {
            get { return m_FailingDate; }
            set { m_FailingDate = value; }
        }

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
                        previousCompany.FailedCompany = null;
                    }

                    m_Company = value;
                    if (m_Company != null)
                    {
                        m_Company.FailedCompany = this;
                    }
                }
            }
        }

        public override CompoundSemanticException WildExceptions()
        {
            CompoundSemanticException sce = base.WildExceptions();

            if (Company == null)
            {
                sce.AddElement(new PropertyException(this, "Company", PropertyException.MandatoryMessage, null));
            }

            return sce;
        }
    }

    public class FailedCompanyMapper : InsertAuditablePersistentObjectMapper<FailedCompany, int>
    {
        public FailedCompanyMapper()
        {
            Id(fc => fc.Id, m => m.Generator(Generators.Foreign<FailedCompany>(fc => fc.Company)));
            Property(fc => fc.FailingDate);
            OneToOne(fc => fc.Company, m => m.Constrained(true));
        }
    }
}
