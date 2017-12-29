// Copyright 2017 by PeopleWare n.v..
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

using PPWCode.Vernacular.NHibernate.I.MappingByCode;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.EnumTranslation.Models
{
    public class EnumTranslation : PersistentObject<int>
    {
        private string m_TranslationNl;
        private string m_TranslationFr;

        protected EnumTranslation()
        {
        }

        protected EnumTranslation(int id)
            : base(id)
        {
        }

        public virtual string TranslationNl
        {
            get { return m_TranslationNl; }
            set { m_TranslationNl = value; }
        }

        public virtual string TranslationFr
        {
            get { return m_TranslationFr; }
            set { m_TranslationFr = value; }
        }
    }

    public class EnumTranslationMapper : PersistentObjectMapper<EnumTranslation, int>
    {
        public EnumTranslationMapper()
        {
            Property(et => et.TranslationNl);
            Property(et => et.TranslationFr);
        }
    }
}
