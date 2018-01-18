// Copyright 2018 by PeopleWare n.v..
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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using NHibernate.Mapping.ByCode.Conformist;

using PPWCode.Vernacular.NHibernate.I.Utilities;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Address
        : IEquatable<Address>,
          IPpwAuditLog
    {
        public bool Equals(Address other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Street, other.Street, StringComparison.CurrentCulture)
                   && string.Equals(Number, other.Number, StringComparison.CurrentCulture)
                   && string.Equals(Box, other.Box, StringComparison.CurrentCulture)
                   && Equals(Country, other.Country);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((Address)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Street != null ? StringComparer.CurrentCulture.GetHashCode(Street) : 0;
                hashCode = (hashCode * 397) ^ (Number != null ? StringComparer.CurrentCulture.GetHashCode(Number) : 0);
                hashCode = (hashCode * 397) ^ (Box != null ? StringComparer.CurrentCulture.GetHashCode(Box) : 0);
                hashCode = (hashCode * 397) ^ (Country != null ? Country.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Address left, Address right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Address left, Address right)
        {
            return !Equals(left, right);
        }

        [DataMember]
        [Required]
        [StringLength(128)]
        public virtual string Street { get; set; }

        [DataMember]
        [Required]
        [StringLength(16)]
        public virtual string Number { get; set; }

        [DataMember]
        [StringLength(16)]
        public virtual string Box { get; set; }

        [DataMember]
        public virtual Country Country { get; set; }

        public bool IsMultiLog
            => true;

        public IEnumerable<PpwAuditLog> GetMultiLogs(string propertyName)
        {
            yield return new PpwAuditLog($"{propertyName}.{nameof(Street)}", Street);
            yield return new PpwAuditLog($"{propertyName}.{nameof(Number)}", Number);
            yield return new PpwAuditLog($"{propertyName}.{nameof(Box)}", Box);
            yield return new PpwAuditLog($"{propertyName}.{nameof(Country)}", Country?.Id.ToString());
        }

        public PpwAuditLog GetSingleLog(string propertyName)
        {
            throw new NotImplementedException();
        }
    }

    public class AddressMapper : ComponentMapping<Address>
    {
        public AddressMapper()
        {
            Property(a => a.Street);
            Property(a => a.Number);
            Property(a => a.Box);
            ManyToOne(a => a.Country);
        }
    }
}
