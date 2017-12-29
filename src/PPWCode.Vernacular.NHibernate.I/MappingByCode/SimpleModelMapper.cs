// Copyright 2017 by PeopleWare n.v..
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
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Impl;

using PPWCode.Vernacular.Exceptions.II;
using PPWCode.Vernacular.NHibernate.I.Interfaces;

namespace PPWCode.Vernacular.NHibernate.I.MappingByCode
{
    /// <summary>
    ///     Simple ModelMapper.
    ///     This code is based on <see cref="ConventionModelMapper" />
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

        protected virtual bool UseCamelCaseUnderScoreForDbObjects => false;

        protected virtual bool DynamicInsert => false;

        protected virtual bool DynamicUpdate => false;

        protected virtual int? BatchSize => null;

        protected virtual string DefaultSchemaName => null;

        protected virtual string DefaultCatalogName => null;

        protected virtual bool QuoteIdentifiers => false;

        public virtual bool CreateIndexForForeignKey => true;

        protected virtual KeyTypeEnum PrimaryKeyType => KeyTypeEnum.TYPE_ID;

        protected virtual KeyTypeEnum ForeignKeyType => KeyTypeEnum.TYPE_ID;

        protected virtual string GetTableName(IModelInspector modelInspector, Type type, bool? quoteIdentifier)
        {
            return ConditionalQuoteIdentifier(GetIdentifier(type.Name), quoteIdentifier ?? QuoteIdentifiers);
        }

        protected virtual string GetColumnName(IModelInspector modelInspector, PropertyPath member, bool? quoteIdentifier)
        {
            return ConditionalQuoteIdentifier(GetIdentifier(member.ToColumnName()), quoteIdentifier ?? QuoteIdentifiers);
        }

        protected virtual string GetForeignKeyColumnName(IModelInspector modelInspector, PropertyPath member, bool quoteIdentifier)
        {
            string keyColumnName;

            if (modelInspector.IsManyToManyItem(member.LocalMember))
            {
                keyColumnName = member.CollectionElementType().Name;
            }
            else if (modelInspector.IsManyToOne(member.LocalMember) || modelInspector.IsSet(member.LocalMember))
            {
                MemberInfo otherSideProperty = member.OneToManyOtherSideProperty();
                keyColumnName =
                    modelInspector.IsOneToMany(member.LocalMember) && otherSideProperty != null
                        ? otherSideProperty.GetPropertyOrFieldType().Name
                        : member.ToColumnName();
            }
            else
            {
                throw new NotSupportedException($"Unable to determine foreignkey column-name for {member}.");
            }

            if (ForeignKeyType == KeyTypeEnum.ID)
            {
                throw new ProgrammingError("Not supported for Foreign keys.");
            }

            return GetKeyColumnName(ForeignKeyType, keyColumnName, quoteIdentifier);
        }

        protected virtual string GetPrimaryKeyColumnName(IModelInspector modelInspector, Type type, bool quoteIdentifier)
        {
            return GetKeyColumnName(PrimaryKeyType, type.Name, quoteIdentifier);
        }

        protected virtual string GetKeyColumnName(KeyTypeEnum keyType, string keyColumnName, bool quoteIdentifier)
        {
            string result;
            switch (keyType)
            {
                case KeyTypeEnum.ID:
                    result = GetIdentifier("Id");
                    break;

                case KeyTypeEnum.TYPE_ID:
                    result = GetIdentifier(string.Concat(keyColumnName, "Id"));
                    break;

                case KeyTypeEnum.ID_TYPE:
                    result = GetIdentifier(string.Concat("Id", keyColumnName));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (quoteIdentifier)
            {
                result = QuoteIdentifier(result);
            }

            return result;
        }

        protected virtual IEnumerable<MemberInfo> VersionProperties(IModelInspector modelInspector, Type type)
        {
            if (type == null)
            {
                yield break;
            }

            Type currentType = type;
            while (currentType != null && currentType != typeof(object))
            {
                PropertyInfo[] properties = currentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (PropertyInfo property in properties.Where(modelInspector.IsVersion))
                {
                    yield return property;
                }

                currentType = currentType.BaseType;
            }

            IEnumerable<PropertyInfo> versionProperties =
                type
                    .GetInterfaces()
                    .SelectMany(@interface => @interface.GetProperties().Where(modelInspector.IsVersion));
            foreach (PropertyInfo property in versionProperties)
            {
                yield return property;
            }
        }

        public virtual string GetDiscriminatorColumnName(IModelInspector modelInspector, Type type, bool? quoteIdentifier)
        {
            return ConditionalQuoteIdentifier(GetIdentifier("Discriminator"), quoteIdentifier ?? QuoteIdentifiers);
        }

        public virtual object GetDiscriminatorValue(IModelInspector modelInspector, Type type)
        {
            return CamelCaseToUnderscore(type.Name);
        }

        public virtual string GetVersionColumnName(IModelInspector modelInspector, Type type, bool? quoteIdentifier)
        {
            return ConditionalQuoteIdentifier(GetIdentifier("PersistenceVersion"), quoteIdentifier ?? QuoteIdentifiers);
        }

        protected DefaultCandidatePersistentMembersProvider MembersProvider => m_MembersProvider;

        protected virtual bool DeclaredPolymorphicMatch(MemberInfo member, Func<MemberInfo, bool> declaredMatch)
        {
            return declaredMatch(member)
                   || member.GetMemberFromDeclaringClasses().Any(declaredMatch)
                   || member.GetPropertyFromInterfaces().Any(declaredMatch);
        }

        protected virtual MemberInfo PoidPropertyOrField(IModelInspector modelInspector, Type type)
        {
            IEnumerable<MemberInfo> poidCandidates = MembersProvider.GetEntityMembersForPoid(type);
            return poidCandidates.FirstOrDefault(mi => DeclaredPolymorphicMatch(mi, modelInspector.IsPersistentId));
        }

        protected virtual void NoPoidGuid(IModelInspector modelInspector, Type type, IClassAttributesMapper classCustomizer)
        {
            MemberInfo poidPropertyOrField = PoidPropertyOrField(modelInspector, type);
            if (ReferenceEquals(null, poidPropertyOrField))
            {
                classCustomizer.Id(null, idm => idm.Generator(Generators.Guid));
            }
        }

        protected virtual void NoSetterPoidToField(IModelInspector modelInspector, Type type, IClassAttributesMapper classCustomizer)
        {
            MemberInfo poidPropertyOrField = PoidPropertyOrField(modelInspector, type);
            if (poidPropertyOrField != null && MatchNoSetterProperty(poidPropertyOrField))
            {
                classCustomizer.Id(poidPropertyOrField, idm => idm.Access(Accessor.NoSetter));
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
            IEnumerable<MemberInfo> persistentProperties =
                MembersProvider
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
            IEnumerable<MemberInfo> persistentProperties =
                MembersProvider
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
            classCustomizer.DynamicInsert(DynamicInsert);
            classCustomizer.DynamicUpdate(DynamicUpdate);

            if (BatchSize != null && BatchSize.Value > 0)
            {
                classCustomizer.BatchSize(BatchSize.Value);
            }

            if (!string.IsNullOrWhiteSpace(DefaultCatalogName))
            {
                classCustomizer.Catalog(ConditionalQuoteIdentifier(DefaultCatalogName, QuoteIdentifiers));
            }

            if (!string.IsNullOrWhiteSpace(DefaultSchemaName))
            {
                classCustomizer.Schema(ConditionalQuoteIdentifier(DefaultSchemaName, QuoteIdentifiers));
            }

            classCustomizer.Table(GetTableName(modelInspector, type, QuoteIdentifiers));

            classCustomizer.Id(
                m =>
                {
                    m.Column(GetPrimaryKeyColumnName(modelInspector, type, QuoteIdentifiers));
                    m.Generator(Generators.HighLow);
                });

            if (modelInspector.IsTablePerClassHierarchy(type))
            {
                classCustomizer.Discriminator(m => m.Column(GetDiscriminatorColumnName(modelInspector, type, QuoteIdentifiers)));
                classCustomizer.DiscriminatorValue(GetDiscriminatorValue(modelInspector, type));
            }

            MemberInfo[] versionProperties = VersionProperties(modelInspector, type).ToArray();
            if (versionProperties.Length == 1)
            {
                classCustomizer.Version(versionProperties[0], m => m.Column(GetVersionColumnName(modelInspector, type, QuoteIdentifiers)));
            }
        }

        protected virtual bool IsMemberDeclaredInATablePerClassHierarchy(IModelInspector modelInspector, PropertyPath member)
        {
            Type declaredType = member.GetRootMember().DeclaringType;
            return modelInspector.IsTablePerClassHierarchy(declaredType) && !modelInspector.IsRootEntity(declaredType);
        }

        protected virtual bool IsRequired(IModelInspector modelInspector, PropertyPath member)
        {
            if (!IsMemberDeclaredInATablePerClassHierarchy(modelInspector, member))
            {
                bool required = IsPropertyPathPrimitive(member);
                if (!required)
                {
                    PropertyPath walker = member;
                    required = true;
                    do
                    {
                        required &=
                            walker
                                .LocalMember
                                .GetCustomAttributes()
                                .OfType<RequiredAttribute>()
                                .Any();
                        walker = walker.PreviousPath;
                    } while (required && walker != null);
                }

                return required;
            }

            return false;
        }

        protected virtual bool IsPropertyPathPrimitive(PropertyPath member)
        {
            Type memberType = member.MemberType();
            return memberType.IsPrimitive || memberType == typeof(DateTime);
        }

        protected override void OnBeforeMapProperty(IModelInspector modelInspector, PropertyPath member, IPropertyMapper propertyCustomizer)
        {
            propertyCustomizer.Column(GetColumnName(modelInspector, member, QuoteIdentifiers));

            if (IsRequired(modelInspector, member))
            {
                propertyCustomizer.NotNullable(true);
            }

            StringLengthAttribute stringLengthAttribute =
                member
                    .LocalMember
                    .GetCustomAttributes()
                    .OfType<StringLengthAttribute>()
                    .FirstOrDefault();
            if (stringLengthAttribute != null)
            {
                if (stringLengthAttribute.MaximumLength > 0)
                {
                    propertyCustomizer.Length(stringLengthAttribute.MaximumLength);
                }
                else
                {
                    propertyCustomizer.Type(NHibernateUtil.StringClob);
                }
            }
        }

        protected override void ModelMapperOnBeforeMapUnionSubclass(IModelInspector modelInspector, Type type, IUnionSubclassAttributesMapper unionSubclassCustomizer)
        {
            unionSubclassCustomizer.Table(GetTableName(modelInspector, type, QuoteIdentifiers));
        }

        protected override void OnBeforeMapJoinedSubclass(IModelInspector modelInspector, Type type, IJoinedSubclassAttributesMapper joinedSubclassCustomizer)
        {
            joinedSubclassCustomizer.Key(
                k =>
                {
                    k.Column(GetPrimaryKeyColumnName(modelInspector, type.BaseType ?? type, QuoteIdentifiers));
                    if (type.BaseType != null)
                    {
                        k.ForeignKey($"FK_{GetTableName(modelInspector, type, false)}_{GetTableName(modelInspector, type.BaseType, false)}");
                    }
                });

            joinedSubclassCustomizer.Table(GetTableName(modelInspector, type, QuoteIdentifiers));
        }

        protected override void OnBeforeMapManyToMany(IModelInspector modelInspector, PropertyPath member, IManyToManyMapper collectionRelationManyToManyCustomizer)
        {
            string columnName = GetForeignKeyColumnName(modelInspector, member, false);
            string tableName = GetTableNameForManyToMany(modelInspector, member, false);
            string foreignKeyName = $"FK_{tableName}_{columnName}";
            // TODO how to put an index on the second FK??
            //string indexName = string.Format("IX_FK_{0}_{1}", tableName, columnName);

            collectionRelationManyToManyCustomizer.Column(ConditionalQuoteIdentifier(columnName, QuoteIdentifiers));
            collectionRelationManyToManyCustomizer.ForeignKey(foreignKeyName);
        }

        protected virtual string GetTableNameForManyToMany(IModelInspector modelInspector, PropertyPath member, bool quoteIdentifier)
        {
            if (!modelInspector.IsManyToManyItem(member.LocalMember))
            {
                throw new ProgrammingError($"Member {member} must be a many-to-many-item.");
            }

            return ConditionalQuoteIdentifier(GetIdentifier(member.ManyToManyIntermediateTableName("To")), quoteIdentifier);
        }

        protected override void OnBeforeMapManyToOne(IModelInspector modelInspector, PropertyPath member, IManyToOneMapper propertyCustomizer)
        {
            string foreignKeyColumnName = GetForeignKeyColumnName(modelInspector, member, false);
            propertyCustomizer.Column(ConditionalQuoteIdentifier(foreignKeyColumnName, QuoteIdentifiers));
            string tableName = GetTableName(modelInspector, member.Owner(), false);
            propertyCustomizer.ForeignKey($"FK_{tableName}_{foreignKeyColumnName}");

            if (IsRequired(modelInspector, member))
            {
                propertyCustomizer.NotNullable(true);
            }

            if (CreateIndexForForeignKey)
            {
                propertyCustomizer.Index($"IX_FK_{tableName}_{foreignKeyColumnName}");
            }
        }

        protected override void OnBeforeMapOneToOne(IModelInspector modelInspector, PropertyPath member, IOneToOneMapper propertyCustomizer)
        {
            propertyCustomizer.ForeignKey($"FK_{GetTableName(modelInspector, member.Owner(), false)}_{GetIdentifier(member.ToColumnName())}");
        }

        protected override void OnBeforeMapSubclass(IModelInspector modelInspector, Type type, ISubclassAttributesMapper subclassCustomizer)
        {
            subclassCustomizer.DiscriminatorValue(GetDiscriminatorValue(modelInspector, type));
        }

        protected virtual void OnBeforeMappingCollectionConvention(IModelInspector modelinspector, PropertyPath member, ICollectionPropertiesMapper collectionPropertiesCustomizer)
        {
            if (modelinspector.IsManyToManyItem(member.LocalMember))
            {
                string tableName = GetTableNameForManyToMany(modelinspector, member, false);
                collectionPropertiesCustomizer.Table(ConditionalQuoteIdentifier(tableName, QuoteIdentifiers));
                collectionPropertiesCustomizer.Key(
                    k =>
                    {
                        string keyColumnName = GetKeyColumnName(ForeignKeyType, member.Owner().Name, QuoteIdentifiers);
                        k.Column(keyColumnName);
                    });
            }
            else if (modelinspector.IsSet(member.LocalMember))
            {
                // If otherside has many-to-one, make it inverse, if not specify foreign key on Key element
                MemberInfo oneToManyProperty = member.OneToManyOtherSideProperty();
                IEnumerable<MemberInfo> candidatesManyToOne =
                    MembersProvider
                        .GetRootEntityMembers(oneToManyProperty.DeclaringType)
                        .Where(modelinspector.IsManyToOne);
                if (candidatesManyToOne.Any(mi => mi.GetPropertyOrFieldType() == member.LocalMember.DeclaringType))
                {
                    collectionPropertiesCustomizer.Inverse(true);
                }
                else
                {
                    Contract.Assert(oneToManyProperty.DeclaringType != null, "otherSideProperty.DeclaringType != null");
                    collectionPropertiesCustomizer.Key(k => k.ForeignKey($"FK_{GetTableName(modelinspector, oneToManyProperty.DeclaringType, false)}_{GetIdentifier(oneToManyProperty.Name)}"));
                }

                string keyColumnName = GetForeignKeyColumnName(modelinspector, member, QuoteIdentifiers);
                collectionPropertiesCustomizer.Key(k => k.Column(keyColumnName));
            }

            if (BatchSize != null && BatchSize.Value > 0)
            {
                collectionPropertiesCustomizer.Fetch(CollectionFetchMode.Select);
                collectionPropertiesCustomizer.BatchSize(BatchSize.Value);
            }
        }

        protected virtual string GetIdentifier(string identifier)
        {
            return UseCamelCaseUnderScoreForDbObjects ? CamelCaseToUnderscore(identifier) : identifier;
        }

        protected virtual string ConditionalQuoteIdentifier(string identifier, bool quoteIdentifier)
        {
            return quoteIdentifier ? QuoteIdentifier(identifier) : identifier;
        }

        protected virtual string QuoteIdentifier(string identifier)
        {
            return $"`{identifier}`";
        }

        public static string CamelCaseToUnderscore(string camelCase)
        {
            const string Rgx = @"([A-Z]+)([A-Z][a-z])";
            const string Rgx2 = @"([a-z\d])([A-Z])";

            string result = Regex.Replace(camelCase, Rgx, "$1_$2");
            result = Regex.Replace(result, Rgx2, "$1_$2");
            return result.ToUpper();
        }

        public enum KeyTypeEnum
        {
            ID,
            TYPE_ID,
            ID_TYPE
        }
    }
}
