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

using Iesi.Collections.Generic;

using PPWCode.Vernacular.NHibernate.I.Semantics;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Models
{
    [Serializable, DataContract(IsReference = true)]
    public class Company : AuditableVersionedPersistentObject<int, int>
    {
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Identifications != null);
            Contract.Invariant(AssociationContracts.BiDirParentToChild(this, Identifications, i => i.Company));
        }

        [DataMember]
        private ISet<CompanyIdentification> m_Identifications = new HashedSet<CompanyIdentification>();

        [DataMember]
        private string m_Name;

        public virtual string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
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