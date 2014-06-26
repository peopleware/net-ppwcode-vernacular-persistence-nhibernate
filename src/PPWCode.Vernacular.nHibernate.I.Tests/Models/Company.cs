using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.nHibernate.I.Tests.Models
{
    [Serializable, DataContract(IsReference = true)]
    public class Company : AuditableVersionedPersistentObject<int, int>
    {
        [DataMember]
        private IList<CompanyIdentification> m_Identifications;

        [DataMember]
        private string m_Name;

        public virtual string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public virtual IList<CompanyIdentification> Identifications
        {
            get { return m_Identifications; }
            set { m_Identifications = value; }
        }
    }
}