using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Weapsy.Cqrs.EventStore.CosmosDB.MongoDB;
using Weapsy.Cqrs.EventStore.CosmosDB.MongoDB.Configuration;
using Weapsy.Cqrs.EventStore.CosmosDB.MongoDB.Documents;
// ReSharper disable FunctionRecursiveOnAllPaths

namespace Duber.Trip.API.Infrastructure.Repository
{
    public class EventStoreRepository : IEventStoreRepository
    {
        private readonly EventStoreDbContext _dbContext;

        public EventStoreRepository(IOptions<EventStoreConfiguration> settings)
        {
            _dbContext = new EventStoreDbContext(settings);
        }

        public async Task<IEnumerable<AggregateDocument>> GetAggregatesAsync()
        {
            var aggregateFilter = Builders<AggregateDocument>.Filter.Empty;
            return await _dbContext.Aggregates.Find(aggregateFilter).ToListAsync();
        }

        public async Task<IEnumerable<EventDocument>> GetEventsAsync(Guid aggregateId)
        {
            var eventFilter = Builders<EventDocument>.Filter.Eq("aggregateId", aggregateId.ToString());
            return await _dbContext.Events.Find(eventFilter).ToListAsync();
        }
    }
}