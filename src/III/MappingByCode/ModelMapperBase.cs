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
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using JetBrains.Annotations;

using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Impl;

namespace PPWCode.Vernacular.NHibernate.III.MappingByCode
{
    public abstract class ModelMapperBase : IPpwHbmMapping
    {
        private readonly object _locker = new object();
        private readonly IMappingAssemblies _mappingAssemblies;
        private HbmMapping _hbmMapping;

        protected ModelMapperBase(
            [NotNull] IMappingAssemblies mappingAssemblies,
            [NotNull] Configuration configuration)
        {
            Configuration = configuration;
            _mappingAssemblies = mappingAssemblies;
            ModelMapper = new ModelMapper();

            ModelMapper.BeforeMapAny += OnBeforeMapAny;
            ModelMapper.BeforeMapBag += OnBeforeMapBag;
            ModelMapper.BeforeMapClass += OnBeforeMapClass;
            ModelMapper.BeforeMapComponent += OnBeforeMapComponent;
            ModelMapper.BeforeMapElement += OnBeforeMapElement;
            ModelMapper.BeforeMapIdBag += OnBeforeMapIdBag;
            ModelMapper.BeforeMapJoinedSubclass += OnBeforeMapJoinedSubclass;
            ModelMapper.BeforeMapList += OnBeforeMapList;
            ModelMapper.BeforeMapManyToMany += OnBeforeMapManyToMany;
            ModelMapper.BeforeMapManyToOne += OnBeforeMapManyToOne;
            ModelMapper.BeforeMapMap += OnBeforeMapMap;
            ModelMapper.BeforeMapMapKey += OnBeforeMapMapKey;
            ModelMapper.BeforeMapMapKeyManyToMany += OnBeforeMapMapKeyManyToMany;
            ModelMapper.BeforeMapOneToMany += OnBeforeMapOneToMany;
            ModelMapper.BeforeMapOneToOne += OnBeforeMapOneToOne;
            ModelMapper.BeforeMapProperty += OnBeforeMapProperty;
            ModelMapper.BeforeMapSet += OnBeforeMapSet;
            ModelMapper.BeforeMapSubclass += OnBeforeMapSubclass;
            ModelMapper.BeforeMapUnionSubclass += OnModelMapperOnBeforeMapUnionSubclass;

            ModelMapper.AfterMapAny += OnAfterMapAny;
            ModelMapper.AfterMapBag += OnAfterMapBag;
            ModelMapper.AfterMapClass += OnAfterMapClass;
            ModelMapper.AfterMapComponent += OnAfterMapComponent;
            ModelMapper.AfterMapElement += OnAfterMapElement;
            ModelMapper.AfterMapIdBag += OnAfterMapIdBag;
            ModelMapper.AfterMapJoinedSubclass += OnAfterMapJoinedSubclass;
            ModelMapper.AfterMapList += OnAfterMapList;
            ModelMapper.AfterMapManyToMany += OnAfterMapManyToMany;
            ModelMapper.AfterMapManyToOne += OnAfterMapManyToOne;
            ModelMapper.AfterMapMap += OnAfterMapMap;
            ModelMapper.AfterMapMapKey += OnAfterMapMapKey;
            ModelMapper.AfterMapMapKeyManyToMany += OnAfterMapMapKeyManyToMany;
            ModelMapper.AfterMapOneToMany += OnAfterMapOneToMany;
            ModelMapper.AfterMapOneToOne += OnAfterMapOneToOne;
            ModelMapper.AfterMapProperty += OnAfterMapProperty;
            ModelMapper.AfterMapSet += OnAfterMapSet;
            ModelMapper.AfterMapSubclass += OnAfterMapSubclass;
            ModelMapper.AfterMapUnionSubclass += OnAfterMapUnionSubclass;
        }

        [NotNull]
        protected IModelInspector ModelInspector
            => ModelMapper.ModelInspector;

        [NotNull]
        public Configuration Configuration { get; }

        [CanBeNull]
        protected virtual IEnumerable<Type> MappingTypes
            => null;

        [CanBeNull]
        protected virtual string DefaultAccess
            => null;

        protected virtual bool DefaultLazy
            => true;

        [CanBeNull]
        protected virtual string DefaultCascade
            => null;

        public abstract ICandidatePersistentMembersProvider MembersProvider { get; }

        public abstract bool QuoteIdentifiers { get; }
        public abstract bool UseCamelCaseUnderScoreForDbObjects { get; }

        public ModelMapper ModelMapper { get; }

        public HbmMapping HbmMapping
        {
            get
            {
                if (_hbmMapping == null)
                {
                    lock (_locker)
                    {
                        if (_hbmMapping == null)
                        {
                            IEnumerable<Type> mappingTypes =
                                MappingTypes
                                ?? _mappingAssemblies
                                    .GetAssemblies()
                                    .SelectMany(a => a.GetExportedTypes());
                            ModelMapper.AddMappings(mappingTypes);
                            HbmMapping hbmMapping;

                            // Following code is an improvement to pre-order our types topological, nHibernate doesn't do this right at this moment v5.1.3
                            FieldInfo customizerHolderFieldInfo = typeof(ModelMapper).GetField("customizerHolder", BindingFlags.Instance | BindingFlags.NonPublic);
                            if (customizerHolderFieldInfo != null)
                            {
                                ICustomizersHolder customizerHolder = (ICustomizersHolder)customizerHolderFieldInfo.GetValue(ModelMapper);
                                IEnumerable<Type> types = customizerHolder.GetAllCustomizedEntities();
                                HashSet<Type> rootClasses =
                                    new HashSet<Type>(
                                        types
                                            .Where(t => ModelMapper.ModelInspector.IsEntity(t)
                                                        && ModelMapper.ModelInspector.IsRootEntity(t)));
                                HashSet<Type> subClasses =
                                    new HashSet<Type>(
                                        types
                                            .Where(t => ModelMapper.ModelInspector.IsEntity(t)
                                                        && !ModelMapper.ModelInspector.IsRootEntity(t)));
                                List<Type> orderedClasses = new List<Type>(rootClasses);
                                HashSet<Type> processedClasses = new HashSet<Type>(orderedClasses);
                                bool nextLevelEntitiesAvailable;
                                do
                                {
                                    HashSet<Type> nextLevelEntities = new HashSet<Type>();
                                    foreach (Type subClass in subClasses.ToList())
                                    {
                                        if (processedClasses.Contains(subClass.BaseType))
                                        {
                                            nextLevelEntities.Add(subClass);
                                            subClasses.Remove(subClass);
                                            processedClasses.Add(subClass);
                                        }
                                    }

                                    orderedClasses.AddRange(nextLevelEntities);
                                    nextLevelEntitiesAvailable = nextLevelEntities.Count > 0;
                                }
                                while (nextLevelEntitiesAvailable && (subClasses.Count > 0));

                                hbmMapping = ModelMapper.CompileMappingFor(orderedClasses.Concat(subClasses));
                            }
                            else
                            {
                                hbmMapping = ModelMapper.CompileMappingForAllExplicitlyAddedEntities();
                            }

                            hbmMapping.defaultlazy = DefaultLazy;
                            if (!string.IsNullOrWhiteSpace(DefaultAccess))
                            {
                                hbmMapping.defaultaccess = DefaultAccess;
                            }

                            if (!string.IsNullOrWhiteSpace(DefaultCascade))
                            {
                                hbmMapping.defaultcascade = DefaultCascade;
                            }

                            _hbmMapping = hbmMapping;
                        }
                    }
                }

                return _hbmMapping;
            }
        }

        protected virtual void OnModelMapperOnBeforeMapUnionSubclass(
            [NotNull] IModelInspector modelInspector,
            [NotNull] Type type,
            [NotNull] IUnionSubclassAttributesMapper unionSubclassCustomizer)
        {
        }

        protected virtual void OnBeforeMapSubclass(
            [NotNull] IModelInspector modelInspector,
            [NotNull] Type type,
            [NotNull] ISubclassAttributesMapper subclassCustomizer)
        {
        }

        protected virtual void OnBeforeMapSet(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] ISetPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnBeforeMapProperty(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IPropertyMapper propertyCustomizer)
        {
        }

        protected virtual void OnBeforeMapOneToOne(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IOneToOneMapper propertyCustomizer)
        {
        }

        protected virtual void OnBeforeMapOneToMany(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IOneToManyMapper collectionRelationOneToManyCustomizer)
        {
        }

        protected virtual void OnBeforeMapMapKeyManyToMany(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IMapKeyManyToManyMapper mapKeyManyToManyCustomizer)
        {
        }

        protected virtual void OnBeforeMapMapKey(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IMapKeyMapper mapKeyElementCustomizer)
        {
        }

        protected virtual void OnBeforeMapMap(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IMapPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnBeforeMapManyToOne(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IManyToOneMapper propertyCustomizer)
        {
        }

        protected virtual void OnBeforeMapManyToMany(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IManyToManyMapper collectionRelationManyToManyCustomizer)
        {
        }

        protected virtual void OnBeforeMapList(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IListPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnBeforeMapJoinedSubclass(
            [NotNull] IModelInspector modelInspector,
            [NotNull] Type type,
            [NotNull] IJoinedSubclassAttributesMapper joinedSubclassCustomizer)
        {
        }

        protected virtual void OnBeforeMapIdBag(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IIdBagPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnBeforeMapElement(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IElementMapper collectionRelationElementCustomizer)
        {
        }

        protected virtual void OnBeforeMapComponent(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IComponentAttributesMapper propertyCustomizer)
        {
        }

        protected virtual void OnBeforeMapClass(
            [NotNull] IModelInspector modelInspector,
            [NotNull] Type type,
            [NotNull] IClassAttributesMapper classCustomizer)
        {
        }

        protected virtual void OnBeforeMapBag(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IBagPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnBeforeMapAny(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IAnyMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapUnionSubclass(
            [NotNull] IModelInspector modelInspector,
            [NotNull] Type type,
            [NotNull] IUnionSubclassAttributesMapper unionSubclassCustomizer)
        {
        }

        protected virtual void OnAfterMapSubclass(
            [NotNull] IModelInspector modelInspector,
            [NotNull] Type type,
            [NotNull] ISubclassAttributesMapper subclassCustomizer)
        {
        }

        protected virtual void OnAfterMapSet(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] ISetPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapProperty(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IPropertyMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapOneToOne(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IOneToOneMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapOneToMany(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IOneToManyMapper collectionRelationOneToManyCustomizer)
        {
        }

        protected virtual void OnAfterMapMapKeyManyToMany(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IMapKeyManyToManyMapper mapKeyManyToManyCustomizer)
        {
        }

        protected virtual void OnAfterMapMapKey(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IMapKeyMapper mapKeyElementCustomizer)
        {
        }

        protected virtual void OnAfterMapMap(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IMapPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapManyToOne(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IManyToOneMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapManyToMany(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IManyToManyMapper collectionRelationManyToManyCustomizer)
        {
        }

        protected virtual void OnAfterMapList(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IListPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapJoinedSubclass(
            [NotNull] IModelInspector modelInspector,
            [NotNull] Type type,
            [NotNull] IJoinedSubclassAttributesMapper joinedSubclassCustomizer)
        {
        }

        protected virtual void OnAfterMapIdBag(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IIdBagPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapElement(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IElementMapper collectionRelationElementCustomizer)
        {
        }

        protected virtual void OnAfterMapComponent(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IComponentAttributesMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapClass(
            [NotNull] IModelInspector modelInspector,
            [NotNull] Type type,
            [NotNull] IClassAttributesMapper classCustomizer)
        {
        }

        protected virtual void OnAfterMapBag(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IBagPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapAny(
            [NotNull] IModelInspector modelInspector,
            [NotNull] PropertyPath member,
            [NotNull] IAnyMapper propertyCustomizer)
        {
        }

        public virtual string CamelCaseToUnderscore(string camelCase)
        {
            const string Rgx = @"([A-Z]+)([A-Z][a-z])";
            const string Rgx2 = @"([a-z\d])([A-Z])";

            if (camelCase != null)
            {
                string result = Regex.Replace(camelCase, Rgx, "$1_$2");
                result = Regex.Replace(result, Rgx2, "$1_$2");
                return result.ToUpper();
            }

            return null;
        }

        [ContractAnnotation("null => null; notnull => notnull")]
        public virtual string GetIdentifier(string identifier)
            => UseCamelCaseUnderScoreForDbObjects ? CamelCaseToUnderscore(identifier) : identifier;

        [ContractAnnotation("identifier:null => null; identifier:notnull => notnull")]
        public virtual string ConditionalQuoteIdentifier(string identifier, bool? quoteIdentifier)
            => quoteIdentifier ?? QuoteIdentifiers ? QuoteIdentifier(identifier) : identifier;

        [ContractAnnotation("null => null; notnull => notnull")]
        public virtual string QuoteIdentifier(string identifier)
            => identifier != null ? $"`{identifier}`" : null;
    }
}
