﻿// Copyright 2017-2018 by PeopleWare n.v..
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
using System.Linq;

using NHibernate.Criterion;

using PPWCode.Vernacular.Exceptions.II;
using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.NHibernate.I.Tests.EnumTranslation.Models;
using PPWCode.Vernacular.NHibernate.I.Tests.Repositories;

namespace PPWCode.Vernacular.NHibernate.I.Tests.EnumTranslation.Repositories
{
    public class GenericEnumTranslationRepository<T, X>
        : TestQueryOverRepository<T>,
          IGenericEnumTranslationRepository<T, X>
        where T : GenericEnumTranslation<X>
        where X : struct, IComparable, IConvertible, IFormattable
    {
        public GenericEnumTranslationRepository(ISessionProvider sessionProvider)
            : base(sessionProvider)
        {
            if (!typeof(X).IsEnum)
            {
                throw new ProgrammingError("Unsupported configuration.");
            }
        }

        public string Translate(X code, string language)
        {
            T translation = Execute(
                "Translate",
                () => Session
                    .CreateCriteria<T>()
                    .Add(Restrictions.Eq("Code", code))
                    .List<T>().SingleOrDefault());

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
