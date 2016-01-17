// Copyright 2016 by PeopleWare n.v..
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

using PPWCode.Vernacular.NHibernate.I.Interfaces;

namespace PPWCode.Vernacular.NHibernate.I.MappingByCode
{
    /// <summary>
    ///     Simple ModelMapper.
    ///     This code is based on <see cref="ConventionModelMapper" />
    /// </summary>
    public abstract class SimpleModelMapper : ModelMapperBase
    {
        private readonly object m_Locker = new object();
        private readonly DefaultCandidatePersistentMembersProvider m_MembersProvider;
        private volatile bool m_ModelMetaDatasByTypeCached;
        private IDictionary<Type, ModelMetaData> m_ModelMetaDatasByTypeCache;

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

        protected virtual IEnumerable<ModelMetaData> ModelMetaDatas
        {
            get { yield break; }
        }

        protected virtual bool UseCamelCaseUnderScoreForDbObjects
        {
            get { return false; }
        }

        protected virtual bool DynamicInsert
        {
            get { return false; }
        }

        protected virtual bool DynamicUpdate
        {
            get { return false; }
        }

        protected virtual int? BatchSize
        {
            get { return null; }
        }

        protected virtual string DefaultSchemaName
        {
            get { return null; }
        }

        protected virtual string DefaultCatalogName
        {
            get { return null; }
        }

        protected virtual bool QuoteIdentifiers
        {
            get { return false; }
        }

        protected virtual IDictionary<Type, ModelMetaData> ModelMetaDatasByType
        {
            get
            {
                if (!m_ModelMetaDatasByTypeCached)
                {
                    lock (m_Locker)
                    {
                        if (!m_ModelMetaDatasByTypeCached)
                        {
                            m_ModelMetaDatasByTypeCache = ModelMetaDatas.ToDictionary(m => m.Type);
                            m_ModelMetaDatasByTypeCached = true;
                        }
                    }
                }

                return m_ModelMetaDatasByTypeCache;
            }
        }

        protected virtual PrimaryKeyTypeEnum PrimaryKeyType
        {
            get { return PrimaryKeyTypeEnum.TYPE_ID; }
        }

        protected virtual string GetTableName(IModelInspector modelInspector, Type type)
        {
            ModelMetaData modelMetaData;
            string tableName = null;
            if (ModelMetaDatasByType.TryGetValue(type, out modelMetaData))
            {
                tableName = modelMetaData.TableName;
            }

            string result = ConditionalQuoteIdentifier(tableName ?? GetIdentifier(type.Name));

            return result;
        }

        protected virtual string GetColumnName(IModelInspector modelInspector, PropertyPath member)
        {
            string defaultColumnName = member.ToColumnName();
            string columnPrefix = null;
            string columnName = null;

            Type currentType = member.LocalMember.ReflectedType;
            bool walkToParent = modelInspector.IsTablePerClassHierarchy(currentType);
            while (currentType != null && currentType != typeof(object))
            {
                ModelMetaData modelMetaData;
                if (ModelMetaDatasByType.TryGetValue(currentType, out modelMetaData))
                {
                    columnPrefix = modelMetaData.ColumnPrefix;
                    modelMetaData.ColumnNames.TryGetValue(defaultColumnName, out columnName);
                    break;
                }

                currentType = walkToParent ? currentType.BaseType : null;
            }

            string result = ConditionalQuoteIdentifier(string.Concat(columnPrefix, columnName ?? GetIdentifier(defaultColumnName)));

            return result;
        }

        protected virtual string GetKeyColumnName(IModelInspector modelInspector, PropertyPath member, bool quoteIdentifiers)
        {
            MemberInfo otherSideProperty = member.OneToManyOtherSideProperty();
            Type type = modelInspector.IsOneToMany(member.LocalMember) && otherSideProperty != null
                            ? otherSideProperty.MemberType()
                            : member.MemberType();
            return GetKeyColumnName(modelInspector, type, true, quoteIdentifiers);
        }

        protected virtual string GetKeyColumnName(IModelInspector modelInspector, Type type, bool foreignKey, bool quoteIdentifiers)
        {
            ModelMetaData modelMetaData;
            string columnPrefix = null;
            string tableName = null;
            if (ModelMetaDatasByType.TryGetValue(type, out modelMetaData))
            {
                tableName = modelMetaData.TableName;
                columnPrefix = modelMetaData.ColumnPrefix;
            }

            string result;
            PrimaryKeyTypeEnum primaryKeyType = foreignKey ? PrimaryKeyTypeEnum.TYPE_ID : PrimaryKeyType;
            switch (primaryKeyType)
            {
                case PrimaryKeyTypeEnum.ID:
                    result = string.Concat(columnPrefix, GetIdentifier(@"Id"));
                    break;

                case PrimaryKeyTypeEnum.TYPE_ID:
                    if (string.IsNullOrWhiteSpace(tableName))
                    {
                        result = string.Concat(columnPrefix, GetIdentifier(string.Concat(type.Name, @"Id")));
                    }
                    else
                    {
                        result = UseCamelCaseUnderScoreForDbObjects
                                     ? string.Concat(columnPrefix, tableName, tableName.EndsWith(@"_") ? @"ID" : @"_ID")
                                     : string.Concat(columnPrefix, tableName, @"Id");
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (quoteIdentifiers)
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

        public virtual string GetDiscriminatorColumnName(IModelInspector modelInspector, Type type)
        {
            string defaultColumnName = @"Discriminator";
            string columnPrefix = null;
            string dicriminatorColumnName = null;
            ModelMetaData modelMetaData;

            // ReSharper disable once AssignNullToNotNullAttribute
            if (ModelMetaDatasByType.TryGetValue(type, out modelMetaData))
            {
                columnPrefix = modelMetaData.ColumnPrefix;
                dicriminatorColumnName = modelMetaData.Discriminator;
            }

            string result = ConditionalQuoteIdentifier(string.Concat(columnPrefix, dicriminatorColumnName ?? GetIdentifier(defaultColumnName)));

            return result;
        }

        public virtual object GetDiscriminatorValue(IModelInspector modelInspector, Type type)
        {
            return CamelCaseToUnderscore(type.Name);
        }

        public virtual string GetVersionColumnName(IModelInspector modelInspector, Type type)
        {
            string defaultColumnName = @"PersistenceVersion";
            string columnPrefix = null;
            string versionColumnName = null;
            ModelMetaData modelMetaData;

            // ReSharper disable once AssignNullToNotNullAttribute
            if (ModelMetaDatasByType.TryGetValue(type, out modelMetaData))
            {
                columnPrefix = modelMetaData.ColumnPrefix;
                versionColumnName = modelMetaData.Version;
            }

            string result = ConditionalQuoteIdentifier(string.Concat(columnPrefix, versionColumnName ?? GetIdentifier(defaultColumnName)));

            return result;
        }

        protected DefaultCandidatePersistentMembersProvider MembersProvider
        {
            get { return m_MembersProvider; }
        }

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
            classCustomizer.DynamicInsert(DynamicInsert);
            classCustomizer.DynamicUpdate(DynamicUpdate);

            if (BatchSize != null && BatchSize.Value > 0)
            {
                classCustomizer.BatchSize(BatchSize.Value);
            }

            if (!string.IsNullOrWhiteSpace(DefaultCatalogName))
            {
                classCustomizer.Schema(ConditionalQuoteIdentifier(DefaultCatalogName));
            }

            if (!string.IsNullOrWhiteSpace(DefaultSchemaName))
            {
                classCustomizer.Schema(ConditionalQuoteIdentifier(DefaultSchemaName));
            }

            classCustomizer.Table(GetTableName(modelInspector, type));

            classCustomizer.Id(
                m =>
                {
                    m.Column(GetKeyColumnName(modelInspector, type, false, true));
                    m.Generator(Generators.HighLow);
                });

            if (modelInspector.IsTablePerClassHierarchy(type))
            {
                classCustomizer.Discriminator(m => m.Column(GetDiscriminatorColumnName(modelInspector, type)));
                classCustomizer.DiscriminatorValue(GetDiscriminatorValue(modelInspector, type));
            }

            MemberInfo[] versionProperties = VersionProperties(modelInspector, type).ToArray();
            if (versionProperties.Length == 1)
            {
                classCustomizer.Version(versionProperties[0], m => m.Column(GetVersionColumnName(modelInspector, type)));
            }
        }

        protected override void OnBeforeMapProperty(IModelInspector modelInspector, PropertyPath member, IPropertyMapper propertyCustomizer)
        {
            Type reflectedType = member.LocalMember.ReflectedType;
            if (reflectedType == null)
            {
                return;
            }

            propertyCustomizer.Column(GetColumnName(modelInspector, member));

            bool required =
                member.LocalMember
                      .GetCustomAttributes()
                      .OfType<RequiredAttribute>()
                      .Any();

            // Getting tableType of reflected object
            Type memberType = member.MemberType();

            bool notNullable = required || (memberType != null && memberType.IsPrimitive) || memberType == typeof(DateTime);
            propertyCustomizer.NotNullable(notNullable);

            StringLengthAttribute stringLengthAttribute =
                member.LocalMember
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
            unionSubclassCustomizer.Table(GetTableName(modelInspector, type));
        }

        protected override void OnBeforeMapJoinedSubclass(IModelInspector modelInspector, Type type, IJoinedSubclassAttributesMapper joinedSubclassCustomizer)
        {
            joinedSubclassCustomizer.Key(
                k =>
                {
                    k.Column(GetKeyColumnName(modelInspector, type.BaseType ?? type, false, true));
                    if (type.BaseType != null)
                    {
                        k.ForeignKey(string.Format("FK_{0}_{1}", type.Name, type.BaseType.Name));
                    }
                });

            joinedSubclassCustomizer.Table(GetTableName(modelInspector, type));
        }

        protected override void OnBeforeMapManyToMany(IModelInspector modelInspector, PropertyPath member, IManyToManyMapper collectionRelationManyToManyCustomizer)
        {
            string columnName = ConditionalQuoteIdentifier(GetIdentifier(string.Format("{0}Id", member.CollectionElementType().Name)));
            collectionRelationManyToManyCustomizer.Column(columnName);
        }

        protected override void OnBeforeMapManyToOne(IModelInspector modelInspector, PropertyPath member, IManyToOneMapper propertyCustomizer)
        {
            string foreignKeyColumnName = GetKeyColumnName(modelInspector, member, false);
            propertyCustomizer.Column(ConditionalQuoteIdentifier(foreignKeyColumnName));
            propertyCustomizer.ForeignKey(string.Format("FK_{0}_{1}", member.Owner().Name, foreignKeyColumnName));

            bool required =
                member.LocalMember
                      .GetCustomAttributes()
                      .OfType<RequiredAttribute>()
                      .Any();
            propertyCustomizer.NotNullable(required);
            propertyCustomizer.Index(string.Format("IX_FK_{0}_{1}", member.Owner().Name, foreignKeyColumnName));
        }

        protected override void OnBeforeMapOneToOne(IModelInspector modelInspector, PropertyPath member, IOneToOneMapper propertyCustomizer)
        {
            propertyCustomizer.ForeignKey(string.Format("FK_{0}_{1}", member.Owner().Name, member.ToColumnName()));
        }

        protected override void OnBeforeMapSubclass(IModelInspector modelInspector, Type type, ISubclassAttributesMapper subclassCustomizer)
        {
            subclassCustomizer.DiscriminatorValue(GetDiscriminatorValue(modelInspector, type));
        }

        protected virtual void OnBeforeMappingCollectionConvention(IModelInspector modelinspector, PropertyPath member, ICollectionPropertiesMapper collectionPropertiesCustomizer)
        {
            if (modelinspector.IsManyToMany(member.LocalMember))
            {
                collectionPropertiesCustomizer.Table(member.ManyToManyIntermediateTableName("To"));
                collectionPropertiesCustomizer.Key(k => k.Column(GetKeyColumnName(modelinspector, member.Owner(), true, true)));
            }
            else if (modelinspector.IsSet(member.LocalMember))
            {
                // If otherside has many-to-one, make it inverse, if not specify foreign key on Key element
                MemberInfo oneToManyProperty = member.OneToManyOtherSideProperty();
                IEnumerable<MemberInfo> candidatesManyToOne =
                    MembersProvider
                        .GetRootEntityMembers(oneToManyProperty.DeclaringType)
                        .Where(modelinspector.IsManyToOne);
                if (candidatesManyToOne.Any(mi => mi.MemberType() == member.LocalMember.DeclaringType))
                {
                    collectionPropertiesCustomizer.Inverse(true);
                }
                else
                {
                    Contract.Assert(oneToManyProperty.DeclaringType != null, "otherSideProperty.DeclaringType != null");
                    collectionPropertiesCustomizer.Key(k => k.ForeignKey(string.Format("FK_{0}_{1}", oneToManyProperty.DeclaringType.Name, oneToManyProperty.Name)));
                }

                collectionPropertiesCustomizer.Key(k => k.Column(GetKeyColumnName(modelinspector, member, true)));
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

        protected virtual string ConditionalQuoteIdentifier(string identifier)
        {
            return QuoteIdentifiers ? QuoteIdentifier(identifier) : identifier;
        }

        protected virtual string QuoteIdentifier(string identifier)
        {
            return string.Format("`{0}`", identifier);
        }

        public static string CamelCaseToUnderscore(string camelCase)
        {
            const string Rgx = @"([A-Z]+)([A-Z][a-z])";
            const string Rgx2 = @"([a-z\d])([A-Z])";

            string result = Regex.Replace(camelCase, Rgx, "$1_$2");
            result = Regex.Replace(result, Rgx2, "$1_$2");
            return result.ToUpper();
        }

        public enum PrimaryKeyTypeEnum
        {
            ID,
            TYPE_ID
        }

        public class ModelMetaData
        {
            private readonly string m_ColumnPrefix;
            private readonly IDictionary<string, string> m_ColumnNames;
            private readonly Type m_Type;
            private readonly string m_TableName;
            private readonly string m_Discriminator;
            private readonly string m_Version;

            public ModelMetaData(Type type, string tableName, string discriminator, string version, string columnPrefix, IDictionary<string, string> columnNames)
            {
                Contract.Requires(type != null);
                Contract.Requires(columnNames != null);
                Contract.Ensures(Type == type);
                Contract.Ensures(TableName == tableName);
                Contract.Ensures(Discriminator == discriminator);
                Contract.Ensures(Version == version);
                Contract.Ensures(ColumnPrefix == columnPrefix);

                m_Type = type;
                m_TableName = tableName;
                m_Discriminator = discriminator;
                m_Version = version;
                m_ColumnPrefix = columnPrefix;
                m_ColumnNames = columnNames;
            }

            public string TableName
            {
                get { return m_TableName; }
            }

            public string ColumnPrefix
            {
                get { return m_ColumnPrefix; }
            }

            public IDictionary<string, string> ColumnNames
            {
                get { return m_ColumnNames; }
            }

            public Type Type
            {
                get { return m_Type; }
            }

            public string Discriminator
            {
                get { return m_Discriminator; }
            }

            public string Version
            {
                get { return m_Version; }
            }
        }
    }
}