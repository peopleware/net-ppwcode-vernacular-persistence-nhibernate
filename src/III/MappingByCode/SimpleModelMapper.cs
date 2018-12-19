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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using JetBrains.Annotations;

using NHibernate;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Impl;

using PPWCode.Vernacular.Exceptions.IV;

namespace PPWCode.Vernacular.NHibernate.III.MappingByCode
{
    /// <summary>
    ///     Simple ModelMapper.
    ///     This code is based on <see cref="ConventionModelMapper" />
    /// </summary>
    public abstract class SimpleModelMapper : ModelMapperBase
    {
        public enum KeyTypeEnum
        {
            ID,
            TYPE_ID,
            ID_TYPE
        }

        private IDictionary<Type, CachedEntityType> _cachedEntityTypes;

        private PropertyInfo _mapPropertyInfo;

        protected SimpleModelMapper([NotNull] IMappingAssemblies mappingAssemblies)
            : base(mappingAssemblies)
        {
            ModelMapper.BeforeMapSet += OnBeforeMappingCollectionConvention;
            ModelMapper.BeforeMapBag += OnBeforeMappingCollectionConvention;
            ModelMapper.BeforeMapList += OnBeforeMappingCollectionConvention;
            ModelMapper.BeforeMapIdBag += OnBeforeMappingCollectionConvention;
            ModelMapper.BeforeMapMap += OnBeforeMappingCollectionConvention;

            MembersProvider = new DefaultCandidatePersistentMembersProvider();

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

        public override bool UseCamelCaseUnderScoreForDbObjects
            => false;

        protected virtual bool DynamicInsert
            => false;

        protected virtual bool DynamicUpdate
            => false;

        protected virtual int? ClassBatchSize
            => null;

        protected virtual int? CollectionBatchSize
            => null;

        protected virtual string DefaultSchemaName
            => null;

        protected virtual string DefaultCatalogName
            => null;

        public override bool QuoteIdentifiers
            => false;

        public virtual bool CreateIndexForForeignKey
            => true;

        protected virtual bool AdjustColumnForForeignGenerator
            => false;

        protected virtual KeyTypeEnum PrimaryKeyType
            => KeyTypeEnum.TYPE_ID;

        protected virtual KeyTypeEnum ForeignKeyType
            => KeyTypeEnum.TYPE_ID;

        [NotNull]
        protected virtual string DefaultRegionNameForCachedEntities { get; } = "common";

        [NotNull]
        protected virtual CacheUsage DefaultCacheUsageForCachedEntities { get; } = CacheUsage.ReadWrite;

        protected virtual IEnumerable<CachedEntityType> GetEntityTypesToCache
        {
            get { yield break; }
        }

        protected IDictionary<Type, CachedEntityType> CachedEntityTypes
            => _cachedEntityTypes ?? (_cachedEntityTypes = GetEntityTypesToCache.ToDictionary(ce => ce.Type));

        public override ICandidatePersistentMembersProvider MembersProvider { get; }

        [CanBeNull]
        protected PropertyInfo MapDocPropertyInfo
            => _mapPropertyInfo
               ?? (_mapPropertyInfo = typeof(ClassMapper).GetProperty("MapDoc", BindingFlags.Instance | BindingFlags.NonPublic));

        [NotNull]
        protected virtual string DefaultDiscriminatorColumnName
            => "Discriminator";

        [NotNull]
        protected virtual string DefaultVersionColumnName
            => "PersistenceVersion";

        [CanBeNull]
        protected virtual string GetTableName(
            [NotNull] IModelInspector modelInspector,
            [NotNull] Type type,
            bool? quoteIdentifier)
            => ConditionalQuoteIdentifier(GetIdentifier(type.Name), quoteIdentifier);

        [CanBeNull]
        protected virtual string GetTableNameForManyToMany(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            bool? quoteIdentifier)
        {
            if (!modelInspector.IsManyToManyItem(member.LocalMember))
            {
                throw new ProgrammingError($"Member {member} must be a many-to-many-item.");
            }

            return ConditionalQuoteIdentifier(GetIdentifier(member.ManyToManyIntermediateTableName("To")), quoteIdentifier);
        }

        [CanBeNull]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "not null")]
        protected virtual string GetColumnName(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            bool? quoteIdentifier)
            => ConditionalQuoteIdentifier(GetIdentifier(member.ToColumnName()), quoteIdentifier);

        [NotNull]
        protected virtual string GetForeignKeyColumnName(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            bool? quoteIdentifier)
        {
            string keyColumnName;

            if (modelInspector.IsManyToManyItem(member.LocalMember))
            {
                keyColumnName = member.CollectionElementType()?.Name;
                if (keyColumnName == null)
                {
                    throw new ProgrammingError($"Unable to determine the keyColumnName, in case of a m-n relation, for {member.LocalMember.Name}");
                }
            }
            else if (modelInspector.IsManyToOne(member.LocalMember)
                     || modelInspector.IsSet(member.LocalMember)
                     || modelInspector.IsBag(member.LocalMember))
            {
                MemberInfo otherSideProperty = GetOneToManyOtherSideProperty(member);
                keyColumnName =
                    otherSideProperty != null
                        ? otherSideProperty.Name
                        : member.ToColumnName();
            }
            else
            {
                throw new NotSupportedException($"Unable to determine then foreign key column-name for {member}.");
            }

            if (ForeignKeyType == KeyTypeEnum.ID)
            {
                throw new ProgrammingError("Not supported for Foreign keys.");
            }

            return GetKeyColumnName(ForeignKeyType, keyColumnName, quoteIdentifier);
        }

        [CanBeNull]
        protected virtual MemberInfo GetOneToManyOtherSideProperty([NotNull] PropertyPath member)
        {
            List<MemberInfo> otherSideProperties =
                member
                    .OneToManyOtherSideProperties()
                    .ToList();
            if (otherSideProperties.Count == 0)
            {
                return null;
            }

            if (otherSideProperties.Count == 1)
            {
                return otherSideProperties.First();
            }

            OtherSidePropertyName otherSidePropertyNameAttribute =
                member
                    .LocalMember
                    .GetCustomAttributes(typeof(OtherSidePropertyName))
                    .OfType<OtherSidePropertyName>()
                    .SingleOrDefault();
            if (otherSidePropertyNameAttribute != null)
            {
                MemberInfo otherSideProperty =
                    otherSideProperties
                        .SingleOrDefault(p => p.Name == otherSidePropertyNameAttribute.PropertyName);
                if (otherSideProperty != null)
                {
                    return otherSideProperty;
                }
            }

            return GetOneToManyOtherSideProperty(member, otherSideProperties);
        }

        [CanBeNull]
        protected virtual MemberInfo GetOneToManyOtherSideProperty([NotNull] PropertyPath member, [NotNull] IList<MemberInfo> otherSideProperties)
            => throw new ProgrammingError($"Unable to map other side property for {member.ToColumnName()}.");

        [NotNull]
        protected virtual string GetPrimaryKeyColumnName(
            [NotNull] IModelInspector modelInspector,
            [NotNull] Type type,
            bool? quoteIdentifier)
            => GetKeyColumnName(PrimaryKeyType, type.Name, quoteIdentifier);

        [NotNull]
        protected virtual string GetKeyColumnName(
            KeyTypeEnum keyType,
            [NotNull] string keyColumnName,
            bool? quoteIdentifier)
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

            return ConditionalQuoteIdentifier(result, quoteIdentifier);
        }

        [NotNull]
        protected virtual IEnumerable<MemberInfo> VersionProperties(
            [NotNull] IModelInspector modelInspector,
            [NotNull] Type type)
        {
            Type walker = type;
            while ((walker != null) && (walker != typeof(object)))
            {
                IEnumerable<PropertyInfo> properties =
                    walker
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                        .Where(modelInspector.IsVersion);
                foreach (PropertyInfo property in properties.Where(modelInspector.IsVersion))
                {
                    yield return property;
                }

                walker = walker.BaseType;
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

        [NotNull]
        public virtual string GetDiscriminatorColumnName(
            [NotNull] IModelInspector modelInspector,
            [NotNull] Type type,
            bool? quoteIdentifier)
            => ConditionalQuoteIdentifier(GetIdentifier(DefaultDiscriminatorColumnName), quoteIdentifier);

        [NotNull]
        public virtual object GetDiscriminatorValue(
            [NotNull] IModelInspector modelInspector,
            [NotNull] Type type)
        {
            string discriminatorValue = type.Name;
            if (type.IsGenericType
                && type.GenericTypeArguments[0].IsEnum
                && discriminatorValue.EndsWith("Enum"))
            {
                discriminatorValue = discriminatorValue.Substring(0, discriminatorValue.Length - "Enum".Length);
            }

            return CamelCaseToUnderscore(discriminatorValue);
        }

        [NotNull]
        public virtual string GetVersionColumnName(
            [NotNull] IModelInspector modelInspector,
            [NotNull] Type type,
            bool? quoteIdentifier)
            => ConditionalQuoteIdentifier(GetIdentifier(DefaultVersionColumnName), quoteIdentifier);

        protected virtual bool DeclaredPolymorphicMatch(
            [NotNull] MemberInfo member,
            [NotNull] Func<MemberInfo, bool> declaredMatch)
            => declaredMatch(member)
               || member.GetMemberFromDeclaringClasses().Any(declaredMatch)
               || member.GetPropertyFromInterfaces().Any(declaredMatch);

        [CanBeNull]
        protected virtual MemberInfo PoidPropertyOrField(
            [NotNull] IModelInspector modelInspector,
            [NotNull] Type type)
        {
            IEnumerable<MemberInfo> poidCandidates = MembersProvider.GetEntityMembersForPoid(type);
            return poidCandidates.FirstOrDefault(mi => DeclaredPolymorphicMatch(mi, modelInspector.IsPersistentId));
        }

        protected virtual void NoPoidGuid(
            [NotNull] IModelInspector modelInspector,
            [NotNull] Type type,
            [NotNull] IClassAttributesMapper classCustomizer)
        {
            MemberInfo poidPropertyOrField = PoidPropertyOrField(modelInspector, type);
            if (ReferenceEquals(null, poidPropertyOrField))
            {
                classCustomizer.Id(null, idm => idm.Generator(Generators.Guid));
            }
        }

        protected virtual void NoSetterPoidToField(
            [NotNull] IModelInspector modelInspector,
            [NotNull] Type type,
            [NotNull] IClassAttributesMapper classCustomizer)
        {
            MemberInfo poidPropertyOrField = PoidPropertyOrField(modelInspector, type);
            if ((poidPropertyOrField != null) && MatchNoSetterProperty(poidPropertyOrField))
            {
                classCustomizer.Id(poidPropertyOrField, idm => idm.Access(Accessor.NoSetter));
            }
        }

        protected virtual void MemberToFieldAccessor(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IAccessorPropertyMapper propertyCustomizer)
        {
            if (MatchPropertyToField(member.LocalMember))
            {
                propertyCustomizer.Access(Accessor.Field);
            }
        }

        protected virtual void MemberNoSetterToField(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IAccessorPropertyMapper propertyCustomizer)
        {
            if (MatchNoSetterProperty(member.LocalMember))
            {
                propertyCustomizer.Access(Accessor.NoSetter);
            }
        }

        protected virtual void MemberReadOnlyAccessor(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IAccessorPropertyMapper propertyCustomizer)
        {
            if (MatchReadOnlyProperty(member.LocalMember))
            {
                propertyCustomizer.Access(Accessor.ReadOnly);
            }
        }

        protected virtual void ComponentParentToFieldAccessor(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IComponentAttributesMapper componentMapper)
        {
            Type componentType = member.LocalMember.GetPropertyOrFieldType();
            IEnumerable<MemberInfo> persistentProperties =
                MembersProvider
                    .GetComponentMembers(componentType)
                    .Where(p => ModelInspector.IsPersistentProperty(p));

            MemberInfo parentReferenceProperty = GetComponentParentReferenceProperty(persistentProperties, member.LocalMember.ReflectedType);
            if ((parentReferenceProperty != null) && MatchPropertyToField(parentReferenceProperty))
            {
                componentMapper.Parent(parentReferenceProperty, cp => cp.Access(Accessor.Field));
            }
        }

        protected virtual void ComponentParentNoSetterToField(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IComponentAttributesMapper componentMapper)
        {
            Type componentType = member.LocalMember.GetPropertyOrFieldType();
            IEnumerable<MemberInfo> persistentProperties =
                MembersProvider
                    .GetComponentMembers(componentType)
                    .Where(p => ModelInspector.IsPersistentProperty(p));

            MemberInfo parentReferenceProperty = GetComponentParentReferenceProperty(persistentProperties, member.LocalMember.ReflectedType);
            if ((parentReferenceProperty != null) && MatchNoSetterProperty(parentReferenceProperty))
            {
                componentMapper.Parent(parentReferenceProperty, cp => cp.Access(Accessor.NoSetter));
            }
        }

        protected virtual bool MatchReadOnlyProperty([CanBeNull] MemberInfo subject)
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

        protected virtual bool CanReadCantWriteInsideType([NotNull] PropertyInfo property)
            => !property.CanWrite && property.CanRead && (property.DeclaringType == property.ReflectedType);

        protected virtual bool CanReadCantWriteInBaseType([NotNull] PropertyInfo property)
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

            return (rfprop != null) && !rfprop.CanWrite && rfprop.CanRead;
        }

        protected virtual bool MatchNoSetterProperty([CanBeNull] MemberInfo subject)
        {
            PropertyInfo property = subject as PropertyInfo;
            if ((property == null) || property.CanWrite || !property.CanRead)
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

        protected virtual bool MatchPropertyToField([CanBeNull] MemberInfo subject)
        {
            PropertyInfo property = subject as PropertyInfo;
            if (property == null)
            {
                return false;
            }

            FieldInfo fieldInfo = PropertyToField.GetBackFieldInfo(property);
            return fieldInfo != null;
        }

        [CanBeNull]
        protected virtual MemberInfo GetComponentParentReferenceProperty(
            [NotNull] IEnumerable<MemberInfo> persistentProperties,
            [CanBeNull] Type propertiesContainerType)
        {
            return
                ModelInspector.IsComponent(propertiesContainerType)
                    ? persistentProperties.FirstOrDefault(pp => pp.GetPropertyOrFieldType() == propertiesContainerType)
                    : null;
        }

        protected virtual bool IsMemberDeclaredInATablePerClassHierarchy(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member)
        {
            Type declaredType = member.GetRootMember().DeclaringType;
            return modelInspector.IsTablePerClassHierarchy(declaredType) && !modelInspector.IsRootEntity(declaredType);
        }

        protected virtual bool IsRequired(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member)
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
                }
                while (required && (walker != null));
            }

            return required;
        }

        protected virtual bool IsPropertyPathPrimitive([NotNull] PropertyPath member)
        {
            Type memberType = member.MemberType();
            return memberType.IsPrimitive || (memberType == typeof(DateTime));
        }

        protected virtual void OnBeforeEntityMap(
            [NotNull] IModelInspector modelInspector,
            [NotNull] Type type,
            [NotNull] IEntityAttributesMapper entityCustomizer)
        {
            entityCustomizer.DynamicInsert(DynamicInsert);
            entityCustomizer.DynamicUpdate(DynamicUpdate);
            if ((ClassBatchSize != null) && (ClassBatchSize.Value > 0))
            {
                entityCustomizer.BatchSize(ClassBatchSize.Value);
            }

            if (entityCustomizer is IClassAttributesMapper classCustomizer)
            {
                classCustomizer.Abstract(type.IsAbstract);

                if (!string.IsNullOrWhiteSpace(DefaultCatalogName))
                {
                    classCustomizer.Catalog(DefaultCatalogName);
                }

                if (!string.IsNullOrWhiteSpace(DefaultSchemaName))
                {
                    classCustomizer.Schema(ConditionalQuoteIdentifier(DefaultSchemaName, null));
                }

                classCustomizer.Table(GetTableName(modelInspector, type, null));

                classCustomizer.Id(
                    m =>
                    {
                        m.Column(GetPrimaryKeyColumnName(modelInspector, type, null));
                        m.Generator(Generators.HighLow);
                    });

                if (modelInspector.IsTablePerClassHierarchy(type))
                {
                    classCustomizer.Discriminator(m => m.Column(GetDiscriminatorColumnName(modelInspector, type, null)));
                    classCustomizer.DiscriminatorValue(GetDiscriminatorValue(modelInspector, type));
                }

                MemberInfo[] versionProperties = VersionProperties(modelInspector, type).ToArray();
                if (versionProperties.Length == 1)
                {
                    classCustomizer.Version(versionProperties[0], m => m.Column(GetVersionColumnName(modelInspector, type, null)));
                }

                return;
            }

            if (entityCustomizer is ISubclassAttributesMapper subclassCustomizer)
            {
                subclassCustomizer.Abstract(type.IsAbstract);
                subclassCustomizer.DiscriminatorValue(GetDiscriminatorValue(modelInspector, type));

                return;
            }

            if (entityCustomizer is IJoinedSubclassAttributesMapper joinedSubclassCustomizer)
            {
                joinedSubclassCustomizer.Abstract(type.IsAbstract);

                if (!string.IsNullOrWhiteSpace(DefaultCatalogName))
                {
                    joinedSubclassCustomizer.Catalog(DefaultCatalogName);
                }

                if (!string.IsNullOrWhiteSpace(DefaultSchemaName))
                {
                    joinedSubclassCustomizer.Schema(ConditionalQuoteIdentifier(DefaultSchemaName, null));
                }

                joinedSubclassCustomizer.Table(GetTableName(modelInspector, type, null));
                joinedSubclassCustomizer.Key(
                    k =>
                    {
                        k.Column(GetPrimaryKeyColumnName(modelInspector, type.BaseType ?? type, null));
                        if (type.BaseType != null)
                        {
                            k.ForeignKey($"FK_{GetTableName(modelInspector, type, false)}_{GetTableName(modelInspector, type.BaseType, false)}");
                        }
                    });

                return;
            }

            if (entityCustomizer is IUnionSubclassAttributesMapper unionSubclassCustomizer)
            {
                unionSubclassCustomizer.Abstract(type.IsAbstract);

                if (!string.IsNullOrWhiteSpace(DefaultCatalogName))
                {
                    unionSubclassCustomizer.Catalog(DefaultCatalogName);
                }

                if (!string.IsNullOrWhiteSpace(DefaultSchemaName))
                {
                    unionSubclassCustomizer.Schema(ConditionalQuoteIdentifier(DefaultSchemaName, null));
                }

                unionSubclassCustomizer.Table(GetTableName(modelInspector, type, null));
            }
        }

        protected override void OnBeforeMapClass(IModelInspector modelInspector, Type type, IClassAttributesMapper classCustomizer)
        {
            OnBeforeEntityMap(modelInspector, type, classCustomizer);

            if (modelInspector.IsRootEntity(type)
                && CachedEntityTypes.TryGetValue(type, out CachedEntityType cachedEntityType))
            {
                classCustomizer
                    .Cache(
                        m =>
                        {
                            m.Region(cachedEntityType.RegionName ?? DefaultRegionNameForCachedEntities);
                            m.Usage(cachedEntityType.CacheUsage ?? DefaultCacheUsageForCachedEntities);
                        });
            }
        }

        protected override void OnBeforeMapSubclass(IModelInspector modelInspector, Type type, ISubclassAttributesMapper subclassCustomizer)
        {
            OnBeforeEntityMap(modelInspector, type, subclassCustomizer);
        }

        protected override void OnBeforeMapJoinedSubclass(IModelInspector modelInspector, Type type, IJoinedSubclassAttributesMapper joinedSubclassCustomizer)
        {
            OnBeforeEntityMap(modelInspector, type, joinedSubclassCustomizer);
        }

        protected override void OnModelMapperOnBeforeMapUnionSubclass(IModelInspector modelInspector, Type type, IUnionSubclassAttributesMapper unionSubclassCustomizer)
        {
            OnBeforeEntityMap(modelInspector, type, unionSubclassCustomizer);
        }

        protected override void OnBeforeMapProperty(IModelInspector modelInspector, PropertyPath member, IPropertyMapper propertyCustomizer)
        {
            string columnName = GetColumnName(modelInspector, member, null);
            propertyCustomizer.Column(columnName);

            if (!IsMemberDeclaredInATablePerClassHierarchy(modelInspector, member)
                && IsRequired(modelInspector, member))
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

        protected override void OnBeforeMapManyToMany(IModelInspector modelInspector, PropertyPath member, IManyToManyMapper collectionRelationManyToManyCustomizer)
        {
            string columnName = GetForeignKeyColumnName(modelInspector, member, false);
            collectionRelationManyToManyCustomizer.Column(ConditionalQuoteIdentifier(columnName, null));

            string tableName = GetTableNameForManyToMany(modelInspector, member, false);
            string foreignKeyName = $"FK_{tableName}_{columnName}";
            collectionRelationManyToManyCustomizer.ForeignKey(foreignKeyName);
        }

        protected override void OnBeforeMapManyToOne(IModelInspector modelInspector, PropertyPath member, IManyToOneMapper propertyCustomizer)
        {
            string foreignKeyColumnName = GetForeignKeyColumnName(modelInspector, member, false);
            propertyCustomizer.Column(ConditionalQuoteIdentifier(foreignKeyColumnName, null));
            string tableName = GetTableName(modelInspector, member.Owner(), false);
            propertyCustomizer.ForeignKey($"FK_{tableName}_{foreignKeyColumnName}");

            if (!IsMemberDeclaredInATablePerClassHierarchy(modelInspector, member)
                && IsRequired(modelInspector, member))
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

        protected virtual void OnBeforeMappingCollectionConvention(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] ICollectionPropertiesMapper collectionPropertiesCustomizer)
        {
            if (!string.IsNullOrWhiteSpace(DefaultCatalogName))
            {
                collectionPropertiesCustomizer.Catalog(DefaultCatalogName);
            }

            if (!string.IsNullOrWhiteSpace(DefaultSchemaName))
            {
                collectionPropertiesCustomizer.Schema(ConditionalQuoteIdentifier(DefaultSchemaName, null));
            }

            if ((CollectionBatchSize != null) && (CollectionBatchSize.Value > 0))
            {
                collectionPropertiesCustomizer.Fetch(CollectionFetchMode.Select);
                collectionPropertiesCustomizer.BatchSize(CollectionBatchSize.Value);
            }

            if (modelInspector.IsManyToManyItem(member.LocalMember))
            {
                string tableName = GetTableNameForManyToMany(modelInspector, member, null);
                collectionPropertiesCustomizer.Table(tableName);
                collectionPropertiesCustomizer.Key(
                    k =>
                    {
                        string keyColumnName = GetKeyColumnName(ForeignKeyType, member.Owner().Name, null);
                        k.Column(keyColumnName);
                    });
            }
            else if (modelInspector.IsSet(member.LocalMember)
                     || modelInspector.IsBag(member.LocalMember))
            {
                // If other side has many-to-one, make it inverse
                MemberInfo oneToManyProperty = GetOneToManyOtherSideProperty(member);
                if (oneToManyProperty != null)
                {
                    collectionPropertiesCustomizer.Inverse(true);
                }

                string keyColumnName = GetForeignKeyColumnName(modelInspector, member, null);
                collectionPropertiesCustomizer.Key(k => k.Column(keyColumnName));
            }

            if (modelInspector.IsSet(member.LocalMember)
                || modelInspector.IsBag(member.LocalMember)
                || modelInspector.IsList(member.LocalMember)
                || modelInspector.IsArray(member.LocalMember))
            {
                Type propertyOrFieldType =
                    member
                        .GetRootMember()
                        .GetPropertyOrFieldType();
                Type itemType = GetCompatibleItemType(propertyOrFieldType);
                if ((itemType != null)
                    && CachedEntityTypes.TryGetValue(itemType, out CachedEntityType cachedEntityType))
                {
                    collectionPropertiesCustomizer
                        .Cache(
                            m =>
                            {
                                m.Region(cachedEntityType.RegionName ?? DefaultRegionNameForCachedEntities);
                                m.Usage(cachedEntityType.CacheUsage ?? DefaultCacheUsageForCachedEntities);
                            });
                }
            }
        }

        protected virtual Type GetCompatibleItemType(Type type)
        {
            if (type == null)
            {
                return null;
            }

            if (type.IsArray)
            {
                return type.GetElementType();
            }

            if (!type.GetTypeInfo().IsGenericType || type.GetTypeInfo().IsGenericTypeDefinition)
            {
                return null;
            }

            return type.GetGenericArguments().FirstOrDefault();
        }

        protected override void OnAfterMapClass(IModelInspector modelInspector, Type type, IClassAttributesMapper classCustomizer)
        {
            base.OnAfterMapClass(modelInspector, type, classCustomizer);

            if (AdjustColumnForForeignGenerator)
            {
                HbmMapping hbmMapping = TryGetHbmMapping(classCustomizer);
                HbmClass rootEntity =
                    hbmMapping
                        ?.RootClasses
                        .SingleOrDefault(c => string.Equals(c.Name, type.FullName, StringComparison.Ordinal)
                                              && string.Equals(c.Id.generator.@class, "foreign", StringComparison.Ordinal));
                HbmParam propertyParam =
                    rootEntity
                        ?.Id
                        .generator
                        .param.SingleOrDefault(p => string.Equals(p.name, "property"));
                if ((propertyParam != null) && (propertyParam.Text.Length == 1))
                {
                    string columnName = GetKeyColumnName(PrimaryKeyType, propertyParam.Text[0], null);
                    classCustomizer.Id(m => m.Column(columnName));
                }
            }
        }

        protected virtual HbmMapping TryGetHbmMapping(object customizer)
            => customizer is ClassMapper classMapper
                   ? MapDocPropertyInfo != null
                         ? MapDocPropertyInfo.GetValue(classMapper) as HbmMapping
                         : null
                   : null;
    }
}
