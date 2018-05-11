// Copyright 2017-2018 by PeopleWare n.v..
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

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.NHibernate.I.Tests.EnumTranslation.Models;
using PPWCode.Vernacular.NHibernate.I.Tests.EnumTranslation.Repositories;
using PPWCode.Vernacular.NHibernate.I.Tests.IntegrationTests.QueryOver;

namespace PPWCode.Vernacular.NHibernate.I.Tests.EnumTranslation
{
    public class EnumTranslationTests : BaseRepositoryTests<GenderEnumTranslation>
    {
        protected override Func<IQueryOverRepository<GenderEnumTranslation, int>> RepositoryFactory
        {
            get { return () => new GenericEnumTranslationRepository<GenderEnumTranslation, GenderEnum>(SessionProvider); }
        }

        public IGenericEnumTranslationRepository<GenderEnumTranslation, GenderEnum> GenderEnumTranslationRepository
            => new GenericEnumTranslationRepository<GenderEnumTranslation, GenderEnum>(SessionProvider);

        public IGenericEnumTranslationRepository<SalutationEnumTranslation, SalutationEnum> SalutationEnumTranslationRepository
            => new GenericEnumTranslationRepository<SalutationEnumTranslation, SalutationEnum>(SessionProvider);

        /// <summary>
        ///     Override this method for setup code that needs to run for each test separately.
        /// </summary>
        protected override void OnSetup()
        {
            base.OnSetup();
            SessionFactory.Statistics.Clear();
        }

        [Test]
        public void Test()
        {
            RunInsideTransaction(
                () =>
                {
                    GenderEnumTranslation tr = new GenderEnumTranslation();
                    tr.Code = GenderEnum.MALE;
                    tr.TranslationFr = "Homme";
                    tr.TranslationNl = "Man";

                    GenderEnumTranslationRepository.Merge(tr);
                },
                true);

            string translation = null;
            RunInsideTransaction(
                () => { translation = GenderEnumTranslationRepository.Translate(GenderEnum.MALE, "nl"); }, true);

            // test
            Console.WriteLine("Translation is: {0}", translation);
            Console.WriteLine("Teardown done.");
        }

        [Test]
        public void TestMore()
        {
            GenderEnumTranslation getMale =
                new GenderEnumTranslation
                {
                    Code = GenderEnum.MALE,
                    TranslationNl = "Man",
                    TranslationFr = "Homme"
                };

            GenderEnumTranslation getFemale =
                new GenderEnumTranslation
                {
                    Code = GenderEnum.FEMALE,
                    TranslationNl = "Vrouw",
                    TranslationFr = "Femme"
                };

            SalutationEnumTranslation setMr =
                new SalutationEnumTranslation
                {
                    Code = SalutationEnum.MR,
                    TranslationNl = "Meneer",
                    TranslationFr = "Monsieur"
                };

            SalutationEnumTranslation setMrs =
                new SalutationEnumTranslation
                {
                    Code = SalutationEnum.MRS,
                    TranslationNl = "Mevrouw",
                    TranslationFr = "Madamme"
                };

            SalutationEnumTranslation setMs =
                new SalutationEnumTranslation
                {
                    Code = SalutationEnum.MS,
                    TranslationNl = "Juffrouw",
                    TranslationFr = "Mademoiselle"
                };

            RunInsideTransaction(
                () =>
                {
                    GenderEnumTranslationRepository.Merge(getMale);
                    GenderEnumTranslationRepository.Merge(getFemale);
                    SalutationEnumTranslationRepository.Merge(setMr);
                    SalutationEnumTranslationRepository.Merge(setMrs);
                    SalutationEnumTranslationRepository.Merge(setMs);
                },
                true);

            string translation = null;

            RunInsideTransaction(() => translation = GenderEnumTranslationRepository.Translate(GenderEnum.FEMALE, "fr"), true);
            Assert.AreEqual("Femme", translation);

            RunInsideTransaction(() => translation = SalutationEnumTranslationRepository.Translate(SalutationEnum.MS, "nl"), true);
            Assert.AreEqual("Juffrouw", translation);
        }
    }
}
