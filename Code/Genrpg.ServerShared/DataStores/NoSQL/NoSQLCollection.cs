using Genrpg.ServerShared.Config;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using MongoDB.Driver;
using System.Reflection.Metadata;
using Genrpg.Shared.Utils;
using System.Reflection;
using Genrpg.Shared.DataStores.Indexes;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Bson.IO;
using System.Reflection.Metadata.Ecma335;
using Genrpg.Shared.Logs.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Amazon.Runtime.Internal.Util;
using AutoMapper.Execution;
using System.Security.Cryptography;

namespace Genrpg.ServerShared.DataStores.NoSQL
{


    public interface INoSQLCollection
    {
        Task<object> Load(string id);
        Task<bool> TransactionSave(object t, IClientSessionHandle session);
        Task<bool> Save(object t);
        Task<bool> Delete(object t);
        Task<bool> DeleteAll(object func);
        Task<bool> UpdateDict(string id, Dictionary<string, object> fieldNameUpdates);
        Task<bool> UpdateAction(string id, object action);
        Task CreateIndex(List<IndexConfig> configs);
        Task<List<object>> Search(object func, int quantity = 1000, int skip = 0);
        Task<bool> SaveAll(object itemList);
    }


    public class NoSQLCollection<T> : INoSQLCollection where T : class, IStringId
    {
        protected IMongoCollection<T> _collection = null;
        protected ILogSystem _logger = null;
        public NoSQLCollection(NoSQLRepository mongoRepository, ILogSystem logger)
        {
            _logger = logger;
            try
            {
                _collection = mongoRepository.GetDatabase().GetCollection<T>(GetCollectionName());
            }
            catch (Exception e)
            {
                _logger.Exception(e, "NoSQL.Init");
            }
        }

        private string GetCollectionName()
        {
            return (typeof(T).Name + "doc").ToLower();
        }

        public async Task<bool> DeleteAll(object listObj)
        {
            Expression<Func<T, bool>> func = (Expression<Func<T, bool>>)listObj;


            if (func == null)
            {
                return false;
            }

            try
            {
                DeleteResult deleteResult = await _collection.DeleteManyAsync<T>(func);
            }
            catch (Exception ex)
            {
                _logger.Exception(ex, "MongoRepo.SaveAll");
                return false;
            }
            return true;
        }


        public async Task<bool> Delete(object obj)
        {
            T t = (T)obj;

            if (t == null)
            {
                return false;
            }

            try
            {
                DeleteResult result = await _collection.DeleteOneAsync(x => x.Id == t.Id);
                if (result.DeletedCount < 1)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Exception(ex, "NoSQL.Delete");
                return false;
            }
            return true;
        }

        public async Task<object> Load(string id)
        {
            try
            {
                var cursor = await _collection.FindAsync(x => x.Id == id);
                return await cursor.FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                _logger.Exception(ex, "Cosmos.Load");
            }
            return null;
        }

        public async Task<bool> Save(object obj)
        {
            return await InnerSave(obj, null);
        }

        public async Task<bool> TransactionSave(object obj, IClientSessionHandle session)
        {
            return await InnerSave(obj, session);
        }

        protected async Task<bool> InnerSave(object obj, IClientSessionHandle session)
        {
            T t = (T)obj;

            if (t == null)
            {
                return false;
            }
            try
            {
                if (string.IsNullOrEmpty(t.Id))
                {
                    throw new Exception("Missing Id on save");
                }

                ReplaceOptions options = new ReplaceOptions() { IsUpsert = true, BypassDocumentValidation = true, };
                ReplaceOneResult replaceResult = await ReplaceDocument(t, options, session);

                if (replaceResult.ModifiedCount < 1 && string.IsNullOrEmpty(replaceResult.UpsertedId?.AsString ?? null))
                {
                    string errorString = "Failed to upsert Document " + typeof(T).Name + " Id: " + t.Id;
                    _logger.Error(errorString);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Exception(ex, "Cosmos.Save");
                return false;
            }
            return true;
        }

        virtual protected async Task<ReplaceOneResult> ReplaceDocument(T t, ReplaceOptions options, IClientSessionHandle session)
        {
            if (session != null)
            {
                return await _collection.ReplaceOneAsync(session, w => w.Id == t.Id, t, options);
            }
            else
            {
                return await _collection.ReplaceOneAsync(w => w.Id == t.Id, t, options);
            }
        }

        public async Task<List<object>> Search(object funcObj, int quantity = 1000, int skip = 0)
        {
            Expression<Func<T, bool>> func = (Expression<Func<T, bool>>)funcObj;

            if (func == null)
            {
                return new List<object>();
            }

            try
            {
                FindOptions<T, T> options = new FindOptions<T, T>();
                if (skip > 0)
                {
                    options.Skip = skip;
                }
                if (quantity > 0)
                {
                    options.Limit = quantity;
                }
                IAsyncCursor<T> cursor = await _collection.FindAsync(func, options);
                List<T> retval = await cursor.ToListAsync();
                return retval.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.Exception(ex, "NoSQL.Search");
            }
            return new List<object>();
        }

        private static ConcurrentDictionary<string, List<IndexConfig>> _configs = new ConcurrentDictionary<string, List<IndexConfig>>();
        public async Task CreateIndex(List<IndexConfig> configs)
        {
            List<IndexConfig> orderedConfigs = configs.OrderBy(x => x.MemberName).ToList();

            string totalIndex = "";
            foreach (IndexConfig config in orderedConfigs)
            {
                totalIndex += config.MemberName;
            }

            if (!_configs.TryAdd(totalIndex, orderedConfigs))
            {
                return;
            }

            CreateIndexOptions<T> options = new CreateIndexOptions<T>();

            Type thisType = typeof(T);
            IndexKeysDefinitionBuilder<T> builder = new IndexKeysDefinitionBuilder<T>();
            List<IndexKeysDefinition<T>> indexes = new List<IndexKeysDefinition<T>>();
            foreach (IndexConfig config in configs)
            {
                MemberInfo mem = thisType.GetMembers().FirstOrDefault(x => x.Name == config.MemberName);
                if (mem == null)
                {
                    continue;
                }
                StringFieldDefinition<T> fieldDef = new StringFieldDefinition<T>(config.MemberName);

                indexes.Add(config.Ascending ? builder.Ascending(fieldDef)
                    : builder.Descending(fieldDef));
            }

            CreateIndexModel<T> indexModel = new CreateIndexModel<T>(builder.Combine(indexes),
                new CreateIndexOptions()
                {
                    Sparse = true,
                    Unique = false,
                });

            await _collection.Indexes.CreateOneAsync(indexModel);
        }

        public async Task<bool> SaveAll(object listObj)
        {
            List<T> items = listObj as List<T>;

            if (items == null)
            {
                return false;
            }
            if (items.Count < 1)
            {
                return true;
            }
            try
            {
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
                await _collection.BulkWriteAsync(models, options);

            }
            catch (Exception ex)
            {
                _logger.Exception(ex, "MongoRepo.SaveAll");
                return false;
            }
            return true;
        }

        protected virtual int GetMaxUpdateAttempts()
        {
            return 1;
        }

        protected virtual Dictionary<string,object> UpdateFieldNameUpdates(Dictionary<string,object> fieldNameUpdates)
        {
            return fieldNameUpdates;
        }

        public virtual async Task<bool> UpdateDict(string docId, Dictionary<string, object> fieldNameUpdates)
        {

            fieldNameUpdates = UpdateFieldNameUpdates(fieldNameUpdates);

            int maxAttempts = GetMaxUpdateAttempts();

            Expression<Func<T, bool>> filter = x => x.Id == docId;

            UpdateDefinitionBuilder<T> builder = Builders<T>.Update;

            List<UpdateDefinition<T>> updates = new List<UpdateDefinition<T>>();

            foreach (string fieldName in fieldNameUpdates.Keys)
            {
                updates.Add(builder.Set(fieldName, fieldNameUpdates[fieldName]));
            }

            UpdateDefinition<T> finalUpdateDef = builder.Combine(updates);

            UpdateOptions options = new UpdateOptions() { BypassDocumentValidation = true, IsUpsert = true };

            for (int i = 0; i < maxAttempts; i++)
            {
                UpdateResult result = await _collection.UpdateOneAsync(filter, finalUpdateDef, options);

                if (result.UpsertedId == docId)
                {
                    return true;
                }

                await Task.Delay(100);
            }

            return false;
        }

        public async Task<bool> UpdateAction(string docId, object actionObj)
        {
            Action<T> action = actionObj as Action<T>;

            if (action == null)
            {
                return false;
            }

            for (int times = 0; times < GetMaxUpdateAttempts(); times++)
            {
                T doc = (T)(await Load(docId));

                if (doc != null)
                {
                    action(doc);

                    if (await Save(doc))
                    {
                        return true;
                    }
                }

                await Task.Delay(250);

            }
            return false;
        }
    }
}
