using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenCqrs.Store.Cosmos.Mongo.Documents;

namespace Duber.Trip.API.Infrastructure.Repository
{
    public interface IEventStoreRepository
    {
        Task<IEnumerable<AggregateDocument>> GetAggregatesAsync();

        Task<IEnumerable<EventDocument>> GetEventsAsync(Guid aggregateId);
    }
}