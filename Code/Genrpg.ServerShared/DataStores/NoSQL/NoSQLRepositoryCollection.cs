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
using Genrpg.Shared.Logs.Entities;

namespace Genrpg.ServerShared.DataStores.NoSQL
{
    public class NoSQLRepositoryCollection<T> : IRepositoryCollection<T> where T : class, IStringId
    {
        private IMongoCollection<T> _collection = null;
        private ILogSystem _logger;
        public NoSQLRepositoryCollection(NoSQLRepository mongoRepository, ILogSystem logger)
        {
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

        public async Task<bool> Delete(T t)
        {
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

        public async Task<T> Load(string id)
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

        public async Task<bool> Save(T t)
        {
            try
            {
                if (string.IsNullOrEmpty(t.Id))
                {
                    throw new Exception("Missing Id on save");
                }

                ReplaceOptions options = new ReplaceOptions() { IsUpsert = true, BypassDocumentValidation = true, };
                await _collection.ReplaceOneAsync(w => w.Id == t.Id, t, options);
            }
            catch (Exception ex)
            {
                _logger.Exception(ex, "Cosmos.Save");
                return false;
            }
            return true;
        }

        public async Task<List<T>> Search(Expression<Func<T, bool>> func, int quantity = 1000, int skip = 0)
        {
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
                return await cursor.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Exception(ex, "NoSQL.Search");
            }
            return new List<T>();
        }

        public async Task<bool> StringSave(string id, string data)
        {
            try
            {
                await _collection.ReplaceOneAsync(w => w.Id == id, SerializationUtils.Deserialize<T>(data), new ReplaceOptions() { IsUpsert = true });
            }
            catch (Exception ex)
            {
                _logger.Exception(ex, "Cosmos.Save");
                return false;
            }
            await Task.CompletedTask;
            return false;
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

        public async Task<bool> SaveAll(List<T> items)
        {
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
    }
}
