using System.Threading.Tasks;
using Duber.Infrastructure.EventBus.Idempotency;
using Kledex.Store.Cosmos.Mongo.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Duber.Trip.API.Infrastructure.Repository
{
    public class IdempotencyStoreProvider : IIdempotencyStoreProvider
    {
        private readonly IMongoDatabase _db;

        public IdempotencyStoreProvider(IOptions<MongoOptions> settings)
        {
            var mongoClient = new MongoClient(settings.Value.ConnectionString);
            _db = mongoClient.GetDatabase(settings.Value.DatabaseName);
        }

        public async Task SaveAsync(IdempotentMessage message)
        {
            var exists = await ExistsAsync(message.MessageId);
            if (exists) return;

            var collection = _db.GetCollection<IdempotentMessage>("IdempotentMessages");
            await collection.InsertOneAsync(message);
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var collection = _db.GetCollection<IdempotentMessage>("IdempotentMessages");
            var filter = Builders<IdempotentMessage>.Filter.Eq("MessageId", id);
            var message = await collection.FindAsync(filter).Result.FirstOrDefaultAsync();
            
            return message != null;
        }
    }
}
