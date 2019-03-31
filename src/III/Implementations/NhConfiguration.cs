// Copyright 2017 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Reflection;

using JetBrains.Annotations;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping;

namespace PPWCode.Vernacular.NHibernate.III
{
    public class NhConfiguration : NhConfigurationBase
    {
        public NhConfiguration(
            [NotNull] INhInterceptor nhInterceptor,
            [NotNull] INhProperties nhProperties,
            [NotNull] IMappingAssemblies mappingAssemblies,
            [NotNull] IPpwHbmMapping ppwHbmMapping,
            [NotNull] IRegisterEventListener[] registerEventListeners,
            [NotNull] IAuxiliaryDatabaseObject[] auxiliaryDatabaseObjects)
            : base(nhInterceptor, nhProperties, mappingAssemblies, ppwHbmMapping, registerEventListeners, auxiliaryDatabaseObjects)
        {
        }

        protected override Configuration Configuration
        {
            get
            {
                Configuration configuration = new Configuration();

                configuration.Configure();

                // Overrule properties if necessary
                foreach (KeyValuePair<string, string> item in NhProperties.GetProperties(configuration))
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

                // Register interceptor / event-listeners
                IInterceptor interceptor = NhInterceptor.GetInterceptor();
                if (interceptor != null)
                {
                    configuration.SetInterceptor(interceptor);
                }

                foreach (IRegisterEventListener registerListener in RegisterEventListeners)
                {
                    registerListener.Register(configuration);
                }

                HbmMapping hbmMapping = PpwHbmMapping.HbmMapping;
                if (hbmMapping != null)
                {
                    configuration.AddMapping(hbmMapping);
                }

                // map embedded resource of specified assemblies
                foreach (Assembly assembly in MappingAssemblies.GetAssemblies())
                {
                    configuration.AddAssembly(assembly);
                }

                foreach (IAuxiliaryDatabaseObject auxiliaryDatabaseObject in AuxiliaryDatabaseObjects)
                {
                    IPpwAuxiliaryDatabaseObject ppwAuxiliaryDatabaseObject = auxiliaryDatabaseObject as IPpwAuxiliaryDatabaseObject;
                    ppwAuxiliaryDatabaseObject?.SetConfiguration(configuration);
                    configuration.AddAuxiliaryDatabaseObject(auxiliaryDatabaseObject);
                }

                return configuration;
            }
        }
    }
}
