// Copyright 2015 by PeopleWare n.v..
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

using System.Linq;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.I.Tests.Models;

namespace PPWCode.Vernacular.NHibernate.I.Tests.IntegrationTests
{
    // ReSharper disable InconsistentNaming
    public class CompanyBiDirectionalityTests : CompanyRepositoryTests
    {
        [Test]
        public void Check_BiDirectionality_Add_Child_To_Parent()
        {
            Company company = CreateCompany(CompanyCreationType.NO_CHILDREN);

            // Add child
            CompanyIdentification companyIdentification =
                new CompanyIdentification
                {
                    Identification = "1"
                };
            company.AddIdentification(companyIdentification);
            Assert.AreEqual(1, company.Identifications.Count);
            foreach (CompanyIdentification identification in company.Identifications)
            {
                Assert.IsTrue(identification.IsTransient);
            }

            Company savedCompany = RunInsideTransaction(() => Repository.Merge(company), true);
            Assert.AreEqual(2, savedCompany.PersistenceVersion);
            Assert.AreEqual(1, savedCompany.Identifications.Count);
            foreach (CompanyIdentification identification in savedCompany.Identifications)
            {
                Assert.IsFalse(identification.IsTransient);
            }
        }

        [Test]
        public void Check_BiDirectionality_Add_Childs_To_Parent()
        {
            Company company = CreateCompany(CompanyCreationType.NO_CHILDREN);

            // Add child
            CompanyIdentification companyIdentification =
                new CompanyIdentification
                {
                    Identification = "1"
                };
            company.AddIdentification(companyIdentification);
            CompanyIdentification companyIdentification2 =
                new CompanyIdentification
                {
                    Identification = "1"
                };
            company.AddIdentification(companyIdentification2);
            Assert.AreEqual(2, company.Identifications.Count);
            foreach (CompanyIdentification identification in company.Identifications)
            {
                Assert.IsTrue(identification.IsTransient);
            }

            Company savedCompany = RunInsideTransaction(() => Repository.Merge(company), true);
            Assert.AreEqual(2, savedCompany.PersistenceVersion);
            Assert.AreEqual(2, savedCompany.Identifications.Count);
            foreach (CompanyIdentification identification in savedCompany.Identifications)
            {
                Assert.IsFalse(identification.IsTransient);
            }
        }

        [Test]
        public void Check_BiDirectionality_Attach_Parent_To_Child()
        {
            Company company = CreateCompany(CompanyCreationType.NO_CHILDREN);

            // Attach parent to child
            // ReSharper disable once ObjectCreationAsStatement
            new CompanyIdentification
            {
                Identification = "1",
                Company = company
            };
            Assert.AreEqual(1, company.Identifications.Count);
            foreach (CompanyIdentification identification in company.Identifications)
            {
                Assert.IsTrue(identification.IsTransient);
            }

            Company savedCompany = RunInsideTransaction(() => Repository.Merge(company), true);
            Assert.AreEqual(2, savedCompany.PersistenceVersion);
            Assert.AreEqual(1, savedCompany.Identifications.Count);
            foreach (CompanyIdentification identification in savedCompany.Identifications)
            {
                Assert.IsFalse(identification.IsTransient);
            }
        }

        [Test]
        public void Check_BiDirectionality_Attach_Parent_To_Child2()
        {
            Company company = CreateCompany(CompanyCreationType.NO_CHILDREN);

            // Attach parent to child
            // ReSharper disable once ObjectCreationAsStatement
            new CompanyIdentification
            {
                Identification = "1",
                Company = company
            };
            // ReSharper disable once ObjectCreationAsStatement
            new CompanyIdentification
            {
                Identification = "1",
                Company = company
            };
            Assert.AreEqual(2, company.Identifications.Count);
            foreach (CompanyIdentification identification in company.Identifications)
            {
                Assert.IsTrue(identification.IsTransient);
            }

            Company savedCompany = RunInsideTransaction(() => Repository.Merge(company), true);
            Assert.AreEqual(2, savedCompany.PersistenceVersion);
            Assert.AreEqual(2, savedCompany.Identifications.Count);
            foreach (CompanyIdentification identification in savedCompany.Identifications)
            {
                Assert.IsFalse(identification.IsTransient);
            }
        }

        [Test]
        public void Check_BiDirectionality_Detach_Parent_From_Child()
        {
            Company company = CreateCompany(CompanyCreationType.WITH_2_CHILDREN);

            Company updatedCompany =
                RunInsideTransaction(
                    () =>
                    {
                        Company mergedCompany = Repository.Merge(company);

                        CompanyIdentification companyIdentification =
                            mergedCompany
                                .Identifications
                                .SingleOrDefault(i => i.Identification == "1");
                        Assert.IsNotNull(companyIdentification);
                        companyIdentification.Company = null;

                        return mergedCompany;
                    },
                    true);

            Company selectedCompany = RunInsideTransaction(() => Repository.GetById(updatedCompany.Id), false);
            Assert.AreEqual(1, selectedCompany.Identifications.Count);
        }

        [Test]
        public void Check_BiDirectionality_Detach_Parent_From_Child2()
        {
            Company company = CreateCompany(CompanyCreationType.WITH_2_CHILDREN);

            Company updatedCompany =
                RunInsideTransaction(
                    () =>
                    {
                        Company mergedCompany = Repository.Merge(company);

                        foreach (CompanyIdentification identification in mergedCompany.Identifications.ToList())
                        {
                            identification.Company = null;
                        }

                        return mergedCompany;
                    },
                    true);

            Company selectedCompany = RunInsideTransaction(() => Repository.GetById(updatedCompany.Id), false);
            Assert.IsFalse(selectedCompany.Identifications.Any());
        }

        [Test]
        public void Check_BiDirectionality_Remove_Child_From_Parent()
        {
            Company company = CreateCompany(CompanyCreationType.WITH_2_CHILDREN);

            Company updatedCompany =
                RunInsideTransaction(
                    () =>
                    {
                        Company mergedCompany = Repository.Merge(company);

                        CompanyIdentification companyIdentification =
                            mergedCompany
                                .Identifications
                                .SingleOrDefault(i => i.Identification == "1");
                        Assert.IsNotNull(companyIdentification);
                        mergedCompany.RemoveIdentification(companyIdentification);

                        return mergedCompany;
                    },
                    true);

            Company selectedCompany = RunInsideTransaction(() => Repository.GetById(updatedCompany.Id), false);
            Assert.AreEqual(1, selectedCompany.Identifications.Count);
        }

        [Test]
        public void Check_BiDirectionality_Remove_Childs_From_Parent()
        {
            Company company = CreateCompany(CompanyCreationType.WITH_2_CHILDREN);

            Company updatedCompany =
                RunInsideTransaction(
                    () =>
                    {
                        Company mergedCompany = Repository.Merge(company);

                        foreach (CompanyIdentification identification in mergedCompany.Identifications.ToList())
                        {
                            mergedCompany.RemoveIdentification(identification);
                        }

                        return mergedCompany;
                    },
                    true);

            Company selectedCompany = RunInsideTransaction(() => Repository.GetById(updatedCompany.Id), false);
            Assert.IsFalse(selectedCompany.Identifications.Any());
        }

        [Test]
        public void Check_BiDirectionality_FailedCompany_Add_FailedCompany_To_Company()
        {
            Company company = CreateCompany(CompanyCreationType.NO_CHILDREN);

            Company updatedCompany =
                RunInsideTransaction(
                    () =>
                    {
                        Company mergedCompany = Repository.Merge(company);

                        FailedCompany failedCompany =
                            new FailedCompany
                            {
                                FailingDate = Now
                            };
                        mergedCompany.FailedCompany = failedCompany;

                        return mergedCompany;
                    },
                    true);

            Company selectedCompany = RunInsideTransaction(() => Repository.GetById(updatedCompany.Id), false);
            Assert.IsNotNull(selectedCompany.FailedCompany);
            Assert.IsTrue(selectedCompany.IsFailed);
        }

        [Test]
        public void Check_BiDirectionality_FailedCompany_Add_FailedCompany_To_Company2()
        {
            Company company = CreateCompany(CompanyCreationType.NO_CHILDREN);

            Company updatedCompany =
                RunInsideTransaction(
                    () =>
                    {
                        Company mergedCompany = Repository.Merge(company);

                        // ReSharper disable once ObjectCreationAsStatement
                        new FailedCompany
                        {
                            FailingDate = Now,
                            Company = mergedCompany
                        };

                        return mergedCompany;
                    },
                    true);

            Company selectedCompany = RunInsideTransaction(() => Repository.GetById(updatedCompany.Id), false);
            Assert.IsNotNull(selectedCompany.FailedCompany);
            Assert.IsTrue(selectedCompany.IsFailed);
        }

        [Test]
        public void Check_BiDirectionality_FailedCompany_Remove_FailedCompany_From_Company()
        {
            Company company = CreateFailedCompany(CompanyCreationType.NO_CHILDREN);

            Company updatedCompany =
                RunInsideTransaction(
                    () =>
                    {
                        Company mergedCompany = Repository.Merge(company);

                        FailedCompany failedCompany = mergedCompany.FailedCompany;
                        mergedCompany.FailedCompany = null;

                        // Due cascading delete isn't supported for one-to-one relation, 
                        // see https://nhibernate.jira.com/browse/NH-1262
                        FailedCompanyRepository.Delete(failedCompany);

                        return mergedCompany;
                    },
                    true);

            Company selectedCompany = RunInsideTransaction(() => Repository.GetById(updatedCompany.Id), false);
            Assert.IsNull(selectedCompany.FailedCompany);
            Assert.IsFalse(selectedCompany.IsFailed);
        }
    }
}