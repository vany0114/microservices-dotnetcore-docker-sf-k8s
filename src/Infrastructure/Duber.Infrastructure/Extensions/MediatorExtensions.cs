using System.Linq;
using System.Threading.Tasks;
using Duber.Infrastructure.DDD;
using MediatR;

namespace Duber.Infrastructure.Extensions
{
    public static class MediatorExtensions
    {
        public static async Task DispatchDomainEventsAsync(this IMediator mediator, Entity entity)
        {
            var domainEvents = entity.DomainEvents?.ToList();
            if (domainEvents == null || domainEvents.Count == 0)
                return;

            entity.DomainEvents.Clear();
            var tasks = domainEvents
                .Select(async domainEvent =>
                {
                    await mediator.Publish(domainEvent);
                });

            await Task.WhenAll(tasks);
        }
    }
}
