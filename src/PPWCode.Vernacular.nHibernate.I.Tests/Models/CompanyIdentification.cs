using System;
using System.Runtime.Serialization;

using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.nHibernate.I.Tests.Models
{
    [Serializable, DataContract(IsReference = true)]
    public class CompanyIdentification : AuditableVersionedPersistentObject<int, int>
    {
        [DataMember]
        private string m_Identification;

        [DataMember]
        private int m_Number;

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
    }
}