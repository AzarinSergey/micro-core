using Microsoft.Extensions.Logging;
using Moedi.Cqrs.Handler;
using Moedi.Cqrs.Messages;
using Moedi.Data.Core.Access;
using System;
using System.Threading.Tasks;

namespace Moedi.Cqrs.Processor
{
    public class DefaultCommandProcessor<TCommand> : ICommandProcessor<TCommand>
        where TCommand : DomainMessage
    {
        private readonly Func<CommandHandler<TCommand>> _handlerBuilder;
        private readonly IExternalServiceProvider _externalServiceProvider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IUowFactory _uowfactory;

        public DefaultCommandProcessor(Func<CommandHandler<TCommand>> handlerBuilder,
            IExternalServiceProvider externalServiceProvider, ILoggerFactory loggerFactory, IUowFactory uowFactory)
        {
            _handlerBuilder = handlerBuilder;
            _externalServiceProvider = externalServiceProvider;
            _loggerFactory = loggerFactory;
            _uowfactory = uowFactory;
        }

        public bool UseTransaction { get; set; }

        public Task Process(TCommand command, CrossContext ctx)
            => ProcessWithEvents(command, ctx);

        public async Task<DomainEvent[]> ProcessWithEvents(TCommand command, CrossContext ctx)
        {
            var logger = _loggerFactory.CreateLogger($"CommandProcessor[{nameof(command)}][{ctx.CorrelationUuid}]");
            IUow uow = null;
            try
            {
                logger.LogInformation($"Started at {DateTime.Now}");
                var handler = _handlerBuilder();
                handler.Logger = _loggerFactory.CreateLogger(nameof(handler));
                handler.ExternalServiceProvider = _externalServiceProvider;
                handler.CancellationToken = ctx.Token;

                await handler.BeforeExecute(async func =>
                {
                    using (var nonTransactionalUow = _uowfactory.CreateUnitOfWork(null, ctx.Token))
                    {
                        await func(nonTransactionalUow);
                    }
                }, command);

                var transactionUuid = UseTransaction ? Guid.NewGuid() : (Guid?)null;
                uow = _uowfactory.CreateUnitOfWork(transactionUuid, ctx.Token);

                await handler.Execute(uow, command);

                await uow.Commit();

                return handler.EventList.ToArray();
            }
            catch (Exception e)
            {
                var t = uow?.Rollback();
                if (t != null)
                    await t;

                logger.LogError("{0}\n{1}", e.Message, e.StackTrace);
                throw;
            }
            finally
            {
                uow?.Dispose();

                logger.LogInformation($"Done at {DateTime.Now}");
            }
        }
    }
}
