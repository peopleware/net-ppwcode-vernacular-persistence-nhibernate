// Copyright 2020 by PeopleWare n.v..
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

using JetBrains.Annotations;

using NHibernate.Dialect;
using NHibernate.Mapping.ByCode;

using PPWCode.Vernacular.NHibernate.II.MappingByCode;

namespace PPWCode.Vernacular.NHibernate.II.Tests
{
    public class TestsSimpleModelMapper : SimpleModelMapper
    {
        public const string GeneratorTableName = "HibernateHiLo";
        public const string GeneratorNextHiColumnName = "NextHi";
        public const string GeneratorEntityNameColumnName = "EntityName";
        public const string GeneratorTableNameColumnName = "TableName";
        public const int GeneratorMaxLo = 999;

        public TestsSimpleModelMapper([NotNull] IMappingAssemblies mappingAssemblies)
            : base(mappingAssemblies)
        {
        }

        protected override int? ClassBatchSize
            => 40;

        protected override int? CollectionBatchSize
            => ClassBatchSize;

        protected override bool DynamicInsert
            => true;

        protected override bool DynamicUpdate
            => true;

        protected override bool AdjustColumnForForeignGenerator
            => true;

        public override bool UseCamelCaseUnderScoreForDbObjects
            => false;

        public override bool QuoteIdentifiers
            => true;

        protected override string DefaultSchemaName
            => "dbo";

        protected override void OnBeforeMapClass(IModelInspector modelInspector, Type type, IClassAttributesMapper classCustomizer)
        {
            base.OnBeforeMapClass(modelInspector, type, classCustomizer);

            classCustomizer
                .Id(m =>
                    {
                        m.Generator(
                            Generators.HighLow,
                            generatorMapper =>
                                generatorMapper.Params(
                                    new
                                    {
                                        table = GetIdentifier(GeneratorTableName),
                                        column = GetIdentifier(GeneratorNextHiColumnName),
                                        max_lo = GeneratorMaxLo,
                                        where = $"{GetIdentifier(GeneratorEntityNameColumnName)} = '{type.FullName}'"
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

        protected override string GeneratorTableName
            => TestsSimpleModelMapper.GeneratorTableName;

        protected override string GeneratorEntityNameColumnName
            => TestsSimpleModelMapper.GeneratorEntityNameColumnName;

        protected override string GeneratorNextHiColumnName
            => TestsSimpleModelMapper.GeneratorNextHiColumnName;

        protected override string GeneratorTableNameColumnName
            => TestsSimpleModelMapper.GeneratorTableNameColumnName;

        /// <inheritdoc />
        protected override int GeneratorEntityNameColumnLength(Dialect dialect)
            => 255;

        /// <inheritdoc />
        protected override int GeneratorTableNameColumnLength(Dialect dialect)
            => dialect.MaxAliasLength;
    }
}
