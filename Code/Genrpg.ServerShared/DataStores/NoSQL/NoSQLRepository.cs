
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.GameSettings.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logs.Entities;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.NoSQL
{
    public class NoSQLRepository : IRepository
    {

        private static ConcurrentDictionary<string, MongoClient> _clientCache = new ConcurrentDictionary<string, MongoClient>();

        private MongoClient _client = null;
        private IMongoDatabase _database = null;
        private ILogSystem _logger = null;
        private ConcurrentDictionary<Type, INoSQLCollection> _collections = new ConcurrentDictionary<Type, INoSQLCollection>();

        #region Core
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

        /// <summary>
        /// This is a bit ugly, but the fact that Mongo requires a generic type
        /// to perform operations means it's either something like this,
        /// or the generic type of everything has to properly be passed
        /// around the codebase, but this leads to a lot of tedious 
        /// helper classes and methods just to be able to have
        /// a list of some base class/interface and still be
        /// able to do database operations through the dynamic type
        /// of the object.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private INoSQLCollection GetCollection(Type t) 
        {
            if (_collections.TryGetValue(t, out INoSQLCollection coll))
            {
                return coll;
            }


            // This uses reflection here to avoid having generic scaffolding classes
            // grow throughout the program
            Type baseCollectionType = (t.GetInterface(nameof(IUpdateData)) != null ?
                typeof (VersionedNoSQLCollection<>) :                
                typeof(NoSQLCollection<>));
            Type genericType = baseCollectionType.MakeGenericType(t);
            coll = (INoSQLCollection)Activator.CreateInstance(genericType, new object[] { this, _logger });
            _collections[t] = coll;
            return coll;
        }
        #endregion

        public async Task<T> Load<T>(string id) where T : class, IStringId
        {

            INoSQLCollection collection = GetCollection(typeof(T));

            return (T)await collection.Load(id);
        }

        public async Task<bool> Save<T>(T obj) where T : class, IStringId
        {

            INoSQLCollection collection = GetCollection(obj.GetType());
            return await collection.Save(obj);
        }

        public async Task<bool> Delete<T>(T obj) where T : class, IStringId
        {
            INoSQLCollection collection = GetCollection(obj.GetType());
            return await collection.Delete(obj);
        }

        public async Task<List<T>> Search<T>(Expression<Func<T, bool>> func, int quantity, int skip) where T : class, IStringId
        {
            INoSQLCollection collection = GetCollection(typeof(T));
           
            List<object> objects = await collection.Search(func, quantity, skip);

            List<T> retval = new List<T>();

            foreach (object o in objects)
            {
                if (o is T t)
                {
                    retval.Add(t);
                }
            }
            return retval;
        }

        /// <summary>
        /// This requires a generic type so that we can put the index directly onto the correct collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configs"></param>
        /// <returns></returns>
        public async Task CreateIndex<T>(List<IndexConfig> configs) where T : class, IStringId
        {
            INoSQLCollection collection = GetCollection(typeof(T));
            await collection.CreateIndex(configs);
        }

        /// <summary>
        /// This requires a generic type so we can save all of them into one collection at once.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public async Task<bool> SaveAll<T>(List<T> items) where T : class, IStringId
        {
            INoSQLCollection collection = GetCollection(typeof(T));
            return await collection.SaveAll(items);
        }

        public async Task<bool> TransactionSave<T>(List<T> list) where T : class, IStringId
        {

            if (true)
            {
                List<Task<bool>> saves = new List<Task<bool>>();

                foreach (T item in list)
                {
                    INoSQLCollection collection = GetCollection(item.GetType());
                    saves.Add(collection.Save(item));
                }

                bool[] results = await Task.WhenAll(saves).ConfigureAwait(false);

                return !results.Any(x => x == false);
            }
            else // Only use this if we have replica sets, does not work in serverless cosmos apparently
            {
                using (IClientSessionHandle session = await _client.StartSessionAsync())
                {
                    try
                    {
                        session.StartTransaction();


                        List<Task<bool>> saves = new List<Task<bool>>();

                        foreach (T item in list)
                        {
                            INoSQLCollection collection = GetCollection(item.GetType());
                            await collection.TransactionSave(item, session);
                        }

                        await session.CommitTransactionAsync();
                    }
                    catch (Exception e)
                    {
                        _logger.Exception(e, "NoSQLRepository.TransactionSave");
                        await session.AbortTransactionAsync();
                        throw new Exception("Failed Transaction", e);
                    }
                    return true;
                }
            }
        }
    }
}
