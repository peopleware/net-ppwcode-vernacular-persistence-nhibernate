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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using JetBrains.Annotations;

using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Mapping;

using PPWCode.Vernacular.Exceptions.III;

namespace PPWCode.Vernacular.NHibernate.II
{
    public abstract class PpwAuxiliaryDatabaseObject
        : AbstractAuxiliaryDatabaseObject,
          IPpwAuxiliaryDatabaseObject
    {
        protected static readonly string[] EmptyStringArray = new string[0];

        private Configuration _configuration;

        protected PpwAuxiliaryDatabaseObject([NotNull] IPpwHbmMapping ppwHbmMapping)
        {
            PpwHbmMapping = ppwHbmMapping;
        }

        [NotNull]
        public IPpwHbmMapping PpwHbmMapping { get; }

        protected Configuration Configuration
            => _configuration;

        public void SetConfiguration(Configuration configuration)
        {
            _configuration = configuration;
        }

        [CanBeNull]
        protected virtual PersistentClass GetPersistentClassFor([NotNull] Type type)
        {
            return
                Configuration
                    .ClassMappings
                    .SingleOrDefault(m => m.MappedClass == type);
        }

        [CanBeNull]
        protected virtual string GetTableNameFor([NotNull] Type type)
            => GetPersistentClassFor(type)?.Table.Name;

        [NotNull]
        protected virtual string[] GetDiscriminatorColumnNameFor([NotNull] Type type)
        {
            return
                GetPersistentClassFor(type)
                    ?.Discriminator
                    ?.ColumnIterator
                    .OfType<Column>()
                    .Select(c => c.Name)
                    .ToArray()
                ?? EmptyStringArray;
        }

        [NotNull]
        protected virtual string[] GetDiscriminatorValuesFor([NotNull] Type type)
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

        [NotNull]
        protected virtual string[] GetColumnNames<TSource>([NotNull] Expression<Func<TSource, object>> propertyLambda)
        {
            PropertyInfo propInfo = GetPropertyInfo(propertyLambda);
            PersistentClass persistentClass = GetPersistentClassFor(typeof(TSource));
            Property nhProperty =
                persistentClass
                    ?.PropertyIterator
                    .SingleOrDefault(p => string.Equals(p.Name, propInfo.Name, StringComparison.Ordinal));
            return
                nhProperty != null
                    ? nhProperty
                        .Value
                        .ColumnIterator
                        .OfType<Column>()
                        .Select(c => c.Name)
                        .ToArray()
                    : EmptyStringArray;
        }

        [NotNull]
        protected virtual string[] GetIdentifierColumns([NotNull] Type type)
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

        [NotNull]
        protected virtual PropertyInfo GetPropertyInfo<TSource>([NotNull] Expression<Func<TSource, object>> propertyLambda)
        {
            Expression body = propertyLambda.Body;
            MemberExpression member = body as MemberExpression;
            if ((member == null) && !(body is UnaryExpression unary && ((member = unary.Operand as MemberExpression) != null)))
            {
                throw new ProgrammingError($"Expression \'{propertyLambda}\' does not refer to a property.");
            }

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
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

        [ContractAnnotation("identifier:null => null; identifier:notnull => notnull")]
        protected virtual string QuoteColumnName([NotNull] Dialect dialect, string identifier)
            => PpwHbmMapping.QuoteIdentifiers ? dialect.QuoteForColumnName(identifier) : identifier;

        [ContractAnnotation("identifier:null => null; identifier:notnull => notnull")]
        protected virtual string QuoteTableName([NotNull] Dialect dialect, string identifier)
            => PpwHbmMapping.QuoteIdentifiers ? dialect.QuoteForTableName(identifier) : identifier;

        [ContractAnnotation("identifier:null => null; identifier:notnull => notnull")]
        protected virtual string QuoteSchemaName([NotNull] Dialect dialect, string identifier)
            => PpwHbmMapping.QuoteIdentifiers ? dialect.QuoteForSchemaName(identifier) : identifier;
    }
}
