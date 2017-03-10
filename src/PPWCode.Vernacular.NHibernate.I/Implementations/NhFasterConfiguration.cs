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

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

using Castle.Core.Logging;

using NHibernate.Cfg;

using PPWCode.Vernacular.NHibernate.I.Interfaces;

using Environment = System.Environment;

namespace PPWCode.Vernacular.NHibernate.I.Implementations
{
    public abstract class NhFasterConfiguration : NhConfiguration
    {
        private const string ConfigFile = "hibernate.cfg.xml";
        private ILogger m_Logger = new NullLogger();

        protected NhFasterConfiguration(INhInterceptor nhInterceptor, INhProperties nhProperties, IMappingAssemblies mappingAssemblies, IHbmMapping hbmMapping, IRegisterEventListener[] registerEventListeners)
            : base(nhInterceptor, nhProperties, mappingAssemblies, hbmMapping, registerEventListeners)
        {
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Logger != null);
        }

        public ILogger Logger
        {
            get
            {
                Contract.Ensures(Contract.Result<ILogger>() != null);

                return m_Logger;
            }

            set
            {
                Contract.Requires(value != null);
                Contract.Ensures(value == Logger);

                m_Logger = value;
            }
        }

        [Pure]
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

                return serializedConfigInfo.LastWriteTime >= maxDate
                       && serializedConfigInfo.LastWriteTime >= nHibernateConfigFileInfo.LastWriteTime;
            }
        }

        private Configuration LoadConfigurationFromFile()
        {
            Contract.Ensures(!IsConfigurationFileValid || Contract.Result<Configuration>() != null);

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
                    Logger.Error(e.Message, e);
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

        private string SerializedConfiguration
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Namespace, "hibernate.cfg.bin"); }
        }

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
    }
}
