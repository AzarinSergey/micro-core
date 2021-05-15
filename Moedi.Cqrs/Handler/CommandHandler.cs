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
        internal ICommandRepositoryFactory Uow;
        internal ILogger Logger;

        protected ILogger UseLogger => Logger;

        protected ICommandRepositoryFactory RepositoryFactory => Uow;

        protected CommandHandler()
        {
            EventList = new List<DomainEvent>();
        }

        protected ICommandRepository<T> GetRepository<T>()
            where T : class, IId
        {
            return Uow.GetRepository<T>();
        }

        internal List<DomainEvent> EventList;

        protected void AddEvent(DomainEvent e) => EventList.Add(e);

        protected void AddEvents(IEnumerable<DomainEvent> e) => EventList.AddRange(e);

        public abstract Task Execute(TCommand command, CancellationToken token);
    }
}
