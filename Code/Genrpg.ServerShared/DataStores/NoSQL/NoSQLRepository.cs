
using Genrpg.Shared.Logs.Entities;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Security.Authentication;

namespace Genrpg.ServerShared.DataStores.NoSQL
{
    public class NoSQLRepository
    {

        private static ConcurrentDictionary<string, MongoClient> _clientCache = new ConcurrentDictionary<string, MongoClient>();

        private MongoClient _client = null;
        private IMongoDatabase _database = null;
        private ILogSystem _logger = null;

        public NoSQLRepository(ILogSystem logger, string databaseName, string connectionString)
        {
            _logger = logger;
            try
            {
                Setup();

                if (_clientCache.TryGetValue(connectionString, out MongoClient client))
                {
                    _client = client;
                }
                else
                {
                    string newConnectionString = connectionString.Replace(";", "&");
                    MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(newConnectionString));
                    settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
                    _clientCache.TryAdd(connectionString, new MongoClient(settings));

                    if (_clientCache.TryGetValue(connectionString, out MongoClient client2))
                    {
                        _client = client2;
                    }
                    else
                    {
                        throw new Exception("Failed to create MongClient");
                    }
                }

                _database = _client.GetDatabase(databaseName);

            }
            catch (Exception ex)
            {
                _logger.Exception(ex, "NoSQLRepo.Init");
            }
        }

        private void Setup()
        {
            ConventionRegistry.Register("IgnoreMessyData",
                            new ConventionPack
                            {
                                new IgnoreIfDefaultConvention(true),
                                new IgnoreExtraElementsConvention(true),
                            },
                            t => true);
        }

        public MongoClient GetClient()
        {
            return _client;
        }

        public IMongoDatabase GetDatabase()
        {
            return _database;
        }
    }
}
