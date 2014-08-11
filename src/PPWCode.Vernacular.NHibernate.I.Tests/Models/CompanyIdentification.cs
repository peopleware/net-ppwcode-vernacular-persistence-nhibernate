// Copyright 2014 by PeopleWare n.v..
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

using PPWCode.Vernacular.NHibernate.I.Semantics;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Models
{
    [Serializable, DataContract(IsReference = true)]
    public class CompanyIdentification : AuditablePersistentObject<int>
    {
        [ContractInvariantMethod]
        private void ObjectsInvariant()
        {
            Contract.Invariant(AssociationContracts.BiDirManyToOne(this, Company, c => c.Identifications));
        }

        [DataMember]
        private string m_Identification;

        [DataMember]
        private int m_Number;

        [DataMember]
        private Company m_Company;

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

        public virtual Company Company
        {
            get { return m_Company; }
            set
            {
                Contract.Ensures(Company == value);
                // ReSharper disable once PossibleNullReferenceException
                Contract.Ensures(Contract.OldValue(Company) == null || Contract.OldValue(Company) == value || !Contract.OldValue(Company).Identifications.Contains(this));
                Contract.Ensures(Company == null || Company.Identifications.Contains(this));

                if (Company != value)
                {
                    if (m_Company != null)
                    {
                        m_Company.RemoveIdentification(this);
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
}