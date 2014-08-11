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

using System.Data;

using NHibernate;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.NHibernate.I.Test;
using PPWCode.Vernacular.NHibernate.I.Tests.Models;

namespace PPWCode.Vernacular.NHibernate.I.Tests.IntegrationTests
{
    public abstract class CompanyRepositoryTests : NHibernateFixture
    {
        protected IRepository<Company, int> Repository { get; private set; }

        protected override void OnSetup()
        {
            base.OnSetup();

            Repository = new CompanyRepository(Session);
        }

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

        protected Company CreateCompany(CompanyCreationType companyCreationType)
        {
            Company savedCompany;
            Company company =
                new Company
                {
                    Name = "Peopleware NV"
                };

            using (ITransaction trans = Session.BeginTransaction(IsolationLevel.ReadCommitted))
            {
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

                savedCompany = Repository.MakePersistent(company);
                trans.Commit();
            }

            Session.Clear();

            Assert.AreNotSame(company, savedCompany);
            Assert.AreEqual(companyCreationType == CompanyCreationType.NO_CHILDREN ? 0 : 2, savedCompany.Identifications.Count);

            return savedCompany;
        }
    }
}