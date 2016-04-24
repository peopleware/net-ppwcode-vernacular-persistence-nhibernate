using System;

using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;

using PPWCode.Vernacular.NHibernate.I.Tests.Models.Enums;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Models.Mapping.Enums
{
    public abstract class GenericEnumTranslationMapper<T> : SubclassMapping<GenericEnumTranslation<T>>
        where T : struct, IComparable, IConvertible, IFormattable
    {
        protected GenericEnumTranslationMapper()
        {
            Property(
                e => e.Code,
                m =>
                {
                    m.Type<EnumStringType<T>>();
                });
        }
    }
}