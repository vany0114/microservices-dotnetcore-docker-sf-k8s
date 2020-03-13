using System;
using System.Threading.Tasks;
using Duber.Infrastructure.EventBus.Abstractions;
using Duber.Infrastructure.EventBus.Events;

namespace Duber.Infrastructure.EventBus.Idempotency
{
    /// <summary>
    /// This handler acts as an envelop to ensure idepotency to the given <typeparamref name="T"/> IntegrationEvent
    /// </summary>
    /// <typeparam name="T">IntegrationEvent</typeparam>
    public class IdempotentIntegrationEventHandler<T> : IIntegrationEventHandler<IdempotentIntegrationEvent<T>>
        where T : IntegrationEvent
    {
        private readonly IEventBus _eventBus;
        private readonly IIdempotencyStoreProvider _storeProvider;

        public IdempotentIntegrationEventHandler(IEventBus eventBus, IIdempotencyStoreProvider storeProvider)
        {
            _eventBus = eventBus;
            _storeProvider = storeProvider;
        }

        protected virtual void HandleDuplicatedRequest(T message)
        {
        }

        public async Task Handle(IdempotentIntegrationEvent<T> message)
        {
            var alreadyExists = await _storeProvider.ExistsAsync(message.MessageId);
            if (alreadyExists)
            {
                HandleDuplicatedRequest(message.Event);
            }
            else
            {
                await _storeProvider.SaveAsync(new IdempotentMessage{ MessageId = message.MessageId, Name = typeof(T).Name, Time = DateTime.UtcNow });
                _eventBus.Publish(message.Event);
            }
        }
    }
}
