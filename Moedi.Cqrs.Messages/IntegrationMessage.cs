namespace Moedi.Cqrs.Messages
{
    public abstract class IntegrationMessage
    {
        public CrossContext CrossContext { get; set; }
    }
}