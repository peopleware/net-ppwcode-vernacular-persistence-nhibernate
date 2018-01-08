// Copyright 2018 by PeopleWare n.v..
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

using NHibernate.Mapping.ByCode;

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.NHibernate.I.MappingByCode;
using PPWCode.Vernacular.NHibernate.I.Utilities;

namespace PPWCode.Vernacular.NHibernate.I.Tests
{
    public class TestsSimpleModelMapper : SimpleModelMapper
    {
        public const string GeneratorTableName = "HIBERNATE_HI_LO";
        public const string GeneratorNextHiColumnName = "NEXT_HI";
        public const string GeneratorEntityNameColumnName = "ENTITY_NAME";
        public const string GeneratorTableNameColumnName = "TABLE_NAME";
        public const int GeneratorMaxLo = 999;

        public TestsSimpleModelMapper(IMappingAssemblies mappingAssemblies)
            : base(mappingAssemblies)
        {
        }

        protected override string DefaultSchemaName => @"dbo";

        public override bool QuoteIdentifiers => false;

        protected override void OnBeforeMapClass(IModelInspector modelInspector, Type type, IClassAttributesMapper classCustomizer)
        {
            base.OnBeforeMapClass(modelInspector, type, classCustomizer);

            classCustomizer
                .Id(idMapper =>
                    {
                        idMapper.Generator(
                            Generators.HighLow,
                            generatorMapper =>
                                generatorMapper.Params(
                                    new
                                    {
                                        table = GeneratorTableName,
                                        column = GeneratorNextHiColumnName,
                                        max_lo = GeneratorMaxLo,
                                        where = $"{GeneratorEntityNameColumnName} = '{type.FullName}'"
                                    }));
                    });
        }
    }

    public class TestHighLowPerTableAuxiliaryDatabaseObject
        : HighLowPerTableAuxiliaryDatabaseObject
    {
        public TestHighLowPerTableAuxiliaryDatabaseObject(IPpwHbmMapping ppwHbmMapping)
            : base(ppwHbmMapping)
        {
        }

        protected override string GeneratorTableName => TestsSimpleModelMapper.GeneratorTableName;

        protected override string GeneratorEntityNameColumnName => TestsSimpleModelMapper.GeneratorEntityNameColumnName;

        protected override string GeneratorNextHiColumnName => TestsSimpleModelMapper.GeneratorNextHiColumnName;

        protected override string GeneratorTableNameColumnName => TestsSimpleModelMapper.GeneratorTableNameColumnName;
    }
}
