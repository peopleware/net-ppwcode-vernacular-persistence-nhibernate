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
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

using PPWCode.Vernacular.Exceptions.II;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Models
{
    [DataContract(IsReference = true), Serializable]
    public class FailedCompany : InsertAuditablePersistentObject<int>
    {
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Company == null || Company.FailedCompany == this);
        }

        [DataMember]
        private DateTime m_FailingDate;

        [DataMember]
        private Company m_Company;

        public virtual DateTime FailingDate
        {
            get { return m_FailingDate; }
            set
            {
                Contract.Ensures(FailingDate == value);

                m_FailingDate = value;
            }
        }

        public virtual Company Company
        {
            get { return m_Company; }
            set
            {
                Contract.Ensures(Company == value);
                // ReSharper disable once PossibleNullReferenceException
                Contract.Ensures(Contract.OldValue(Company) == null || Contract.OldValue(Company) == value || Contract.OldValue(Company).FailedCompany != this);
                Contract.Ensures(Company == null || Company.FailedCompany == this);

                if (m_Company != value)
                {
                    Company previousCompany = m_Company;
                    m_Company = value;

                    if (previousCompany != null)
                    {
                        previousCompany.FailedCompany = null;
                    }

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
}
