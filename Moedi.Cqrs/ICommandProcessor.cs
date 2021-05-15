using System.Threading;
using System.Threading.Tasks;
using Moedi.Cqrs.Messages;

namespace Moedi.Cqrs
{
    public interface ICommandProcessor<TCommand>
        where TCommand : DomainMessage
    {
        bool UseTransaction { set; }
        Task Process(TCommand command, CrossContext ctx, CancellationToken token);
        Task<DomainEvent[]> ProcessWithEvents(TCommand command, CrossContext ctx, CancellationToken token);
    }
}