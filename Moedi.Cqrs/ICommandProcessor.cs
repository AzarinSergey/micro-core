using Moedi.Cqrs.Messages;
using System.Threading.Tasks;

namespace Moedi.Cqrs
{
    public interface ICommandProcessor<TCommand>
        where TCommand : DomainMessage
    {
        bool UseTransaction { set; }
        Task Process(TCommand command, CrossContext ctx);
        Task<DomainEvent[]> ProcessWithEvents(TCommand command, CrossContext ctx);
    }
}