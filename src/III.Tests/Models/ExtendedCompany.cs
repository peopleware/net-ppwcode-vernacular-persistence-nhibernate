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

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using JetBrains.Annotations;

using PPWCode.Vernacular.Exceptions.IV;
using PPWCode.Vernacular.NHibernate.III.MappingByCode;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Tests.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class ExtendedCompany : AuditablePersistentObject<int>
    {
        [DataMember]
        private Company _company;

        public ExtendedCompany(int id)
            : base(id)
        {
        }

        public ExtendedCompany()
        {
        }

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
                        previousCompany.ExtendedCompany = null;
                    }

                    _company = value;
                    if (_company != null)
                    {
                        _company.ExtendedCompany = this;
                    }
                }
            }
        }

        [DataMember]
        [StringLength(200)]
        public virtual string ExtraData { get; set; }

        public override CompoundSemanticException WildExceptions()
        {
            CompoundSemanticException sce = base.WildExceptions();

            if (Company == null)
            {
                sce.AddElement(new PropertyException(this, nameof(Company), PropertyException.MandatoryMessage, null));
            }

            return sce;
        }

        [UsedImplicitly]
        public class ExtendedCompanyMapper : InsertAuditablePersistentObjectMapper<ExtendedCompany, int>
        {
            public ExtendedCompanyMapper()
            {
                Property(ec => ec.ExtraData);
                OneToOne(
                    ec => ec.Company,
                    m =>
                    {
                        m.Constrained(true);
                        m.ForeignKey(null);
                    });
            }
        }
    }
}
