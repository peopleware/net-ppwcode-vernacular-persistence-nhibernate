using System.Collections.Generic;
using System.Linq;

using PPWCode.Vernacular.nHibernate.I.Interfaces;

namespace PPWCode.Vernacular.nHibernate.I.Implementations
{
    public class NhProperties : INhProperties
    {
        public IEnumerable<KeyValuePair<string, string>> Properties
        {
            get { return Enumerable.Empty<KeyValuePair<string, string>>(); }
        }
    }
}