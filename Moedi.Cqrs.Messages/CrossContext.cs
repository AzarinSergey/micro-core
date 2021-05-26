using System;
using System.Threading;

namespace Moedi.Cqrs.Messages
{
    public class CrossContext
    {
        public CrossContext(string serviceName, Guid correlationUuid, CancellationToken token)
        {
            ServiceName = serviceName;
            CreatedAt = DateTime.Now;
            Token = token;
            CorrelationUuid = correlationUuid;
        }

        public Guid CorrelationUuid { get; }
        public DateTime CreatedAt { get; }
        public string ServiceName { get; }
        public CancellationToken Token { get; set; }
    }
}