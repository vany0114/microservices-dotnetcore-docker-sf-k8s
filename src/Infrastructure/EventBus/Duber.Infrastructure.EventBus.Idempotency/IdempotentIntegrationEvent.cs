using System;
using Duber.Infrastructure.EventBus.Events;

namespace Duber.Infrastructure.EventBus.Idempotency
{
    public class IdempotentIntegrationEvent<T> : IntegrationEvent
        where T : IntegrationEvent
    {
        public T Event { get; }

        public string MessageId { get; }

        public IdempotentIntegrationEvent(T @event, string messageId)
        {
            Event = @event;
            MessageId = messageId;
        }
    }
}
