﻿// Copyright 2017 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using NHibernate.Cfg;
using NHibernate.Event;

using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Castle Windsor usage")]
    [Serializable]
    public class CivilizedEventListener
        : IRegisterEventListener,
          IPreUpdateEventListener,
          IPreInsertEventListener
    {
        /// <summary>
        ///     Return true if the operation should be vetoed.
        /// </summary>
        /// <param name="event">The given event.</param>
        /// <param name="cancellationToken">The given cancellation token.</param>
        /// <returns>A boolean indicating whether the operation should be vetoed.</returns>
        [NotNull]
        public virtual Task<bool> OnPreInsertAsync([NotNull] PreInsertEvent @event, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<bool>(cancellationToken);
            }

            try
            {
                return Task.FromResult(OnPreInsert(@event));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return Task.FromException<bool>(ex);
            }
        }

        /// <summary>
        ///     Return true if the operation should be vetoed.
        /// </summary>
        /// <param name="event">The given event.</param>
        /// <returns>A boolean indicating whether the operation should be vetoed.</returns>
        public virtual bool OnPreInsert([NotNull] PreInsertEvent @event)
        {
            ValidateObject(@event.Entity);
            return false;
        }

        /// <summary>
        ///     Return true if the operation should be vetoed.
        /// </summary>
        /// <param name="event">The given event.</param>
        /// <param name="cancellationToken">The given cancellation token.</param>
        /// <returns>A boolean indicating whether the operation should be vetoed.</returns>
        [NotNull]
        public virtual Task<bool> OnPreUpdateAsync([NotNull] PreUpdateEvent @event, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<bool>(cancellationToken);
            }

            try
            {
                return Task.FromResult(OnPreUpdate(@event));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return Task.FromException<bool>(ex);
            }
        }

        /// <summary>
        ///     Return true if the operation should be vetoed.
        /// </summary>
        /// <param name="event">The given event.</param>
        /// <returns>A boolean indicating whether the operation should be vetoed.</returns>
        public virtual bool OnPreUpdate([NotNull] PreUpdateEvent @event)
        {
            ValidateObject(@event.Entity);
            return false;
        }

        public virtual void Register(Configuration cfg)
        {
            cfg.EventListeners.PreUpdateEventListeners = new IPreUpdateEventListener[] { this }
                .Concat(cfg.EventListeners.PreUpdateEventListeners)
                .ToArray();
            cfg.EventListeners.PreInsertEventListeners = new IPreInsertEventListener[] { this }
                .Concat(cfg.EventListeners.PreInsertEventListeners)
                .ToArray();
        }

        protected virtual void ValidateObject([CanBeNull] object entity)
            => (entity as ICivilizedObject)?.ThrowIfNotCivilized();
    }
}