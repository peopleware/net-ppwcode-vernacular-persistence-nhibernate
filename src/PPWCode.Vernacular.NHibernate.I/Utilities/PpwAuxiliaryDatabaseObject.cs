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

using NHibernate.Cfg;
using NHibernate.Mapping;

using PPWCode.Vernacular.NHibernate.I.Interfaces;

namespace PPWCode.Vernacular.NHibernate.I.Utilities
{
    public abstract class PpwAuxiliaryDatabaseObject
        : AbstractAuxiliaryDatabaseObject,
          IAuxiliaryDatabaseObjectEx
    {
        private Configuration m_Configuration;
        public IHbmMapping HbmMapping { get; }

        protected Configuration Configuration => m_Configuration;

        protected PpwAuxiliaryDatabaseObject(IHbmMapping hbmMapping)
        {
            HbmMapping = hbmMapping;
        }

        public void SetConfiguration(Configuration configuration)
        {
            m_Configuration = configuration;
        }
    }
}
