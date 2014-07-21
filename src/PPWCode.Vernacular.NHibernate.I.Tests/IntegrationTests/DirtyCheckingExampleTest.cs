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

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.I.Test;
using PPWCode.Vernacular.NHibernate.I.Tests.Models;

namespace PPWCode.Vernacular.NHibernate.I.Tests.IntegrationTests
{
    public class DirtyCheckingExampleTests : CompanyRepositoryTests
    {
        [Test]
        public void DirtyCheckingTest()
        {
            CompanyIdentification companyIdentification1 =
                new CompanyIdentification
                {
                    Identification = "1"
                };
            CompanyIdentification companyIdentification2 =
                new CompanyIdentification
                {
                    Identification = "2"
                };
            Company company =
                new Company
                {
                    Name = "Peopleware NV",
                    Identifications = new[] { companyIdentification1, companyIdentification2 }
                };
            Repository.Save(company);
            Session.Evict(company);

            /* Set Number to null (but Number is defined as int) */
            /*
            using (ISession session = NhConfigurator.SessionFactory.OpenSession())
            {
                using (ITransaction trans = session.BeginTransaction())
                {
                    session.CreateQuery(@"update CompanyIdentification set Number = null").ExecuteUpdate();
                    trans.Commit();
                }
            }
            */
            new DirtyChecking(NhConfigurator.Configuration, NhConfigurator.SessionFactory, Assert.Fail, Assert.Inconclusive).Test();
        }
    }
}