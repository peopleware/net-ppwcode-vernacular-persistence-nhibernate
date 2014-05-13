using System.Diagnostics.Contracts;

using NHibernate.Cfg;

namespace PPWCode.Vernacular.nHibernate.I.Interfaces
{
    [ContractClass(typeof(INhConfigurationContract))]
    public interface INhConfiguration
    {
        Configuration GetConfiguration();
    }

    // ReSharper disable once InconsistentNaming
    [ContractClassFor(typeof(INhConfiguration))]
    public abstract class INhConfigurationContract : INhConfiguration
    {
        public Configuration GetConfiguration()
        {
            Contract.Ensures(Contract.Result<Configuration>() != null);

            return default(Configuration);
        }
    }
}