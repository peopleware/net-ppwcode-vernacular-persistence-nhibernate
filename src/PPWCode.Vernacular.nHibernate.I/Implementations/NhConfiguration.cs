using System.Collections.Generic;
using System.Reflection;

using NHibernate;
using NHibernate.Cfg;

using PPWCode.Vernacular.nHibernate.I.Interfaces;

namespace PPWCode.Vernacular.nHibernate.I.Implementations
{
    public class NhConfiguration : NhConfigurationBase
    {
        public NhConfiguration(IInterceptor interceptor, INhProperties nhProperties, IMappingAssemblies mappingAssemblies, IRegisterEventListener[] registerEventListeners)
            : base(interceptor, nhProperties, mappingAssemblies, registerEventListeners)
        {
        }

        protected override Configuration Configuration
        {
            get
            {
                Configuration result = new Configuration();
                result.Configure();
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
                foreach (Assembly assembly in MappingAssemblies.GetAssemblies())
                {
                    result.AddAssembly(assembly);
                }
                return result;
            }
        }
    }
}