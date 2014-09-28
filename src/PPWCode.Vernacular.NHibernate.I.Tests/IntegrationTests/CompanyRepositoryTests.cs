// Copyright 2014 by PeopleWare n.v..
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
using System.Data;

using Moq;

using NHibernate;
using NHibernate.Cfg.MappingSchema;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.NHibernate.I.Test;
using PPWCode.Vernacular.NHibernate.I.Tests.Models;
using PPWCode.Vernacular.NHibernate.I.Tests.Models.Mapping;
using PPWCode.Vernacular.NHibernate.I.Utilities;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Tests.IntegrationTests
{
    public abstract class CompanyRepositoryTests : NHibernateSqlServerFixtureTest
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

        protected const string UserName = "Danny";

        protected readonly DateTime Now = DateTime.Now.ToUniversalTime();

        protected override string InitialCatalog
        {
            get { return "Test.PPWCode.Vernacular.NHibernate.I.Tests"; }
        }

        protected override HbmMapping GetHbmMapping()
        {
            IHbmMapping mapper = new TestsSimpleModelMapper(new TestsMappingAssemblies());
            return mapper.GetHbmMapping();
        }

        protected override ISession OpenSession()
        {
            Mock<IIdentityProvider> identityProvider = new Mock<IIdentityProvider>();
            identityProvider.Setup(ip => ip.IdentityName).Returns(UserName);

            Mock<ITimeProvider> timeProvider = new Mock<ITimeProvider>();
            timeProvider.Setup(tp => tp.Now).Returns(Now);

            AuditInterceptor<int> sessionLocalInterceptor = new AuditInterceptor<int>(identityProvider.Object, timeProvider.Object);
            return SessionFactory.OpenSession(sessionLocalInterceptor);
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            Repository = new CompanyRepository(Session);
            FailedCompanyRepository = new FailedCompanyRepository(Session);
        }

        protected IRepository<Company, int> Repository { get; private set; }

        protected IRepository<FailedCompany, int> FailedCompanyRepository { get; private set; }

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

            Company savedCompany;
            using (ITransaction trans = Session.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                savedCompany = Repository.Merge(company);
                trans.Commit();
            }

            Session.Clear();

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

            Company savedCompany;
            using (ITransaction trans = Session.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                savedCompany = Repository.Merge(company);
                trans.Commit();
            }

            Session.Clear();

            Assert.AreNotSame(company, savedCompany);
            Assert.IsNotNull(savedCompany.FailedCompany);
            Assert.IsTrue(savedCompany.IsFailed);

            return savedCompany;
        }
    }
}