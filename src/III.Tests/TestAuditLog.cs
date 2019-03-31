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

using NHibernate.Mapping.ByCode;

using PPWCode.Vernacular.NHibernate.III.MappingByCode;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Tests
{
    public class TestIntAuditLog : AuditLog<int>
    {
    }

    public class TestLongAuditLog : AuditLog<long>
    {
    }

    public class TestGuidAuditLog : AuditLog<Guid>
    {
    }

    public class TestIntAuditLogMapper : AuditLogMapper<TestIntAuditLog, int>
    {
    }

    public class TestLongAuditLogMapper : AuditLogMapper<TestLongAuditLog, long>
    {
    }

    public class TestGuidAuditLogMapper : AuditLogMapper<TestGuidAuditLog, Guid>
    {
        public TestGuidAuditLogMapper()
        {
            Id(c => c.Id, m => m.Generator(Generators.Guid));
        }
    }
}
