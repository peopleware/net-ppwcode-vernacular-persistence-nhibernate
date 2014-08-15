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

using System.Collections.Generic;
using System.Reflection;

using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;

using PPWCode.Vernacular.NHibernate.I.Interfaces;

namespace PPWCode.Vernacular.NHibernate.I.Implementations
{
    public abstract class NhConfiguration : NhConfigurationBase
    {
        protected NhConfiguration(INhInterceptor nhInterceptor, INhProperties nhProperties, IMappingAssemblies mappingAssemblies, IHbmMapping hbmMapping, IRegisterEventListener[] registerEventListeners)
            : base(nhInterceptor, nhProperties, mappingAssemblies, hbmMapping, registerEventListeners)
        {
        }

        protected override Configuration Configuration
        {
            get
            {
                Configuration configuration = new Configuration();
                configuration.Configure();
                foreach (KeyValuePair<string, string> item in NhProperties.Properties)
                {
                    if (configuration.Properties.ContainsKey(item.Key))
                    {
                        if (string.IsNullOrWhiteSpace(item.Value))
                        {
                            configuration.Properties.Remove(item.Key);
                        }
                        else
                        {
                            configuration.SetProperty(item.Key, item.Value);
                        }
                    }
                    else
                    {
                        configuration.Properties.Add(item);
                    }
                }

                HbmMapping hbmMapping = HbmMapping.GetHbmMapping();
                if (hbmMapping != null)
                {
                    configuration.AddMapping(hbmMapping);
                }

                foreach (Assembly assembly in MappingAssemblies.GetAssemblies())
                {
                    configuration.AddAssembly(assembly);
                }

                return configuration;
            }
        }
    }
}