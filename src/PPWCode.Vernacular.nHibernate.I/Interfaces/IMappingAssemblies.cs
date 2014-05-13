using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace PPWCode.Vernacular.nHibernate.I.Interfaces
{
    [ContractClass(typeof(IMappingAssembliesContract))]
    public interface IMappingAssemblies
    {
        IEnumerable<Assembly> GetAssemblies();
    }

    // ReSharper disable once InconsistentNaming
    [ContractClassFor(typeof(IMappingAssemblies))]
    public abstract class IMappingAssembliesContract : IMappingAssemblies
    {
        public IEnumerable<Assembly> GetAssemblies()
        {
            Contract.Ensures(Contract.Result<IEnumerable<Assembly>>() != null);

            return default(IEnumerable<Assembly>);
        }
    }
}