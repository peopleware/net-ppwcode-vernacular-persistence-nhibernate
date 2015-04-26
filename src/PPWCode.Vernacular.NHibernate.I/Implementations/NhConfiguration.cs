// Copyright 2015 by PeopleWare n.v..
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

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;

using PPWCode.Vernacular.NHibernate.I.Interfaces;

namespace PPWCode.Vernacular.NHibernate.I.Implementations
{
    public class NhConfiguration : NhConfigurationBase
    {
        public NhConfiguration(INhInterceptor nhInterceptor, INhProperties nhProperties, IMappingAssemblies mappingAssemblies, IHbmMapping hbmMapping, IRegisterEventListener[] registerEventListeners)
            : base(nhInterceptor, nhProperties, mappingAssemblies, hbmMapping, registerEventListeners)
        {
        }

        protected override Configuration Configuration
        {
            get
            {
                Configuration result = new Configuration();

                // Overrule properties if necessary
                foreach (KeyValuePair<string, string> item in NhProperties.Properties)
                {
                    if (result.Properties.ContainsKey(item.Key))
                    {
                        if (string.IsNullOrWhiteSpace(item.Value))
                        {
                            result.Properties.Remove(item.Key);
                        }
                        else
                        {
                            result.SetProperty(item.Key, item.Value);
                        }
                    }
                    else
                    {
                        result.Properties.Add(item);
                    }
                }

                // Register interceptor / event-listeners
                IInterceptor interceptor = NhInterceptor.GetInterceptor();
                if (interceptor != null)
                {
                    result.SetInterceptor(interceptor);
                }

                foreach (IRegisterEventListener registerListener in RegisterEventListeners)
                {
                    registerListener.Register(result);
                }

                HbmMapping hbmMapping = HbmMapping.GetHbmMapping();
                if (hbmMapping != null)
                {
                    result.AddMapping(hbmMapping);
                }

                // map embedded resource of specified assemblies
                foreach (Assembly assembly in MappingAssemblies.GetAssemblies())
                {
                    result.AddAssembly(assembly);
                }

                // finally configure everything
                result.Configure();

                return result;
            }
        }
    }
}