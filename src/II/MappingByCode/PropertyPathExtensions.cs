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
using System.Linq;
using System.Reflection;

using JetBrains.Annotations;

using NHibernate.Mapping.ByCode;

namespace PPWCode.Vernacular.NHibernate.II.MappingByCode
{
    /// <summary>
    ///     Code found on https://gist.github.com/NOtherDev/1569982.
    /// </summary>
    public static class PropertyPathExtensions
    {
        public static Type Owner([NotNull] this PropertyPath member)
            => member.GetRootMember().DeclaringType;

        [CanBeNull]
        public static Type CollectionElementType([NotNull] this PropertyPath member)
            => member.LocalMember.GetPropertyOrFieldType().DetermineCollectionElementOrDictionaryValueType();

        [NotNull]
        public static IEnumerable<MemberInfo> OneToManyOtherSideProperties([NotNull] this PropertyPath member)
            => member.CollectionElementType().GetAllPropertiesOfType(member.Owner());

        [NotNull]
        public static string ManyToManyIntermediateTableName([NotNull] this PropertyPath member, [NotNull] string manyToManyIntermediateTableInfix)
        {
            return string.Join(manyToManyIntermediateTableInfix, member.ManyToManySidesNames().OrderBy(x => x));
        }

        [NotNull]
        public static Type MemberType([NotNull] this PropertyPath member)
            => member.LocalMember.GetPropertyOrFieldType();

        [NotNull]
        private static IEnumerable<string> ManyToManySidesNames([NotNull] this PropertyPath member)
        {
            yield return member.Owner().Name;
            Type collectionElementType = member.CollectionElementType();
            if (collectionElementType != null)
            {
                yield return collectionElementType.Name;
            }
        }

        [NotNull]
        public static IEnumerable<MemberInfo> GetAllPropertiesOfType([CanBeNull] this Type propertyContainerType, [CanBeNull] Type propertyType)
            => GetAllPropertiesOfType(propertyContainerType, propertyType, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

        [NotNull]
        public static IEnumerable<MemberInfo> GetAllPropertiesOfType([CanBeNull] this Type propertyContainerType, [CanBeNull] Type propertyType, [NotNull] Func<PropertyInfo, bool> acceptPropertyClauses)
            => GetAllPropertiesOfType(propertyContainerType, propertyType, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, acceptPropertyClauses);

        [NotNull]
        public static IEnumerable<MemberInfo> GetAllPropertiesOfType([CanBeNull] this Type propertyContainerType, [CanBeNull] Type propertyType, BindingFlags bindingFlags)
        {
            return GetAllPropertiesOfType(propertyContainerType, propertyType, bindingFlags, x => true);
        }

        [NotNull]
        public static IEnumerable<MemberInfo> GetAllPropertiesOfType([CanBeNull] this Type propertyContainerType, [CanBeNull] Type propertyType, BindingFlags bindingFlags, [NotNull] Func<PropertyInfo, bool> acceptPropertyClauses)
        {
            if ((propertyContainerType == null) || (propertyType == null))
            {
                yield break;
            }

            IEnumerable<PropertyInfo> candidatePropertyInfos =
                propertyContainerType
                    .GetProperties(bindingFlags)
                    .Where(p => acceptPropertyClauses(p)
                                && (propertyType == p.PropertyType));
            foreach (PropertyInfo propertyInfo in candidatePropertyInfos)
            {
                yield return propertyInfo;
            }
        }
    }
}
