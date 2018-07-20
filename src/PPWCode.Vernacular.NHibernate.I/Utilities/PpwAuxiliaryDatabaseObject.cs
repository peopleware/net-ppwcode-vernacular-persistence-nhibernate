// Copyright 2017-2018 by PeopleWare n.v..
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Mapping;

using PPWCode.Vernacular.Exceptions.II;
using PPWCode.Vernacular.NHibernate.I.Interfaces;

namespace PPWCode.Vernacular.NHibernate.I.Utilities
{
    public abstract class PpwAuxiliaryDatabaseObject
        : AbstractAuxiliaryDatabaseObject,
          IPpwAuxiliaryDatabaseObject
    {
        protected static readonly string[] EmptyStringArray = new string[0];

        private Configuration _configuration;

        protected PpwAuxiliaryDatabaseObject(IPpwHbmMapping ppwHbmMapping)
        {
            PpwHbmMapping = ppwHbmMapping;
        }

        public IPpwHbmMapping PpwHbmMapping { get; }

        protected Configuration Configuration
            => _configuration;

        public void SetConfiguration(Configuration configuration)
        {
            _configuration = configuration;
        }

        protected virtual PersistentClass GetPersistentClassFor(Type type)
        {
            return
                Configuration
                    .ClassMappings
                    .Single(m => m.MappedClass == type);
        }

        protected virtual string GetTableNameFor(Type type)
            => GetPersistentClassFor(type)?.Table.Name;

        protected virtual string[] GetDiscriminatorColumnNameFor(Type type)
        {
            return
                GetPersistentClassFor(type)
                    ?.Discriminator
                    ?.ColumnIterator
                    .OfType<Column>()
                    .Select(c => c.Name)
                    .ToArray();
        }

        protected virtual string[] GetDiscriminatorValuesFor(Type type)
        {
            return
                Configuration
                    .ClassMappings
                    .SelectMany(classMapping => classMapping.DirectSubclasses, (classMapping, subclass) => new { classMapping, subclass })
                    .Where(t => (t.classMapping.MappedClass == type) && !t.subclass.MappedClass.IsAbstract)
                    .Select(t => t.subclass.DiscriminatorValue)
                    .Union(
                        Configuration
                            .ClassMappings
                            .Where(m => (m.MappedClass == type) && !m.MappedClass.IsAbstract)
                            .Select(m => m.DiscriminatorValue))
                    .ToArray();
        }

        protected virtual string[] GetColumnNames<TSource>(Expression<Func<TSource, object>> propertyLambda)
        {
            PropertyInfo propInfo = GetPropertyInfo(propertyLambda);
            if (propInfo != null)
            {
                PersistentClass persistentClass = GetPersistentClassFor(typeof(TSource));
                if (persistentClass != null)
                {
                    Property nhProperty =
                        persistentClass
                            .PropertyIterator
                            .SingleOrDefault(p => string.Equals(p.Name, propInfo.Name, StringComparison.Ordinal));
                    if (nhProperty != null)
                    {
                        return
                            nhProperty
                                .Value
                                .ColumnIterator
                                .OfType<Column>()
                                .Select(c => c.Name)
                                .ToArray();
                    }
                }
            }

            return EmptyStringArray;
        }

        protected virtual string[] GetIdentifierColumns(Type type)
        {
            PersistentClass persistentClass = GetPersistentClassFor(type);
            if (persistentClass != null)
            {
                return
                    persistentClass
                        .Identifier
                        .ColumnIterator
                        .OfType<Column>()
                        .Select(c => c.Name)
                        .ToArray();
            }

            return EmptyStringArray;
        }

        protected virtual PropertyInfo GetPropertyInfo<TSource>(Expression<Func<TSource, object>> propertyLambda)
        {
            Expression body = propertyLambda.Body;
            MemberExpression member = body as MemberExpression;
            UnaryExpression unary = body as UnaryExpression;
            if ((member == null) && !((unary != null) && ((member = unary.Operand as MemberExpression) != null)))
            {
                throw new ProgrammingError($"Expression \'{propertyLambda}\' does not refer to a property.");
            }

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (!(propInfo != null))
            {
                throw new ProgrammingError($"Expression \'{propertyLambda}\' refers to a field, not a property.");
            }

            Type type = typeof(TSource);
            if ((propInfo.DeclaringType != null) && !propInfo.DeclaringType.IsAssignableFrom(type))
            {
                throw new ProgrammingError($"Expression \'{propertyLambda}\' refers to a property that is not from type \'{{type}}\'.");
            }

            return propInfo;
        }

        protected virtual string QuoteColumnName(Dialect dialect, string identifier)
            => PpwHbmMapping.QuoteIdentifiers ? dialect.QuoteForColumnName(identifier) : identifier;

        protected virtual string QuoteTableName(Dialect dialect, string identifier)
            => PpwHbmMapping.QuoteIdentifiers ? dialect.QuoteForTableName(identifier) : identifier;

        protected virtual string QuoteSchemaName(Dialect dialect, string identifier)
            => PpwHbmMapping.QuoteIdentifiers ? dialect.QuoteForSchemaName(identifier) : identifier;
    }
}
