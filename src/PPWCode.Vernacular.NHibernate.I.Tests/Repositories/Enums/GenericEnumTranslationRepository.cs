﻿// Copyright 2016 by PeopleWare n.v..
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

using NHibernate;
using NHibernate.Criterion;

using PPWCode.Vernacular.Exceptions.II;
using PPWCode.Vernacular.NHibernate.I.Tests.Models;
using PPWCode.Vernacular.NHibernate.I.Tests.Models.Enums;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Repositories.Enums
{
    public class GenericEnumTranslationRepository<T, X>
        : TestRepository<T>,
          IGenericEnumTranslationRepository<T, X>
        where T : GenericEnumTranslation<X>
        where X : struct, IComparable, IConvertible, IFormattable
    {
        public GenericEnumTranslationRepository(ISession session)
            : base(session)
        {
            if (!typeof(X).IsEnum)
            {
                throw new ProgrammingError("Unsupported configuration.");
            }
        }

        public string Translate(X code, string language)
        {
            var translation = Execute(
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