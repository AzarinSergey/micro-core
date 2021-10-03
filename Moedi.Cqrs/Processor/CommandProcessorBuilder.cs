using FluentValidation;
using Microsoft.Extensions.Logging;
using Moedi.Cqrs.Handler;
using Moedi.Cqrs.Messages;
using Moedi.Data.Core.Access;
using System;
using System.Threading.Tasks;
using Core.Service.Interfaces;

namespace Moedi.Cqrs.Processor
{
    public class CommandProcessorBuilder<TDomainMessage>
        where TDomainMessage : DomainMessage
    {
        private readonly CrossContext _ctx;
        private readonly IExternalServiceProvider _externalServiceProvider;
        private readonly IUowFactory _uowFactory;
        private readonly ILoggerFactory _loggerFactory;

        private TDomainMessage _domainMessage;
        private Func<IValidator<TDomainMessage>> _validator;
        private bool _useTransaction;

        public CommandProcessorBuilder(CrossContext ctx, IExternalServiceProvider externalServiceProvider,
            IUowFactory uowFactory,
            ILoggerFactory loggerFactory)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx), "Correlation unreachable");
            _externalServiceProvider = externalServiceProvider ?? throw new ArgumentNullException(nameof(externalServiceProvider));
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public CommandProcessorBuilder<TDomainMessage> UseDomain(TDomainMessage domainMessage)
        {
            _domainMessage = domainMessage ?? throw new ArgumentNullException(nameof(domainMessage));
            return this;
        }

        public CommandProcessorBuilder<TDomainMessage> UseValidator<T>()
            where T : IValidator<TDomainMessage>, new()
        {
            _validator = () => new T();
            return this;
        }

        public CommandProcessorBuilder<TDomainMessage> UseTransaction()
        {
            _useTransaction = true;
            return this;
        }

        public Task Run(Func<CommandHandler<TDomainMessage>> handlerBuilder)
            => PrepareProcessor(handlerBuilder)
                .Process(_domainMessage, _ctx);

        public Task<DomainEvent[]> RunWithEvents(Func<CommandHandler<TDomainMessage>> handlerBuilder)
            => PrepareProcessor(handlerBuilder)
                .ProcessWithEvents(_domainMessage, _ctx);

        private ICommandProcessor<TDomainMessage> PrepareProcessor(Func<CommandHandler<TDomainMessage>> handlerBuilder)
        {
            if (_domainMessage == null)
            {
                throw new MemberAccessException("Required call 'UseDomain' should be done for command processing");
            }

            if (_validator != null)
            {
                var vResult = _validator().Validate(_domainMessage);
                if (!vResult.IsValid)
                    throw new ValidationException(vResult.Errors);
            }

            var processor = new DefaultCommandProcessor<TDomainMessage>(handlerBuilder, _externalServiceProvider, _loggerFactory, _uowFactory)
            {
                UseTransaction = _useTransaction
            };

            return processor;
        }
    }
}