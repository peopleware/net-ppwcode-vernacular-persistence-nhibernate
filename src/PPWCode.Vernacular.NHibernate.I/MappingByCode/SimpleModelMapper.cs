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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Impl;

using PPWCode.Vernacular.NHibernate.I.Interfaces;

namespace PPWCode.Vernacular.NHibernate.I.MappingByCode
{
    /// <summary>
    ///     Simple ModelMapper.
    /// </summary>
    public abstract class SimpleModelMapper : ModelMapperBase
    {
        private readonly DefaultCandidatePersistentMembersProvider m_MembersProvider;

        protected SimpleModelMapper(IMappingAssemblies mappingAssemblies)
            : base(mappingAssemblies)
        {
            ModelMapper.BeforeMapSet += OnBeforeMappingCollectionConvention;
            ModelMapper.BeforeMapBag += OnBeforeMappingCollectionConvention;
            ModelMapper.BeforeMapList += OnBeforeMappingCollectionConvention;
            ModelMapper.BeforeMapIdBag += OnBeforeMappingCollectionConvention;
            ModelMapper.BeforeMapMap += OnBeforeMappingCollectionConvention;

            // Following code is from NHibernate.Mapping.ByCode.ConventionModelMapper class
            // It set correctly accces, and readonly attributes
            m_MembersProvider = new DefaultCandidatePersistentMembersProvider();

            ModelMapper.BeforeMapClass += NoPoidGuid;
            ModelMapper.BeforeMapClass += NoSetterPoidToField;

            ModelMapper.BeforeMapProperty += MemberToFieldAccessor;
            ModelMapper.BeforeMapProperty += MemberNoSetterToField;
            ModelMapper.BeforeMapProperty += MemberReadOnlyAccessor;

            ModelMapper.BeforeMapComponent += MemberToFieldAccessor;
            ModelMapper.BeforeMapComponent += MemberNoSetterToField;
            ModelMapper.BeforeMapComponent += MemberReadOnlyAccessor;
            ModelMapper.BeforeMapComponent += ComponentParentToFieldAccessor;
            ModelMapper.BeforeMapComponent += ComponentParentNoSetterToField;

            ModelMapper.BeforeMapBag += MemberToFieldAccessor;
            ModelMapper.BeforeMapIdBag += MemberToFieldAccessor;
            ModelMapper.BeforeMapSet += MemberToFieldAccessor;
            ModelMapper.BeforeMapMap += MemberToFieldAccessor;
            ModelMapper.BeforeMapList += MemberToFieldAccessor;

            ModelMapper.BeforeMapBag += MemberNoSetterToField;
            ModelMapper.BeforeMapIdBag += MemberNoSetterToField;
            ModelMapper.BeforeMapSet += MemberNoSetterToField;
            ModelMapper.BeforeMapMap += MemberNoSetterToField;
            ModelMapper.BeforeMapList += MemberNoSetterToField;

            ModelMapper.BeforeMapBag += MemberReadOnlyAccessor;
            ModelMapper.BeforeMapIdBag += MemberReadOnlyAccessor;
            ModelMapper.BeforeMapSet += MemberReadOnlyAccessor;
            ModelMapper.BeforeMapMap += MemberReadOnlyAccessor;
            ModelMapper.BeforeMapList += MemberReadOnlyAccessor;

            ModelMapper.BeforeMapManyToOne += MemberToFieldAccessor;
            ModelMapper.BeforeMapOneToOne += MemberToFieldAccessor;
            ModelMapper.BeforeMapAny += MemberToFieldAccessor;
            ModelMapper.BeforeMapManyToOne += MemberNoSetterToField;
            ModelMapper.BeforeMapOneToOne += MemberNoSetterToField;
            ModelMapper.BeforeMapAny += MemberNoSetterToField;
            ModelMapper.BeforeMapManyToOne += MemberReadOnlyAccessor;
            ModelMapper.BeforeMapOneToOne += MemberReadOnlyAccessor;
            ModelMapper.BeforeMapAny += MemberReadOnlyAccessor;
        }

        protected virtual bool UseCamelCaseUnderScoreForDbObjects
        {
            get { return false; }
        }

        protected DefaultCandidatePersistentMembersProvider MembersProvider
        {
            get { return m_MembersProvider; }
        }

        protected virtual void NoPoidGuid(IModelInspector modelInspector, Type type, IClassAttributesMapper classCustomizer)
        {
            MemberInfo poidPropertyOrField = MembersProvider.GetEntityMembersForPoid(type).FirstOrDefault(modelInspector.IsPersistentId);
            if (ReferenceEquals(null, poidPropertyOrField))
            {
                classCustomizer.Id(null, idm => idm.Generator(Generators.Guid));
            }
        }

        protected virtual void NoSetterPoidToField(IModelInspector modelInspector, Type type, IClassAttributesMapper classCustomizer)
        {
            MemberInfo poidPropertyOrField = MembersProvider.GetEntityMembersForPoid(type).FirstOrDefault(modelInspector.IsPersistentId);
            if (MatchNoSetterProperty(poidPropertyOrField))
            {
                classCustomizer.Id(idm => idm.Access(Accessor.NoSetter));
            }
        }

        protected virtual void MemberToFieldAccessor(IModelInspector modelInspector, PropertyPath member, IAccessorPropertyMapper propertyCustomizer)
        {
            if (MatchPropertyToField(member.LocalMember))
            {
                propertyCustomizer.Access(Accessor.Field);
            }
        }

        protected virtual void MemberNoSetterToField(IModelInspector modelInspector, PropertyPath member, IAccessorPropertyMapper propertyCustomizer)
        {
            if (MatchNoSetterProperty(member.LocalMember))
            {
                propertyCustomizer.Access(Accessor.NoSetter);
            }
        }

        protected virtual void MemberReadOnlyAccessor(IModelInspector modelInspector, PropertyPath member, IAccessorPropertyMapper propertyCustomizer)
        {
            if (MatchReadOnlyProperty(member.LocalMember))
            {
                propertyCustomizer.Access(Accessor.ReadOnly);
            }
        }

        protected virtual void ComponentParentToFieldAccessor(IModelInspector modelInspector, PropertyPath member, IComponentAttributesMapper componentMapper)
        {
            Type componentType = member.LocalMember.GetPropertyOrFieldType();
            IEnumerable<MemberInfo> persistentProperties = MembersProvider
                .GetComponentMembers(componentType)
                .Where(p => ModelInspector.IsPersistentProperty(p));

            MemberInfo parentReferenceProperty = GetComponentParentReferenceProperty(persistentProperties, member.LocalMember.ReflectedType);
            if (parentReferenceProperty != null && MatchPropertyToField(parentReferenceProperty))
            {
                componentMapper.Parent(parentReferenceProperty, cp => cp.Access(Accessor.Field));
            }
        }

        protected virtual void ComponentParentNoSetterToField(IModelInspector modelInspector, PropertyPath member, IComponentAttributesMapper componentMapper)
        {
            Type componentType = member.LocalMember.GetPropertyOrFieldType();
            IEnumerable<MemberInfo> persistentProperties = MembersProvider
                .GetComponentMembers(componentType)
                .Where(p => ModelInspector.IsPersistentProperty(p));

            MemberInfo parentReferenceProperty = GetComponentParentReferenceProperty(persistentProperties, member.LocalMember.ReflectedType);
            if (parentReferenceProperty != null && MatchNoSetterProperty(parentReferenceProperty))
            {
                componentMapper.Parent(parentReferenceProperty, cp => cp.Access(Accessor.NoSetter));
            }
        }

        protected bool MatchReadOnlyProperty(MemberInfo subject)
        {
            PropertyInfo property = subject as PropertyInfo;
            if (property == null)
            {
                return false;
            }

            if (CanReadCantWriteInsideType(property) || CanReadCantWriteInBaseType(property))
            {
                return PropertyToField.GetBackFieldInfo(property) == null;
            }

            return false;
        }

        protected bool CanReadCantWriteInsideType(PropertyInfo property)
        {
            return !property.CanWrite && property.CanRead && property.DeclaringType == property.ReflectedType;
        }

        protected bool CanReadCantWriteInBaseType(PropertyInfo property)
        {
            if (property.DeclaringType == property.ReflectedType)
            {
                return false;
            }

            PropertyInfo rfprop =
                property.DeclaringType != null
                    ? property
                          .DeclaringType
                          .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                          .SingleOrDefault(pi => pi.Name == property.Name)
                    : null;

            return rfprop != null && !rfprop.CanWrite && rfprop.CanRead;
        }

        protected bool MatchNoSetterProperty(MemberInfo subject)
        {
            PropertyInfo property = subject as PropertyInfo;
            if (property == null || property.CanWrite || !property.CanRead)
            {
                return false;
            }

            FieldInfo fieldInfo = PropertyToField.GetBackFieldInfo(property);
            if (fieldInfo != null)
            {
                return fieldInfo.FieldType == property.PropertyType;
            }

            return false;
        }

        protected bool MatchPropertyToField(MemberInfo subject)
        {
            PropertyInfo property = subject as PropertyInfo;
            if (property == null)
            {
                return false;
            }

            FieldInfo fieldInfo = PropertyToField.GetBackFieldInfo(property);
            return fieldInfo != null;
        }

        protected MemberInfo GetComponentParentReferenceProperty(IEnumerable<MemberInfo> persistentProperties, Type propertiesContainerType)
        {
            return ModelInspector.IsComponent(propertiesContainerType)
                       ? persistentProperties.FirstOrDefault(pp => pp.GetPropertyOrFieldType() == propertiesContainerType)
                       : null;
        }

        protected override void OnBeforeMapClass(IModelInspector modelInspector, Type type, IClassAttributesMapper classCustomizer)
        {
            classCustomizer.DynamicUpdate(true);
            classCustomizer.Id(m =>
                               {
                                   m.Column(GetIdentifier(string.Format("{0}Id", type.Name)));
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
                    k.Column(GetIdentifier(string.Format("{0}Id", type.Name)));
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
            propertyCustomizer.Column(GetIdentifier(string.Format("{0}Id", member.ToColumnName())));
            propertyCustomizer.ForeignKey(string.Format("FK_{0}_{1}", member.Owner().Name, member.ToColumnName()));
        }

        protected override void OnBeforeMapOneToOne(IModelInspector modelInspector, PropertyPath member, IOneToOneMapper propertyCustomizer)
        {
            propertyCustomizer.ForeignKey(string.Format("FK_{0}_{1}", member.Owner().Name, member.ToColumnName()));
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