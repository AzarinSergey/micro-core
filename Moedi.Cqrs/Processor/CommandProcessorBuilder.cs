using FluentValidation;
using Microsoft.Extensions.Logging;
using Moedi.Cqrs.Handler;
using Moedi.Cqrs.Messages;
using Moedi.Data.Core.Access;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Moedi.Cqrs.Processor
{
    public class CommandProcessorBuilder<TDomainMessage>
        where TDomainMessage : DomainMessage
    {
        private readonly CrossContext _ctx;
        private readonly IUowFactory _uowFactory;
        private readonly ILoggerFactory _loggerFactory;

        private TDomainMessage _domainMessage;
        private IValidator<TDomainMessage> _validator;
        private bool _useTransaction;

        public CommandProcessorBuilder(CrossContext ctx, IUowFactory uowFactory,
            ILoggerFactory loggerFactory)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx), "Correlation unreachable");
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public CommandProcessorBuilder<TDomainMessage> UseDomain(TDomainMessage domainMessage)
        {
            _domainMessage = domainMessage ?? throw new ArgumentNullException(nameof(domainMessage));
            return this;
        }

        public CommandProcessorBuilder<TDomainMessage> UseValidator(IValidator<TDomainMessage> validator)
        {
            _validator = validator;
            return this;
        }

        public CommandProcessorBuilder<TDomainMessage> UseTransaction()
        {
            _useTransaction = true;
            return this;
        }

        public Task Run(Func<CommandHandler<TDomainMessage>> handlerBuilder, CancellationToken token)
            => PrepareProcessor(handlerBuilder)
                .Process(_domainMessage, _ctx, token);

        public Task<DomainEvent[]> RunWithEvents(Func<CommandHandler<TDomainMessage>> handlerBuilder, CancellationToken token)
            => PrepareProcessor(handlerBuilder)
                .ProcessWithEvents(_domainMessage, _ctx, token);

        private ICommandProcessor<TDomainMessage> PrepareProcessor(Func<CommandHandler<TDomainMessage>> handlerBuilder)
        {
            if (_domainMessage == null)
            {
                throw new MemberAccessException("Required call 'UseDomain' should be done for command processing");
            }

            if (_validator != null)
            {
                var vResult = _validator.Validate(_domainMessage);
                if (!vResult.IsValid)
                    throw new ValidationException(vResult.Errors);
            }

            var processor = new DefaultCommandProcessor<TDomainMessage>(handlerBuilder, _loggerFactory, _uowFactory)
            {
                UseTransaction = _useTransaction
            };
            return processor;
        }
    }
}