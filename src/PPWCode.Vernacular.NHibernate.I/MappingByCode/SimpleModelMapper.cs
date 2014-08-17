// Copyright 2014 by PeopleWare n.v..
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
using System.Reflection;
using System.Text.RegularExpressions;

using NHibernate.Mapping.ByCode;

using PPWCode.Vernacular.NHibernate.I.Interfaces;

namespace PPWCode.Vernacular.NHibernate.I.MappingByCode
{
    /// <summary>
    ///     Simple ModelMapper
    /// </summary>
    public abstract class SimpleModelMapper : ModelMapperBase
    {
        protected SimpleModelMapper(IMappingAssemblies mappingAssemblies)
            : base(mappingAssemblies)
        {
            ModelMapper.BeforeMapSet += OnBeforeMappingCollectionConvention;
            ModelMapper.BeforeMapBag += OnBeforeMappingCollectionConvention;
            ModelMapper.BeforeMapList += OnBeforeMappingCollectionConvention;
            ModelMapper.BeforeMapIdBag += OnBeforeMappingCollectionConvention;
            ModelMapper.BeforeMapMap += OnBeforeMappingCollectionConvention;
        }

        protected virtual bool UseCamelCaseUnderScoreForDbObjects
        {
            get { return false; }
        }

        protected override void OnBeforeMapClass(IModelInspector modelInspector, Type type, IClassAttributesMapper classCustomizer)
        {
            classCustomizer.DynamicUpdate(true);
            classCustomizer.Id(m =>
                               {
                                   m.Column(GetIdentifier("Id"));
                                   m.Generator(Generators.HighLow);
                               });
            classCustomizer.Table(GetIdentifier(type.Name));
        }

        protected override void OnBeforeMapProperty(IModelInspector modelInspector, PropertyPath member, IPropertyMapper propertyCustomizer)
        {
            Type declaringType = member.LocalMember.DeclaringType;
            if (declaringType == null)
            {
                return;
            }

            propertyCustomizer.Column(GetIdentifier(member.ToColumnName()));

            // Getting type of reflected object
            Type propertyType;
            switch (member.LocalMember.MemberType)
            {
                case MemberTypes.Field:
                    propertyType = ((FieldInfo)member.LocalMember).FieldType;
                    break;
                case MemberTypes.Property:
                    propertyType = ((PropertyInfo)member.LocalMember).PropertyType;
                    break;
                default:
                    propertyType = null;
                    break;
            }

            if ((propertyType != null && propertyType.IsPrimitive)
                || propertyType == typeof(DateTime))
            {
                propertyCustomizer.NotNullable(true);
            }
        }

        protected override void ModelMapperOnBeforeMapUnionSubclass(IModelInspector modelInspector, Type type, IUnionSubclassAttributesMapper unionSubclassCustomizer)
        {
            unionSubclassCustomizer.Table(GetIdentifier(type.Name));
        }

        protected override void OnBeforeMapJoinedSubclass(IModelInspector modelInspector, Type type, IJoinedSubclassAttributesMapper joinedSubclassCustomizer)
        {
            joinedSubclassCustomizer.Key(
                k =>
                {
                    k.Column(GetIdentifier("Id"));
                    if (type.BaseType != null)
                    {
                        k.ForeignKey(string.Format("FK_{0}_{1}", type.Name, type.BaseType.Name));
                    }
                });
            joinedSubclassCustomizer.Table(GetIdentifier(type.Name));
        }

        protected override void OnBeforeMapManyToMany(IModelInspector modelInspector, PropertyPath member, IManyToManyMapper collectionRelationManyToManyCustomizer)
        {
            collectionRelationManyToManyCustomizer.Column(GetIdentifier(string.Format("{0}Id", member.CollectionElementType().Name)));
        }

        protected override void OnBeforeMapManyToOne(IModelInspector modelInspector, PropertyPath member, IManyToOneMapper propertyCustomizer)
        {
            propertyCustomizer.Column(GetIdentifier(string.Format("{0}Id", member.LocalMember.Name)));
            propertyCustomizer.ForeignKey(string.Format("FK_{0}_{1}", member.Owner().Name, member.LocalMember.Name));
        }

        protected override void OnBeforeMapOneToOne(IModelInspector modelInspector, PropertyPath member, IOneToOneMapper propertyCustomizer)
        {
            propertyCustomizer.ForeignKey(string.Format("FK_{0}_{1}", member.Owner().Name, member.LocalMember.Name));
        }

        protected virtual void OnBeforeMappingCollectionConvention(IModelInspector modelinspector, PropertyPath member, ICollectionPropertiesMapper collectionPropertiesCustomizer)
        {
            if (modelinspector.IsManyToMany(member.LocalMember))
            {
                collectionPropertiesCustomizer.Table(member.ManyToManyIntermediateTableName("To"));
            }

            collectionPropertiesCustomizer.Key(k => k.Column(GetIdentifier(DetermineKeyColumnName(modelinspector, member))));
        }

        protected virtual string GetIdentifier(string identifier)
        {
            return UseCamelCaseUnderScoreForDbObjects ? CamelCaseToUnderscore(identifier) : identifier;
        }

        public static string CamelCaseToUnderscore(string camelCase)
        {
            const string Rgx = @"([A-Z]+)([A-Z][a-z])";
            const string Rgx2 = @"([a-z\d])([A-Z])";

            string result = Regex.Replace(camelCase, Rgx, "$1_$2");
            result = Regex.Replace(result, Rgx2, "$1_$2");
            return result.ToUpper();
        }

        protected string DetermineKeyColumnName(IModelInspector inspector, PropertyPath member)
        {
            MemberInfo otherSideProperty = member.OneToManyOtherSideProperty();
            string name = inspector.IsOneToMany(member.LocalMember) && otherSideProperty != null
                              ? otherSideProperty.Name
                              : member.Owner().Name;
            return string.Format("{0}Id", name);
        }
    }
}