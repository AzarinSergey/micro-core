using System;
using Core.Service.Interfaces;
using Microsoft.Extensions.Logging;
using Moedi.Cqrs.Messages;
using Moedi.Data.Core.Access;
using Moedi.Data.Core.Entity;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Moedi.Cqrs.Handler
{
    public abstract class CommandHandler<TCommand>
        where TCommand : DomainMessage
    {
        internal ILogger Logger;
        internal IExternalServiceProvider ExternalServiceProvider;
        internal CancellationToken CancellationToken;

        protected ILogger UseLogger => Logger;
        protected CancellationToken Token => CancellationToken;

        protected CommandHandler()
        {
            EventList = new List<DomainEvent>();
        }

        protected T UseExternalHttpService<T>() where T : IExternalHttpService 
            => ExternalServiceProvider.GetExternalHttpService<T>();

        internal readonly List<DomainEvent> EventList;

        protected void AddEvent(DomainEvent e) => EventList.Add(e);

        protected void AddEvents(IEnumerable<DomainEvent> e) => EventList.AddRange(e);

        /// <summary>
        /// Will call before transaction 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual Task BeforeExecute(Func<Func<IQueryRepositoryFactory, Task>, Task> query, TCommand command)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Will call after transaction created, if 'UseTransaction()' called.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public abstract Task Execute(ICommandRepositoryFactory factory, TCommand command);
    }
}
