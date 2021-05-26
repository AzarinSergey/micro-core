using Moedi.Cqrs.Handler;
using Moedi.Cqrs.Messages;
using Moedi.Cqrs.Processor;
using System.Threading.Tasks;

namespace Moedi.Cqrs
{
    public interface IProcessorFactory
    {
        CommandProcessorBuilder<TDomainMessage> Command<TDomainMessage>(object model, CrossContext ctx)
            where TDomainMessage : DomainMessage;

        Task<T> Query<T>(CrossContext ctx, QueryHandler<T> handler);
    }
}