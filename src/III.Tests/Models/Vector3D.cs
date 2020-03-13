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
using System.Diagnostics;
using System.Runtime.Serialization;

using JetBrains.Annotations;

using NHibernate.Mapping.ByCode.Conformist;

using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Tests.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Vector3D
        : CivilizedObject,
          IPpwAuditLog,
          IEquatable<Vector3D>
    {
        private Vector3D()
        {
        }

        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        [DataMember]
        public virtual double X { get; }

        [DataMember]
        public virtual double Y { get; }

        [DataMember]
        public virtual double Z { get; }

        /// <inheritdoc />
        public bool Equals(Vector3D other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        /// <inheritdoc />
        [DataMember]
        public bool IsMultiLog
            => true;

        /// <inheritdoc />
        public IEnumerable<PpwAuditLog> GetMultiLogs(string propertyName)
        {
            yield return new PpwAuditLog($"{propertyName}.{nameof(X)}", X.ToString("G"));
            yield return new PpwAuditLog($"{propertyName}.{nameof(Y)}", Y.ToString("G"));
            yield return new PpwAuditLog($"{propertyName}.{nameof(Z)}", Z.ToString("G"));
        }

        /// <inheritdoc />
        public PpwAuditLog GetSingleLog(string propertyName)
            => throw new NotImplementedException();

        /// <inheritdoc />
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

            return Equals((Vector3D)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Vector3D left, Vector3D right)
            => Equals(left, right);

        public static bool operator !=(Vector3D left, Vector3D right)
            => !Equals(left, right);
    }

    public class Vector3DMapper : ComponentMapping<Vector3D>
    {
        public Vector3DMapper()
        {
            Property(v => v.X);
            Property(v => v.Y);
            Property(v => v.Z);
        }
    }

    public class Vector3DBuilder
    {
        private double _x;
        private double _y;
        private double _z;

        public Vector3DBuilder()
        {
        }

        public Vector3DBuilder([CanBeNull] Vector3D vector3D)
        {
            if (vector3D != null)
            {
                _x = vector3D.X;
                _y = vector3D.Y;
                _z = vector3D.Z;
            }
        }

        [DebuggerStepThrough]
        [NotNull]
        public Vector3DBuilder X(double x)
        {
            _x = x;
            return this;
        }

        [DebuggerStepThrough]
        [NotNull]
        public Vector3DBuilder Y(double y)
        {
            _y = y;
            return this;
        }

        [DebuggerStepThrough]
        [NotNull]
        public Vector3DBuilder Z(double z)
        {
            _z = z;
            return this;
        }

        [NotNull]
        public Vector3D Build()
            => this;

        [NotNull]
        public static implicit operator Vector3D(Vector3DBuilder builder)
            => new Vector3D(builder._x, builder._y, builder._z);
    }
}
