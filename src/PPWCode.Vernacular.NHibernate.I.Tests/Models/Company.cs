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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

using PPWCode.Vernacular.NHibernate.I.Semantics;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Models
{
    [Serializable, DataContract(IsReference = true)]
    public class Company : AuditableVersionedPersistentObject<int, int>
    {
        public Company(int id, int persistenceVersion)
            : base(id, persistenceVersion)
        {
        }

        public Company(int id)
            : base(id)
        {
        }

        public Company()
        {
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Identifications != null);
            Contract.Invariant(AssociationContracts.BiDirParentToChild(this, Identifications, i => i.Company));
            Contract.Invariant(FailedCompany == null || FailedCompany.Company == this);
            Contract.Invariant(IsFailed == (FailedCompany != null));
        }

        [DataMember]
        private string m_Name;

        [DataMember]
        private FailedCompany m_FailedCompany;

        [DataMember]
        private ISet<CompanyIdentification> m_Identifications = new HashSet<CompanyIdentification>();

        [Required, StringLength(128)]
        public virtual string Name
        {
            get { return m_Name; }
            set
            {
                Contract.Ensures(Name == value);

                m_Name = value;
            }
        }

        public virtual FailedCompany FailedCompany
        {
            get { return m_FailedCompany; }
            set
            {
                Contract.Ensures(FailedCompany == value);
                // ReSharper disable once PossibleNullReferenceException
                Contract.Ensures(Contract.OldValue(FailedCompany) == null || Contract.OldValue(FailedCompany) == value || Contract.OldValue(FailedCompany).Company != this);
                Contract.Ensures(FailedCompany == null || FailedCompany.Company == this);

                if (m_FailedCompany != value)
                {
                    FailedCompany previousFailedCompany = m_FailedCompany;
                    m_FailedCompany = value;

                    if (previousFailedCompany != null)
                    {
                        previousFailedCompany.Company = null;
                    }

                    if (m_FailedCompany != null)
                    {
                        m_FailedCompany.Company = this;
                    }
                }
            }
        }

        public virtual bool IsFailed
        {
            get
            {
                Contract.Ensures(Contract.Result<bool>() == (FailedCompany != null));

                return FailedCompany != null;
            }
        }

        public virtual ISet<CompanyIdentification> Identifications
        {
            get { return m_Identifications; }
        }

        public virtual void RemoveIdentification(CompanyIdentification companyIdentification)
        {
            Contract.Ensures(!Identifications.Contains(companyIdentification));

            if (companyIdentification != null && m_Identifications.Remove(companyIdentification))
            {
                companyIdentification.Company = null;
            }
        }

        public virtual void AddIdentification(CompanyIdentification companyIdentification)
        {
            Contract.Ensures(Identifications.Contains(companyIdentification));

            if (companyIdentification != null && m_Identifications.Add(companyIdentification))
            {
                companyIdentification.Company = this;
            }
        }
    }
}
