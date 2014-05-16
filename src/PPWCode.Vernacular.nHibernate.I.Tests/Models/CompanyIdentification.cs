using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.nHibernate.I.Tests.Models
{
    public class CompanyIdentification : AuditableVersionedPersistentObject<int, int>
    {
        public virtual string Identification { get; set; }
        public virtual int Number { get; set; }
    }
}