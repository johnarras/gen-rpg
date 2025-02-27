
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Logging.Interfaces;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Authentication;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace Genrpg.ServerShared.DataStores.NoSQL
{
    public class NoSQLRepository : IServerRepository
    {

        private static ConcurrentDictionary<string, MongoClient> _clientCache = new ConcurrentDictionary<string, MongoClient>();

        private MongoClient _client = null;
        private IMongoDatabase _database = null;
        private ILogService _logger = null;
        private ConcurrentDictionary<Type, INoSQLCollection> _collections = new ConcurrentDictionary<Type, INoSQLCollection>();

        static object _connectionLock = new object();

        #region Core
        public NoSQLRepository(ILogService logger, string env, string dataCategory, string connectionString)
        { 
            string databaseName = (env+dataCategory).ToLower();
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
                    lock (_connectionLock)
                    {
                        if (_clientCache.TryGetValue(connectionString, out MongoClient client2))
                        {
                            _client = client2;
                        }
                        else
                        {
                            string newConnectionString = connectionString.Replace(";", "&");
                            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(newConnectionString));
                            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
                            _clientCache.TryAdd(connectionString, new MongoClient(settings));

                            if (_clientCache.TryGetValue(connectionString, out MongoClient client3))
                            {
                                _client = client3;
                            }
                            else
                            {
                                throw new Exception("Failed to create MongClient");
                            }
                        }
                    }
                }

                _database = _client.GetDatabase(databaseName);

            }
            catch (Exception ex)
            {
                _logger.Exception(ex, "NoSQLRepo.Init");
            }
        }

        public async Task CopyBetweenCollections<T>(string startSuffix, string endSuffix) where T : IStringId, new()
        {
            string startName = (typeof(T).Name + startSuffix).ToLower();
            string endName = (typeof(T).Name + endSuffix).ToLower();

            IMongoCollection<T> startColl = _database.GetCollection<T>(startName);
            IMongoCollection<T> endColl = _database.GetCollection<T>(endName);

            List<T> items = await startColl.Find(x => true).ToListAsync();

            List<WriteModel<T>> models = new List<WriteModel<T>>();

            foreach (T item in items)
            {
                ReplaceOneModel<T> replaceModel = new ReplaceOneModel<T>(new FilterDefinitionBuilder<T>().Where(x => x.Id == item.Id), item);
                replaceModel.IsUpsert = true;
                models.Add(replaceModel);
            }

            BulkWriteOptions options = new BulkWriteOptions()
            {
                BypassDocumentValidation = true,
                IsOrdered = false,
            };
            await endColl.BulkWriteAsync(models, options);
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
        /// 
        /// This is public, but not in an interface to expose a few helper functions.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public INoSQLCollection GetCollection(Type t) 
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

        public async Task<bool> Save<T>(T obj, bool verbose = false) where T : class, IStringId
        {

            INoSQLCollection collection = GetCollection(obj.GetType());
            return await collection.Save(obj, verbose);
        }

        public async Task<bool> Delete<T>(T obj) where T : class, IStringId
        {
            INoSQLCollection collection = GetCollection(obj.GetType());
            return await collection.Delete(obj);
            
        }

        public async Task<bool> DeleteAll<T>(Expression<Func<T,bool>> func) where T : class, IStringId
        {
            INoSQLCollection collection = GetCollection(typeof(T));
            return await collection.DeleteAll(func);
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
        public async Task CreateIndex<T>(CreateIndexData data) where T : class, IStringId
        {
            INoSQLCollection collection = GetCollection(typeof(T));
            await collection.CreateIndex(data);
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
                //using (IClientSessionHandle session = await _client.StartSessionAsync())
                //{
                //    try
                //    {
                //        session.StartTransaction();


                //        List<Task<bool>> saves = new List<Task<bool>>();

                //        foreach (T item in list)
                //        {
                //            INoSQLCollection collection = GetCollection(item.GetType());
                //            await collection.TransactionSave(item, session);
                //        }

                //        await session.CommitTransactionAsync();
                //    }
                //    catch (Exception e)
                //    {
                //        _logger.Exception(e, "NoSQLRepository.TransactionSave");
                //        await session.AbortTransactionAsync();
                //        throw new Exception("Failed Transaction", e);
                //    }
                //    return true;
                //}
            }
        }

        public virtual async Task<bool> UpdateDict<T>(string docId, Dictionary<string, object> fieldNameUpdates) where T : class, IStringId
        {
            INoSQLCollection collection = GetCollection(typeof(T));

            return await collection.UpdateDict(docId, fieldNameUpdates);
        }

        public virtual async Task<bool> UpdateAction<T>(string docId, Action<T> action) where T : class, IStringId
        {
            INoSQLCollection collection = GetCollection(typeof(T));

            return await collection.UpdateAction(docId, action);
        }

        public async Task<T> AtomicIncrement<T>(string docId, string fieldName, long increment) where T : class, IStringId
        {
            INoSQLCollection collection = GetCollection(typeof(T));

            return (T)await collection.AtomicIncrement(docId, fieldName, increment);
        }
    }
}
