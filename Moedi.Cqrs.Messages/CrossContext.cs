using System;

namespace Moedi.Cqrs.Messages
{
    public class CrossContext
    {
        public CrossContext()
        {
            CreatedAt = DateTime.Now;
        }

        public Guid CorrelationUuid { get; set; }
        public DateTime CreatedAt { get; }
    }
}