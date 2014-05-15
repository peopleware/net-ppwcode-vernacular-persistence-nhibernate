using System.Security.Principal;

using PPWCode.Vernacular.nHibernate.I.Interfaces;

namespace PPWCode.Vernacular.nHibernate.I.Tests.Audit
{
    public class IdentityProvider : IIdentityProvider
    {
        public string IdentityName
        {
            get
            {
                WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
                return windowsIdentity != null ? windowsIdentity.Name : "(null)";
            }
        }
    }
}