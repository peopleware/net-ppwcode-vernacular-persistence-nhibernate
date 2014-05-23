using NHibernate;

namespace PPWCode.Vernacular.nHibernate.I.Interfaces
{
    public interface INHibernateSessionFactory
    {
        ISessionFactory SessionFactory { get; }
    }
}