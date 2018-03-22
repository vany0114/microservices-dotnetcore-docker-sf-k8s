using System;
using Microsoft.Azure.ServiceBus;

namespace Duber.Infrastructure.EventBus.ServiceBus
{
    public interface IServiceBusPersisterConnection : IDisposable
    {
        ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder { get; }

        ITopicClient CreateModel();
    }
}