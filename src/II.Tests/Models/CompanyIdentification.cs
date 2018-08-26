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
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using PPWCode.Vernacular.NHibernate.II.MappingByCode;
using PPWCode.Vernacular.Persistence.III;

namespace PPWCode.Vernacular.NHibernate.II.Tests.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class CompanyIdentification : AuditablePersistentObject<int>
    {
        [DataMember]
        private Company _company;

        [DataMember]
        private Company _parentCompany;

        [Required]
        [StringLength(256)]
        [DataMember]
        public virtual string Identification { get; set; }

        [DataMember]
        public virtual int Number { get; set; }

        [Required]
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
                        previousCompany.RemoveIdentification(this);
                    }

                    _company = value;
                    _company?.AddIdentification(this);
                }
            }
        }

        public virtual Company ParentCompany
        {
            get => _parentCompany;
            set
            {
                if (_parentCompany != value)
                {
                    if (_parentCompany != null)
                    {
                        Company previousParentCompany = _parentCompany;
                        _parentCompany = null;
                        previousParentCompany.RemoveParentIdentification(this);
                    }

                    _parentCompany = value;
                    _parentCompany?.AddParentIdentification(this);
                }
            }
        }
    }

    public class CompanyIdentificationMapper : AuditablePersistentObjectMapper<CompanyIdentification, int>
    {
        public CompanyIdentificationMapper()
        {
            Property(ci => ci.Identification);
            Property(ci => ci.Number);
            ManyToOne(ci => ci.Company);
            ManyToOne(ci => ci.ParentCompany);
        }
    }
}
