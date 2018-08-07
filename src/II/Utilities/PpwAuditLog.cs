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
using System.Runtime.Serialization;

using JetBrains.Annotations;

namespace PPWCode.Vernacular.NHibernate.II.Utilities
{
    [Serializable]
    [DataContract]
    public class PpwAuditLog
    {
        public PpwAuditLog(
            [NotNull] string propertyName,
            [CanBeNull] string value)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            Value = value;
        }

        [DataMember]
        public string PropertyName { get; }

        [DataMember]
        public string Value { get; }
    }
}
