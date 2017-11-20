using NHibernate.Cfg.MappingSchema;

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.NHibernate.I.Test;
using PPWCode.Vernacular.NHibernate.I.Tests.Models.Mapping;

namespace PPWCode.Vernacular.NHibernate.I.Tests.IntegrationTests
{
    public abstract class BaseQueryTests : BaseRepositoryFixture<int, TestIntAuditLog>
    {
        protected override string CatalogName
        {
            get { return "Test.PPWCode.Vernacular.NHibernate.I.Tests"; }
        }

        protected override string ConnectionString
        {
            get { return FixedConnectionString; }
        }

        protected override HbmMapping GetHbmMapping()
        {
            IHbmMapping mapper = new TestsSimpleModelMapper(new TestsMappingAssemblies());
            return mapper.GetHbmMapping();
        }

        protected override string IdentityName
        {
            get { return "Test - IdentityName"; }
        }
    }
}