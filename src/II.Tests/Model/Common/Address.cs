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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.Serialization;

using JetBrains.Annotations;

using NHibernate.Mapping.ByCode.Conformist;

using PPWCode.Vernacular.Exceptions.III;
using PPWCode.Vernacular.Persistence.III;

namespace PPWCode.Vernacular.NHibernate.II.Tests.Model.Common
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Address
        : CivilizedObject,
          IEquatable<Address>,
          IPpwAuditLog
    {
        private Address()
        {
        }

        public Address(
            [NotNull] string street,
            [NotNull] string number,
            [CanBeNull] string box,
            [CanBeNull] Country country)
        {
            Street = street;
            Number = number;
            Box = box;
            Country = country;
        }

        [DataMember]
        [Required]
        [StringLength(128)]
        public virtual string Street { get; }

        [DataMember]
        [Required]
        [StringLength(16)]
        public virtual string Number { get; }

        [DataMember]
        [StringLength(16)]
        public virtual string Box { get; }

        [DataMember]
        public virtual Country Country { get; }

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
            => throw new NotImplementedException();

        public override CompoundSemanticException WildExceptions()
        {
            CompoundSemanticException cse = base.WildExceptions();

            if (Country != null)
            {
                foreach (SemanticException wildException in Country.WildExceptions().Elements)
                {
                    cse.AddElement(wildException);
                }
            }

            return cse;
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
            => Equals(left, right);

        public static bool operator !=(Address left, Address right)
            => !Equals(left, right);
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

    public class AddressBuilder
    {
        private string _box;
        private Country _country;
        private string _number;
        private string _street;

        public AddressBuilder()
        {
        }

        public AddressBuilder([CanBeNull] Address address)
        {
            if (address != null)
            {
                _street = address.Street;
                _number = address.Number;
                _box = address.Box;
                _country = address.Country;
            }
        }

        [DebuggerStepThrough]
        [NotNull]
        public AddressBuilder Street([NotNull] string street)
        {
            _street = street;
            return this;
        }

        [DebuggerStepThrough]
        [NotNull]
        public AddressBuilder Number([NotNull] string number)
        {
            _number = number;
            return this;
        }

        [DebuggerStepThrough]
        [NotNull]
        public AddressBuilder Box([CanBeNull] string box)
        {
            _box = box;
            return this;
        }

        [DebuggerStepThrough]
        [NotNull]
        public AddressBuilder Country([CanBeNull] Country country)
        {
            _country = country;
            return this;
        }

        [DebuggerStepThrough]
        [NotNull]
        public Address Build()
            => this;

        [NotNull]
        public static implicit operator Address(AddressBuilder builder)
            => new Address(builder._street, builder._number, builder._box, builder._country);
    }
}
