using System;

namespace Duber.Infrastructure.EventBus.Idempotency
{
    public class IdempotentMessage
    {
        public string MessageId { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }
    }
}
