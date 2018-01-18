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
using System.Linq;
using System.Reflection;
using System.Text;

using NHibernate.Dialect;
using NHibernate.Engine;
using NHibernate.Mapping;
using NHibernate.Mapping.ByCode.Impl;

using PPWCode.Vernacular.NHibernate.I.Interfaces;

namespace PPWCode.Vernacular.NHibernate.I.Utilities
{
    public class PpwSubclassCheckConstraintsAuxiliaryDatabaseObject : PpwAuxiliaryDatabaseObject
    {
        public PpwSubclassCheckConstraintsAuxiliaryDatabaseObject(IPpwHbmMapping ppwHbmMapping)
            : base(ppwHbmMapping)
        {
        }

        protected virtual void AddCheckConstraintsFor(string tableName, string discriminatorColumnName, Subclass @class)
        {
            ICandidatePersistentMembersProvider membersProvider = PpwHbmMapping.MembersProvider;
            Dictionary<string, Property> persistentPropertyNames =
                @class
                    .PropertyIterator
                    .ToDictionary(p => p.Name, StringComparer.Ordinal);
            IEnumerable<MemberInfo> subEntityMembers = membersProvider.GetSubEntityMembers(@class.MappedClass, @class.MappedClass.BaseType);
            IDictionary<Property, MemberInfo> checkedProperties =
                subEntityMembers
                    .Where(p => persistentPropertyNames.ContainsKey(p.Name))
                    .ToDictionary(p => persistentPropertyNames[p.Name]);
            foreach (KeyValuePair<Property, MemberInfo> property in checkedProperties)
            {
            }

            foreach (Subclass subclass in @class.DirectSubclasses)
            {
                AddCheckConstraintsFor(tableName, discriminatorColumnName, subclass);
            }
        }

        public override string SqlCreateString(Dialect dialect, IMapping p, string defaultCatalog, string defaultSchema)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<PersistentClass> rootClassesWithSubclasses =
                Configuration
                    .ClassMappings
                    .Where(c => c.HasSubclasses);
            foreach (PersistentClass @class in rootClassesWithSubclasses)
            {
                string tableName = GetTableNameFor(@class.MappedClass);
                string discriminatorColumnName = GetDiscriminatorColumnNameFor(@class.MappedClass);
                foreach (Subclass subclass in @class.DirectSubclasses)
                {
                    AddCheckConstraintsFor(tableName, discriminatorColumnName, subclass);
                }
            }

            return sb.ToString();
        }

        public override string SqlDropString(Dialect dialect, string defaultCatalog, string defaultSchema)
        {
            StringBuilder sb = new StringBuilder();

            return sb.ToString();
        }
    }
}
