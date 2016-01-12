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

using NHibernate.Mapping.ByCode;

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.NHibernate.I.MappingByCode;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Models.Mapping
{
    public class TestsSimpleModelMapper : SimpleModelMapper
    {
        public TestsSimpleModelMapper(IMappingAssemblies mappingAssemblies)
            : base(mappingAssemblies)
        {
        }

        protected override void OnBeforeMapClass(IModelInspector modelInspector, Type type, IClassAttributesMapper classCustomizer)
        {
            base.OnBeforeMapClass(modelInspector, type, classCustomizer);

            classCustomizer.Schema(@"dbo");
        }

        protected override string GetTableName(IModelInspector modelInspector, Type type)
        {
            return string.Format("`{0}`", base.GetTableName(modelInspector, type));
        }

        protected override string GetColumnName(IModelInspector modelInspector, PropertyPath member)
        {
            return string.Format("`{0}`", base.GetColumnName(modelInspector, member));
        }

        public override string GetDiscriminatorColumnName(IModelInspector modelInspector, Type type)
        {
            return string.Format("`{0}`", base.GetDiscriminatorColumnName(modelInspector, type));
        }
    }
}