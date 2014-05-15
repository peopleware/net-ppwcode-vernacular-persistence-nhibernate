using System;
using System.Runtime.Serialization;

using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.nHibernate.I.Tests.Models
{
    [Serializable, DataContract(IsReference = true)]
    public class Company : AuditableVersionedPersistentObject<int, int>
    {
        public virtual string Name { get; set; }
    }
}