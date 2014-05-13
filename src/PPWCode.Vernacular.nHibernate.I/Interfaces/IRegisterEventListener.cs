using System.Diagnostics.Contracts;

using NHibernate.Cfg;

namespace PPWCode.Vernacular.nHibernate.I.Interfaces
{
    [ContractClass(typeof(IRegisterEventListenerContract))]
    public interface IRegisterEventListener
    {
        void Register(Configuration cfg);
    }

    // ReSharper disable once InconsistentNaming
    [ContractClassFor(typeof(IRegisterEventListener))]
    public abstract class IRegisterEventListenerContract : IRegisterEventListener
    {
        public void Register(Configuration cfg)
        {
            Contract.Requires(cfg != null);
        }
    }
}