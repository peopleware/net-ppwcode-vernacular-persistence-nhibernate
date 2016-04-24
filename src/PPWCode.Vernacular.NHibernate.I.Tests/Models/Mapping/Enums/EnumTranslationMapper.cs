using PPWCode.Vernacular.NHibernate.I.MappingByCode;
using PPWCode.Vernacular.NHibernate.I.Tests.Models.Enums;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Models.Mapping.Enums
{
    public class EnumTranslationMapper : PersistentObjectMapper<EnumTranslation, int>
    {
        public EnumTranslationMapper()
        {
            Property(et => et.TranslationNl);
            Property(et => et.TranslationFr);
        }
    }
}