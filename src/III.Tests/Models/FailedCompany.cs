// Copyright 2017 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Runtime.Serialization;

using JetBrains.Annotations;

using NHibernate.Mapping.ByCode;

using PPWCode.Vernacular.Exceptions.IV;
using PPWCode.Vernacular.NHibernate.III.MappingByCode;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Tests.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    [AuditLog(AuditLogAction = AuditLogActionEnum.ALL)]
    public class FailedCompany : InsertAuditablePersistentObject<int>
    {
        [DataMember]
        private Company _company;

        [DataMember]
        public virtual DateTime FailingDate { get; set; }

        public virtual Company Company
        {
            get => _company;
            set
            {
                if (_company != value)
                {
                    if (_company != null)
                    {
                        Company previousCompany = _company;
                        _company = null;
                        previousCompany.FailedCompany = null;
                    }

                    _company = value;
                    if (_company != null)
                    {
                        _company.FailedCompany = this;
                    }
                }
            }
        }

        public override CompoundSemanticException WildExceptions()
        {
            CompoundSemanticException sce = base.WildExceptions();

            if (Company == null)
            {
                sce.AddElement(new PropertyException(this, nameof(Company), PropertyException.MandatoryMessage, null));
            }

            return sce;
        }
    }

    [UsedImplicitly]
    public class FailedCompanyMapper : InsertAuditablePersistentObjectMapper<FailedCompany, int>
    {
        public FailedCompanyMapper()
        {
            Id(fc => fc.Id, m => m.Generator(Generators.Foreign<FailedCompany>(fc => fc.Company)));
            Property(fc => fc.FailingDate);
            OneToOne(fc => fc.Company, m => m.Constrained(true));
        }
    }
}
