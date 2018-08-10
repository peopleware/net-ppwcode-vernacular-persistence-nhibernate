// Copyright 2017 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;

using NHibernate.Criterion;

using NUnit.Framework;

using PPWCode.Vernacular.Exceptions.III;
using PPWCode.Vernacular.NHibernate.II.Interfaces;
using PPWCode.Vernacular.NHibernate.II.Tests.EnumTranslation.Models;
using PPWCode.Vernacular.NHibernate.II.Tests.Repositories;

namespace PPWCode.Vernacular.NHibernate.II.Tests.EnumTranslation.Repositories
{
    public class GenericEnumTranslationRepository<TRoot, TEnum>
        : TestQueryOverRepository<TRoot>,
          IGenericEnumTranslationRepository<TRoot, TEnum>
        where TRoot : GenericEnumTranslation<TEnum>
        where TEnum : struct, IComparable, IConvertible, IFormattable
    {
        public GenericEnumTranslationRepository(ISessionProvider sessionProvider)
            : base(sessionProvider)
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ProgrammingError("Unsupported configuration.");
            }
        }

        public string Translate(TEnum code, string language)
        {
            TRoot translation =
                Execute(
                    nameof(Translate),
                    () => Session
                        .CreateCriteria<TRoot>()
                        .Add(Restrictions.Eq("Code", code))
                        .List<TRoot>().SingleOrDefault());
            Assert.IsNotNull(translation);

            string result = null;

            if (language == "nl")
            {
                result = translation.TranslationNl;
            }

            if (language == "fr")
            {
                result = translation.TranslationFr;
            }

            return result;
        }
    }
}
