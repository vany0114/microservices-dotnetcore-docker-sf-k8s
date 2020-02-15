using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kledex.Store.Cosmos.Mongo;
using Kledex.Store.Cosmos.Mongo.Configuration;
using Kledex.Store.Cosmos.Mongo.Documents;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
// ReSharper disable FunctionRecursiveOnAllPaths

namespace Duber.Trip.API.Infrastructure.Repository
{
    public class EventStoreRepository : IEventStoreRepository
    {
        private readonly MongoDbContext _dbContext;

        public EventStoreRepository(IOptions<MongoOptions> settings)
        {
            _dbContext = new MongoDbContext(settings);
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