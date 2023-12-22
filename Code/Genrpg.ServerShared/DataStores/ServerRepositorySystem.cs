using Genrpg.Shared.DataStores.Indexes;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Utils;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Genrpg.ServerShared.DataStores.NoSQL;
using Genrpg.ServerShared.DataStores.Blobs;
using System.Threading;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Genrpg.Shared.Logs.Interfaces;

namespace Genrpg.ServerShared.DataStores
{
    // The original goal of this was to have an outer repository factory that could have different inner ones
    // for different platforms, which is why there's so much indirection here. Currently, it's just the
    // AzureRepositoryfactory, although the first implementation of all of this used AWS.

    public class ServerRepositorySystem : IRepositorySystem
    {

        const int QueueCount = 4;

        const string BlobPrefix = "Blob";
        const string NoSQLPrefix = "NoSQL";
        const string ReplacementString = "XXXX";

        protected ServerConfig _config = null;
        private List<DbQueue> _queues = null;
        private ILogSystem _logger = null;

        private Dictionary<string, string> _environments = new Dictionary<string, string>();
        private Dictionary<string, string> _connectionStrings = new Dictionary<string, string>();

        private static ConcurrentDictionary<string, IRepository> _repos = new ConcurrentDictionary<string, IRepository>();
        private static ConcurrentDictionary<string, BlobRepository> _blobRepos = new ConcurrentDictionary<string, BlobRepository>();
        private static ConcurrentDictionary<string, NoSQLRepository> _noSQLRepos = new ConcurrentDictionary<string, NoSQLRepository>();
        private ConcurrentDictionary<Type, IRepository> _repoTypeDict = new ConcurrentDictionary<Type, IRepository>();

        public ServerRepositorySystem(ILogSystem logger, Dictionary<string,string> dataEnvironments, Dictionary<string, string> connectionStrings,
            CancellationToken token)
        {
            _logger = logger;
            _environments = dataEnvironments;
            _connectionStrings = connectionStrings;
            _queues = new List<DbQueue>();
            for (int i = 0; i < QueueCount; i++)
            {
                _queues.Add(new DbQueue(logger, token));
            }

            foreach (string key in _connectionStrings.Keys)
            {
                if (key.IndexOf(BlobPrefix) == 0)
                {
                    string dataCategory = key.Replace(BlobPrefix, "");
                    string blobEnv = _environments[dataCategory];
                    AddBlobRepo(dataCategory, blobEnv);
                }

                else if (key.IndexOf(NoSQLPrefix) == 0)
                {
                    string dataCategory = key.Replace(NoSQLPrefix, "");
                    string dbEnv = _environments[dataCategory];
                    AddNoSqlRepo(dataCategory, dbEnv);
                }
            }
        }

        public BlobRepository AddBlobRepo(string dataCategory, string blobEnv)
        {
            string typeKey = (_environments[dataCategory] + dataCategory).ToLower();
            if (!_blobRepos.TryGetValue(typeKey, out BlobRepository currentBlobRepo))
            {
                string blobConnection = _connectionStrings[BlobPrefix + dataCategory];
                BlobRepository blobRepo = new BlobRepository(_logger, blobConnection);
                _blobRepos[typeKey] = blobRepo;
                _repos[typeKey] = blobRepo;
                return blobRepo;
            }
            return currentBlobRepo;
        }

        public NoSQLRepository AddNoSqlRepo(string dataCategory, string dbEnv)
        {
            string typeKey = (_environments[dataCategory] + dataCategory).ToLower();
            if (!_noSQLRepos.TryGetValue(typeKey, out NoSQLRepository currentNoSqlRepo))
            {
                NoSQLRepository noSQLRepo = new NoSQLRepository(_logger, typeKey, _connectionStrings[NoSQLPrefix+dataCategory]);
                _noSQLRepos[typeKey] = noSQLRepo;
                _repos[typeKey] = noSQLRepo;
                return noSQLRepo;
            }
            return currentNoSqlRepo;
        }

        /// <summary>
        /// Find a repository based on the type passed in.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public IRepository FindRepo(Type t)
        {
            if (_repoTypeDict.TryGetValue(t, out IRepository repo))
            {
                return repo;
            }

            string dataCategoryName = DataCategoryTypes.Default;

            DataCategory category = Attribute.GetCustomAttribute(t, typeof(DataCategory), true) as DataCategory;

            if (category != null)
            {
                dataCategoryName = category.Category;
            }
            else
            {
                throw new Exception("Missing DataCategory");
            }

            string dbEnv = _environments[dataCategoryName];

            dataCategoryName = (dbEnv + dataCategoryName).ToLower();

            if (_repos.TryGetValue(dataCategoryName, out IRepository existingRepo))
            {               
                _repoTypeDict[t] = existingRepo;
                return existingRepo;
            }

            return null;
        }

        public async Task<bool> Delete<T>(T obj) where T : class, IStringId
         {
            IRepository repo = FindRepo(obj.GetType());
            return await repo.Delete(obj);
        }

        public async Task<bool> Save<T>(T obj) where T : class, IStringId
        {
            IRepository repo = FindRepo(obj.GetType());
            return await repo.Save(obj);
        }

        public async Task<T> Load<T>(string id) where T : class, IStringId
        {
            IRepository repo = FindRepo(typeof(T));
            return await repo.Load<T>(id);
        }

        public async Task<bool> SaveAll<T>(List<T> t) where T : class, IStringId
        {
            IRepository repo = FindRepo(typeof(T));
            return await repo.SaveAll(t);
        }

        public async Task<bool> TransactionSave<T>(List<T> list) where T : class, IStringId
        {
            IRepository repo = FindRepo(typeof(T));
            return await repo.TransactionSave(list);
        }

        public void QueueSave<T>(T t) where T : class, IStringId
        {
            SaveAction<T> saveAction = new SaveAction<T>(t, this);
            _queues[StrUtils.GetIdHash(t.Id) % QueueCount].Enqueue(saveAction);
        }

        public void QueueTransactionSave<T>(List<T> list, string queueId) where T : class, IStringId
        {
            if (list.Count < 1)
            {
                return;
            }

            SaveAction<T> saveAction = new SaveAction<T>(list, this);

            _queues[StrUtils.GetIdHash(queueId) % QueueCount].Enqueue(saveAction);
        }

        public void QueueDelete<T>(T t) where T : class, IStringId
        {
            DeleteAction<T> deleteAction = new DeleteAction<T>(t, this);
            _queues[StrUtils.GetIdHash(t.Id) % QueueCount].Enqueue(deleteAction);
        }

        public async Task<List<T>> Search<T>(Expression<Func<T, bool>> func, int quantity = 1000, int skip = 0) where T : class, IStringId
        {
            IRepository repo = FindRepo(typeof(T));
            return await repo.Search<T>(func, quantity, skip);
        }

        public async Task CreateIndex<T>(List<IndexConfig> configs) where T : class, IStringId
        {
            IRepository repo = FindRepo(typeof(T));
            await repo.CreateIndex<T>(configs);
            return;
        }
    }
}
