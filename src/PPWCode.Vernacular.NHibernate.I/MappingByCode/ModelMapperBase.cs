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
using System.Diagnostics.Contracts;
using System.Linq;

using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;

using PPWCode.Vernacular.NHibernate.I.Interfaces;

namespace PPWCode.Vernacular.NHibernate.I.MappingByCode
{
    public abstract class ModelMapperBase : IHbmMapping
    {
        private readonly IMappingAssemblies m_MappingAssemblies;
        private readonly ModelMapper m_ModelMapper;

        protected ModelMapperBase(IMappingAssemblies mappingAssemblies)
        {
            Contract.Requires(mappingAssemblies != null);

            m_MappingAssemblies = mappingAssemblies;
            m_ModelMapper = new ModelMapper();

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
            ModelMapper.BeforeMapUnionSubclass += ModelMapperOnBeforeMapUnionSubclass;

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

        protected ModelMapper ModelMapper
        {
            get { return m_ModelMapper; }
        }

        protected IModelInspector ModelInspector
        {
            get { return ModelMapper.ModelInspector; }
        }

        public HbmMapping GetHbmMapping()
        {
            IEnumerable<Type> mappingTypes =
                MappingTypes
                ?? m_MappingAssemblies
                    .GetAssemblies()
                    .SelectMany(a => a.GetExportedTypes());
            ModelMapper.AddMappings(mappingTypes);

            HbmMapping hbmMapping = ModelMapper.CompileMappingForAllExplicitlyAddedEntities();

            hbmMapping.defaultlazy = DefaultLazy;
            if (!string.IsNullOrWhiteSpace(DefaultAccess))
            {
                hbmMapping.defaultaccess = DefaultAccess;
            }

            if (!string.IsNullOrWhiteSpace(DefaultCascade))
            {
                hbmMapping.defaultcascade = DefaultCascade;
            }

            return hbmMapping;
        }

        protected virtual IEnumerable<Type> MappingTypes
        {
            get { return null; }
        }

        protected virtual string DefaultAccess
        {
            get { return null; }
        }

        protected virtual bool DefaultLazy
        {
            get { return true; }
        }

        protected virtual string DefaultCascade
        {
            get { return null; }
        }

        protected virtual void ModelMapperOnBeforeMapUnionSubclass(IModelInspector modelInspector, Type type, IUnionSubclassAttributesMapper unionSubclassCustomizer)
        {
        }

        protected virtual void OnBeforeMapSubclass(IModelInspector modelInspector, Type type, ISubclassAttributesMapper subclassCustomizer)
        {
        }

        protected virtual void OnBeforeMapSet(IModelInspector modelInspector, PropertyPath member, ISetPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnBeforeMapProperty(IModelInspector modelInspector, PropertyPath member, IPropertyMapper propertyCustomizer)
        {
        }

        protected virtual void OnBeforeMapOneToOne(IModelInspector modelInspector, PropertyPath member, IOneToOneMapper propertyCustomizer)
        {
        }

        protected virtual void OnBeforeMapOneToMany(IModelInspector modelInspector, PropertyPath member, IOneToManyMapper collectionRelationOneToManyCustomizer)
        {
        }

        protected virtual void OnBeforeMapMapKeyManyToMany(IModelInspector modelInspector, PropertyPath member, IMapKeyManyToManyMapper mapKeyManyToManyCustomizer)
        {
        }

        protected virtual void OnBeforeMapMapKey(IModelInspector modelInspector, PropertyPath member, IMapKeyMapper mapKeyElementCustomizer)
        {
        }

        protected virtual void OnBeforeMapMap(IModelInspector modelInspector, PropertyPath member, IMapPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnBeforeMapManyToOne(IModelInspector modelInspector, PropertyPath member, IManyToOneMapper propertyCustomizer)
        {
        }

        protected virtual void OnBeforeMapManyToMany(IModelInspector modelInspector, PropertyPath member, IManyToManyMapper collectionRelationManyToManyCustomizer)
        {
        }

        protected virtual void OnBeforeMapList(IModelInspector modelInspector, PropertyPath member, IListPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnBeforeMapJoinedSubclass(IModelInspector modelInspector, Type type, IJoinedSubclassAttributesMapper joinedSubclassCustomizer)
        {
        }

        protected virtual void OnBeforeMapIdBag(IModelInspector modelInspector, PropertyPath member, IIdBagPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnBeforeMapElement(IModelInspector modelInspector, PropertyPath member, IElementMapper collectionRelationElementCustomizer)
        {
        }

        protected virtual void OnBeforeMapComponent(IModelInspector modelInspector, PropertyPath member, IComponentAttributesMapper propertyCustomizer)
        {
        }

        protected virtual void OnBeforeMapClass(IModelInspector modelInspector, Type type, IClassAttributesMapper classCustomizer)
        {
        }

        protected virtual void OnBeforeMapBag(IModelInspector modelInspector, PropertyPath member, IBagPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnBeforeMapAny(IModelInspector modelInspector, PropertyPath member, IAnyMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapUnionSubclass(IModelInspector modelInspector, Type type, IUnionSubclassAttributesMapper unionSubclassCustomizer)
        {
        }

        protected virtual void OnAfterMapSubclass(IModelInspector modelInspector, Type type, ISubclassAttributesMapper subclassCustomizer)
        {
        }

        protected virtual void OnAfterMapSet(IModelInspector modelInspector, PropertyPath member, ISetPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapProperty(IModelInspector modelInspector, PropertyPath member, IPropertyMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapOneToOne(IModelInspector modelInspector, PropertyPath member, IOneToOneMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapOneToMany(IModelInspector modelInspector, PropertyPath member, IOneToManyMapper collectionRelationOneToManyCustomizer)
        {
        }

        protected virtual void OnAfterMapMapKeyManyToMany(IModelInspector modelInspector, PropertyPath member, IMapKeyManyToManyMapper mapKeyManyToManyCustomizer)
        {
        }

        protected virtual void OnAfterMapMapKey(IModelInspector modelInspector, PropertyPath member, IMapKeyMapper mapKeyElementCustomizer)
        {
        }

        protected virtual void OnAfterMapMap(IModelInspector modelInspector, PropertyPath member, IMapPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapManyToOne(IModelInspector modelInspector, PropertyPath member, IManyToOneMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapManyToMany(IModelInspector modelInspector, PropertyPath member, IManyToManyMapper collectionRelationManyToManyCustomizer)
        {
        }

        protected virtual void OnAfterMapList(IModelInspector modelInspector, PropertyPath member, IListPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapJoinedSubclass(IModelInspector modelInspector, Type type, IJoinedSubclassAttributesMapper joinedSubclassCustomizer)
        {
        }

        protected virtual void OnAfterMapIdBag(IModelInspector modelInspector, PropertyPath member, IIdBagPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapElement(IModelInspector modelInspector, PropertyPath member, IElementMapper collectionRelationElementCustomizer)
        {
        }

        protected virtual void OnAfterMapComponent(IModelInspector modelInspector, PropertyPath member, IComponentAttributesMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapClass(IModelInspector modelInspector, Type type, IClassAttributesMapper classCustomizer)
        {
        }

        protected virtual void OnAfterMapBag(IModelInspector modelInspector, PropertyPath member, IBagPropertiesMapper propertyCustomizer)
        {
        }

        protected virtual void OnAfterMapAny(IModelInspector modelInspector, PropertyPath member, IAnyMapper propertyCustomizer)
        {
        }
    }
}