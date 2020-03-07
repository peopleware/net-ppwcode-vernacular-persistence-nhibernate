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

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

using Common.Logging;

using JetBrains.Annotations;

using NHibernate.Cfg;
using NHibernate.Mapping;

using Environment = System.Environment;

namespace PPWCode.Vernacular.NHibernate.III
{
    public abstract class NhFasterConfiguration : NhConfiguration
    {
        private const string ConfigFile = "hibernate.cfg.xml";

        [NotNull]
        private static readonly ILog _logger = LogManager.GetLogger<NhFasterConfiguration>();

        protected NhFasterConfiguration(
            [NotNull] INhInterceptor nhInterceptor,
            [NotNull] INhProperties nhProperties,
            [NotNull] IMappingAssemblies mappingAssemblies,
            [NotNull] IPpwHbmMapping ppwHbmMapping,
            [NotNull] IRegisterEventListener[] registerEventListeners,
            [NotNull] IAuxiliaryDatabaseObject[] auxiliaryDatabaseObjects)
            : base(nhInterceptor, nhProperties, mappingAssemblies, ppwHbmMapping, registerEventListeners, auxiliaryDatabaseObjects)
        {
        }

        private bool IsConfigurationFileValid
        {
            get
            {
                Assembly callingAssembly = Assembly.GetCallingAssembly();
                DateTime maxDate =
                    new[] { callingAssembly }
                        .Union(MappingAssemblies.GetAssemblies())
                        .Select(a => new FileInfo(a.Location))
                        .Max(fi => fi.LastWriteTime);
                FileInfo serializedConfigInfo = new FileInfo(SerializedConfiguration);
                FileInfo nHibernateConfigFileInfo = new FileInfo(ConfigFile);

                return (serializedConfigInfo.LastWriteTime >= maxDate)
                       && (serializedConfigInfo.LastWriteTime >= nHibernateConfigFileInfo.LastWriteTime);
            }
        }

        private string SerializedConfiguration
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Namespace, "hibernate.cfg.bin");

        protected abstract string Namespace { get; }

        protected override Configuration Configuration
        {
            get
            {
                Configuration result = LoadConfigurationFromFile();
                if (result == null)
                {
                    result = base.Configuration;
                    SaveConfigurationToFile(result);
                }

                return result;
            }
        }

        private Configuration LoadConfigurationFromFile()
        {
            Configuration result = null;
            if (IsConfigurationFileValid)
            {
                try
                {
                    using (FileStream file = File.Open(SerializedConfiguration, FileMode.Open))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        result = bf.Deserialize(file) as Configuration;
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e.Message, e);
                }
            }

            return result;
        }

        private void SaveConfigurationToFile(Configuration configuration)
        {
            using (FileStream file = File.Open(SerializedConfiguration, FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(file, configuration);
            }
        }
    }
}
