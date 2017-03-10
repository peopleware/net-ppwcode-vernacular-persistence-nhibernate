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

using System;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.NHibernate.I.Tests.Models;
using PPWCode.Vernacular.NHibernate.I.Tests.Repositories;

namespace PPWCode.Vernacular.NHibernate.I.Tests.IntegrationTests
{
    public abstract class BaseCompanyTests : BaseRepositoryTests<Company>
    {
        private Company m_CreatedCompany;

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

        protected Company CreatedCompany
        {
            get { return m_CreatedCompany; }
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            m_CreatedCompany = CreateCompany(CompanyCreationType.WITH_2_CHILDREN);
            SessionFactory.Statistics.Clear();
        }

        protected override void OnTeardown()
        {
            m_CreatedCompany = null;

            base.OnTeardown();
        }

        protected override Func<IRepository<Company, int>> RepositoryFactory
        {
            get { return () => new CompanyRepository(Session); }
        }

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

            Assert.AreNotSame(company, savedCompany);
            Assert.AreEqual(1, savedCompany.PersistenceVersion);
            Assert.AreEqual(companyCreationType == CompanyCreationType.NO_CHILDREN ? 0 : 2, savedCompany.Identifications.Count);

            return savedCompany;
        }

        protected Company CreateFailedCompany(CompanyCreationType companyCreationType)
        {
            Company company = CreateCompany(companyCreationType);

            // ReSharper disable once ObjectCreationAsStatement
            new FailedCompany
            {
                FailingDate = Now,
                Company = company
            };

            Company savedCompany = RunInsideTransaction(() => Repository.Merge(company), true);

            Assert.AreNotSame(company, savedCompany);
            Assert.IsNotNull(savedCompany.FailedCompany);
            Assert.IsTrue(savedCompany.IsFailed);

            return savedCompany;
        }
    }
}
