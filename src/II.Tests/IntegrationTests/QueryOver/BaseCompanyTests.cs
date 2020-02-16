// Copyright 2018 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using JetBrains.Annotations;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.II.Tests.Models;
using PPWCode.Vernacular.NHibernate.II.Tests.Repositories;

namespace PPWCode.Vernacular.NHibernate.II.Tests.IntegrationTests.QueryOver
{
    public abstract class BaseCompanyTests : BaseRepositoryTests<Company>
    {
        protected enum CompanyCreationType
        {
            /// <summary>
            ///     Save initially a company without any identifications.
            /// </summary>
            NO_CHILDREN,

            /// <summary>
            ///     Save initially a company with 2 identifications, they are identified by a Identification 1 and 2.
            /// </summary>
            WITH_2_CHILDREN
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            Repository = new CompanyQueryOverRepository(SessionProvider);
        }

        protected override void OnTeardown()
        {
            Repository = null;

            base.OnTeardown();
        }

        protected CompanyQueryOverRepository Repository { get; private set; }

        [NotNull]
        protected Company CreateCompany(CompanyCreationType companyCreationType)
        {
            Company company =
                new Company
                {
                    Name = "Peopleware NV"
                };

            if (companyCreationType == CompanyCreationType.WITH_2_CHILDREN)
            {
                // ReSharper disable once ObjectCreationAsStatement
                new CompanyIdentification
                {
                    Identification = "1",
                    Company = company
                };

                // ReSharper disable once ObjectCreationAsStatement
                new CompanyIdentification
                {
                    Identification = "2",
                    Company = company
                };
            }

            Company savedCompany = RunInsideTransaction(() => Repository.Merge(company), true);

            Assert.That(savedCompany, Is.Not.Null);
            Assert.That(savedCompany.IsTransient, Is.False);
            Assert.That(savedCompany, Is.Not.SameAs(company));
            Assert.That(savedCompany.PersistenceVersion, Is.EqualTo(1));
            Assert.That(companyCreationType == CompanyCreationType.NO_CHILDREN ? 0 : 2, Is.EqualTo(savedCompany.Identifications.Count));

            return savedCompany;
        }

        [NotNull]
        protected Company CreateFailedCompany(CompanyCreationType companyCreationType)
        {
            Company company = CreateCompany(companyCreationType);
            company.FailedCompany =
                new FailedCompany
                {
                    FailingDate = UtcNow
                };

            Company savedCompany = RunInsideTransaction(() => Repository.Merge(company), true);

            Assert.That(savedCompany, Is.Not.Null);
            Assert.That(savedCompany.IsTransient, Is.False);
            Assert.That(savedCompany, Is.Not.SameAs(company));
            Assert.That(savedCompany.PersistenceVersion, Is.EqualTo(1));
            Assert.That(savedCompany.FailedCompany, Is.Not.Null);
            Assert.That(savedCompany.FailedCompany.IsTransient, Is.False);
            Assert.That(savedCompany.FailedCompany, Is.Not.SameAs(company.FailedCompany));
            Assert.That(savedCompany.IsFailed, Is.True);

            return savedCompany;
        }

        [NotNull]
        protected Company CreateExtendedCompany(CompanyCreationType companyCreationType)
        {
            Company company = CreateCompany(companyCreationType);
            company.ExtendedCompany =
                new ExtendedCompany
                {
                    ExtraData = "Test-Test"
                };

            Company savedCompany = RunInsideTransaction(() => Repository.Merge(company), true);

            Assert.That(savedCompany, Is.Not.Null);
            Assert.That(savedCompany.IsTransient, Is.False);
            Assert.That(savedCompany, Is.Not.SameAs(company));
            Assert.That(savedCompany.PersistenceVersion, Is.EqualTo(2));
            Assert.That(savedCompany.ExtendedCompany, Is.Not.Null);
            Assert.That(savedCompany.ExtendedCompany.IsTransient, Is.False);
            Assert.That(savedCompany.ExtendedCompany, Is.Not.SameAs(company.ExtendedCompany));
            Assert.That(savedCompany.IsExtended, Is.True);

            return savedCompany;
        }
    }
}
