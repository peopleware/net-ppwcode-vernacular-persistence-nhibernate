using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace PPWCode.Vernacular.nHibernate.I.Interfaces
{
    [ContractClass(typeof(INhPropertiesContract))]
    public interface INhProperties
    {
        IEnumerable<KeyValuePair<string, string>> Properties { get; }
    }

    // ReSharper disable once InconsistentNaming
    [ContractClassFor(typeof(INhProperties))]
    public abstract class INhPropertiesContract : INhProperties
    {
        public IEnumerable<KeyValuePair<string, string>> Properties
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<KeyValuePair<string, string>>>() != null);

                return default(IEnumerable<KeyValuePair<string, string>>);
            }
        }
    }
}