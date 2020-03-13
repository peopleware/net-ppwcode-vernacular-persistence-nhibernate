// Copyright 2020 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests.Async.Linq.Common.Repositories;
using PPWCode.Vernacular.NHibernate.III.Tests.Model.Common;

namespace PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests.Async.Linq.Common
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

        [CanBeNull]
        private CompanyRepository _repository;

        protected override void OnTeardown()
        {
            _repository = null;

            base.OnTeardown();
        }

        [NotNull]
        protected CompanyRepository Repository
            => _repository ?? (_repository = new CompanyRepository(SessionProviderAsync));

        protected async Task<Company> CreateCompanyAsync(
            CompanyCreationType companyCreationType,
            CancellationToken cancellationToken)
        {
            Company company =
                new IctCompany
                {
                    Name = "Peopleware NV",
                    Address =
                        new AddressBuilder()
                            .Street("Duwijckstraat")
                            .Number("17")
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

            Company savedCompany =
                await Repository
                    .MergeAsync(company, cancellationToken)
                    .ConfigureAwait(false);
            Assert.IsNotNull(savedCompany);
            Assert.AreNotSame(company, savedCompany);
            Assert.AreEqual(1, savedCompany.PersistenceVersion);
            Assert.AreEqual(companyCreationType == CompanyCreationType.NO_CHILDREN ? 0 : 2, savedCompany.Identifications.Count);

            return savedCompany;
        }
    }
}
