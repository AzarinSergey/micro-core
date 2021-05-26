using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moedi.Cqrs.Messages;
using System;
using System.Threading;

namespace Core.Service.Host
{
    public abstract class AbstractController : ControllerBase
    {
        protected abstract string ServiceUuid { get; }

        protected readonly ILogger<AbstractController> Logger;

        private CrossContext _ctx;
        private Guid? _correlationUuid;

        protected CrossContext CrossContext(CancellationToken token)
        {
            _correlationUuid ??= Guid.NewGuid();
            _ctx ??= new CrossContext(ServiceUuid, _correlationUuid.Value, token);

            return _ctx;
        }

        protected AbstractController(ILogger<AbstractController> logger)
        {
            Logger = logger;
        }
    }
}