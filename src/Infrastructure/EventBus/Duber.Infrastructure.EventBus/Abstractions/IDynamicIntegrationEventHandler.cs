using System.Threading.Tasks;

namespace Duber.Infrastructure.EventBus.Abstractions
{
    public interface IDynamicIntegrationEventHandler
    {
        Task Handle(dynamic eventData);
    }
}
