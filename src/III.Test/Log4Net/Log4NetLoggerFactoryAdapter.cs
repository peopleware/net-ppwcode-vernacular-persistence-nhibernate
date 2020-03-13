using System.IO;
using System.Reflection;

using Common.Logging;
using Common.Logging.Factory;

using JetBrains.Annotations;

using log4net.Config;
using log4net.Repository;

using LogManager = log4net.LogManager;

namespace PPWCode.Vernacular.NHibernate.III.Test.Log4Net
{
    public class Log4NetLoggerFactoryAdapter : AbstractCachingLoggerFactoryAdapter
    {
        private readonly ILoggerRepository _logRepository;
        public Log4NetLoggerFactoryAdapter(
            [NotNull] Assembly repositoryAssembly,
            [CanBeNull] string log4NetConfigFile)
            : base(true)
        {
            _logRepository = LogManager.GetRepository(repositoryAssembly);
            XmlConfigurator.Configure(_logRepository, new FileInfo(log4NetConfigFile ?? "log4net.config"));
        }

        /// <summary>
        ///     Create a ILog instance by name
        /// </summary>
        /// <param name="name">Namespace name</param>
        /// <returns>An instance of <see cref="ILog" /></returns>
        protected override ILog CreateLogger(string name)
            => new Log4NetLogger(LogManager.GetLogger(_logRepository.Name, name));
    }
}
