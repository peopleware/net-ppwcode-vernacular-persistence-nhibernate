using System.Linq;

using NHibernate.Cfg;
using NHibernate.Event;

using PPWCode.Vernacular.nHibernate.I.Interfaces;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.nHibernate.I.Implementations
{
    public class CivilizedEventListener :
        IRegisterEventListener,
        IPreUpdateEventListener,
        IPreInsertEventListener
    {
        /// <summary>
        /// Return true if the operation should be vetoed
        /// </summary>
        /// <param name="event"/>
        public bool OnPreInsert(PreInsertEvent @event)
        {
            ValidateObject(@event.Entity);
            return false;
        }

        /// <summary>
        /// Return true if the operation should be vetoed
        /// </summary>
        /// <param name="event"/>
        public bool OnPreUpdate(PreUpdateEvent @event)
        {
            ValidateObject(@event.Entity);
            return false;
        }

        public void Register(Configuration cfg)
        {
            cfg.EventListeners.PreUpdateEventListeners = new IPreUpdateEventListener[] { this }
                .Concat(cfg.EventListeners.PreUpdateEventListeners)
                .ToArray();
            cfg.EventListeners.PreInsertEventListeners = new IPreInsertEventListener[] { this }
                .Concat(cfg.EventListeners.PreInsertEventListeners)
                .ToArray();
        }

        private void ValidateObject(object entity)
        {
            ICivilizedObject civilizedObject = entity as ICivilizedObject;
            if (civilizedObject != null)
            {
                civilizedObject.ThrowIfNotCivilized();
            }
        }
    }
}