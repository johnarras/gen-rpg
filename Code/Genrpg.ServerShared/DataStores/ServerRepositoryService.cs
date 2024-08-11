using Genrpg.Shared.DataStores.Indexes;
using Genrpg.ServerShared.Config;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Genrpg.ServerShared.DataStores.NoSQL;
using Genrpg.ServerShared.DataStores.Blobs;
using System.Threading;
using Genrpg.ServerShared.DataStores.DbQueues;
using Genrpg.ServerShared.DataStores.DbQueues.Actions;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Setup.Constants;
using Genrpg.Shared.DataStores.DataGroups;

namespace Genrpg.ServerShared.DataStores
{
    public interface IServerRepositoryService : IRepositoryService
    {
        Task<T> AtomicIncrement<T>(string docId, string fieldName, long increment) where T : class, IStringId;
    }

    // The original goal of this was to have an outer repository factory that could have different inner ones
    // for different platforms, which is why there's so much indirection here. Currently, it's just the
    // AzureRepositoryfactory, although the first implementation of all of this used AWS.

    public class ServerRepositoryService : IServerRepositoryService
    {

        public async Task Initialize(CancellationToken toke)
        {
            await Task.CompletedTask;
        }

        const int QueueCount = 4;

        private List<DbQueue> _queues = null;
        private ILogService _logger = null;
        private IServerConfig _config = null;

        private Dictionary<string, string> _environments = new Dictionary<string, string>();
        private Dictionary<string, string> _connectionStrings = new Dictionary<string, string>();

        private static ConcurrentDictionary<string, IRepository> _repos = new ConcurrentDictionary<string, IRepository>();
        private static ConcurrentDictionary<string, BlobRepository> _blobRepos = new ConcurrentDictionary<string, BlobRepository>();
        private static ConcurrentDictionary<string, NoSQLRepository> _noSQLRepos = new ConcurrentDictionary<string, NoSQLRepository>();
        private ConcurrentDictionary<Type, IRepository> _repoTypeDict = new ConcurrentDictionary<Type, IRepository>();

        public int SetupPriorityAscending() { return SetupPriorities.Repositories; }

        public async Task PrioritySetup(CancellationToken token)
        {

            _environments = _config.DataEnvs;
            _connectionStrings = _config.GetConnectionStrings();
            _queues = new List<DbQueue>();
            for (int i = 0; i < QueueCount; i++)
            {
                _queues.Add(new DbQueue(_logger, token));
            }

            string blobRepoTypeName = ERepoTypes.Blob.ToString();
            string noSqlRepoTypeName = ERepoTypes.NoSQL.ToString();   

            foreach (string key in _connectionStrings.Keys)
            {
                if (key.IndexOf(blobRepoTypeName) == 0)
                {
                    string dataCategory = key.Replace(blobRepoTypeName, "");
                    string env = _environments[dataCategory];
                    await AddBlobRepo(_connectionStrings[key], env, dataCategory, blobRepoTypeName);
                }

                else if (key.IndexOf(noSqlRepoTypeName) == 0)
                {
                    string dataCategory = key.Replace(noSqlRepoTypeName, "");
                    string env = _environments[dataCategory];
                    AddNoSqlRepo(_connectionStrings[key], env, dataCategory, noSqlRepoTypeName);
                }
            }
            await Task.CompletedTask;
        }

        public async Task AddBlobRepo(string connectionString, string env, string dataCategory, string dataStoreType)
        {
            await Task.CompletedTask;
            string typeKey = GetEnvCategoryStoreTypeKey(env, dataCategory, dataStoreType);
            BlobRepository blobRepo = new BlobRepository(_logger, _config, connectionString, dataCategory, env);
            _repos[typeKey] = blobRepo;
        }

        public void AddNoSqlRepo(string connectionString, string env, string dataCategory, string dataStoreType)
        {
            string typeKey = GetEnvCategoryStoreTypeKey(env, dataCategory, dataStoreType);

            _repos[typeKey] = new NoSQLRepository(_logger, env, dataCategory, connectionString);
        }

        private string GetEnvCategoryStoreTypeKey(string env, string dataCategory, string dataStoreType)
        {
            return (env + dataCategory + dataStoreType).ToLower();
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

            DataGroup dataGroup = Attribute.GetCustomAttribute(t, typeof(DataGroup), true) as DataGroup;

            if (dataGroup == null)
            {
                throw new Exception("Missing DataCategory on type " + t.Name);
            }

            string dataCategoryName = dataGroup.Category.ToString();

            string dbEnv = _environments[dataCategoryName];

            string repoTypeName = dataGroup.RepoType.ToString();

            string typeKey = GetEnvCategoryStoreTypeKey(dbEnv, dataCategoryName, repoTypeName);

            if (_repos.TryGetValue(typeKey, out IRepository existingRepo))
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

        public async Task<bool> DeleteAll<T>(Expression<Func<T, bool>> func) where T : class, IStringId
        {
            IRepository repo = FindRepo(typeof(T));
            return await repo.DeleteAll(func);
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
            if (typeof(T).IsInterface)
            {
                Console.Write("cannot queue interface type");
            }
            _queues[StrUtils.GetIdHash(t.Id) % QueueCount].Enqueue(saveAction);
        }

        public void QueueTransactionSave<T>(List<T> list, string queueId) where T : class, IStringId
        {
            if (list.Count < 1)
            {
                return;
            }

            SaveAction<T> saveAction = new SaveAction<T>(list, this);
            if (typeof(T).IsInterface)
            {
                Console.Write("cannot queue interface type 2");
            }
            _queues[StrUtils.GetIdHash(queueId) % QueueCount].Enqueue(saveAction);
        }

        public void QueueDelete<T>(T t) where T : class, IStringId
        {
            if (typeof(T).IsInterface)
            {
                Console.Write("cannot queue interface type 3");
            }
            DeleteAction<T> deleteAction = new DeleteAction<T>(t, this);
            _queues[StrUtils.GetIdHash(t.Id) % QueueCount].Enqueue(deleteAction);
        }

        public async Task<List<T>> Search<T>(Expression<Func<T, bool>> func, int quantity = 1000, int skip = 0) where T : class, IStringId
        {
            IRepository repo = FindRepo(typeof(T));
            return await repo.Search<T>(func, quantity, skip);
        }

        public async Task CreateIndex<T>(CreateIndexData data) where T : class, IStringId
        {
            IRepository repo = FindRepo(typeof(T));
            await repo.CreateIndex<T>(data);
            return;
        }

        public async Task<bool> UpdateDict<T>(string docId, Dictionary<string, object> fieldNameUpdates) where T : class, IStringId
        {
            IRepository repo = FindRepo(typeof(T));

            return await repo.UpdateDict<T>(docId, fieldNameUpdates);
        }

        public void QueueUpdateDict<T>(string docId, Dictionary<string, object> fieldNameUpdates) where T : class, IStringId
        {
            UpdateAction<T> updateAction = new UpdateAction<T>(docId, fieldNameUpdates, this);
            _queues[StrUtils.GetIdHash(docId) % QueueCount].Enqueue(updateAction);
        }


        public async Task<bool> UpdateAction<T>(string docId, Action<T> action) where T : class, IStringId
        {
            IRepository repo = FindRepo(typeof(T));

            return await repo.UpdateAction<T>(docId, action);
        }

        public void QueueUpdateAction<T>(string docId, Action<T> action) where T : class, IStringId
        {
            UpdateAction<T> updateAction = new UpdateAction<T>(docId, action, this);
            _queues[StrUtils.GetIdHash(docId) % QueueCount].Enqueue(updateAction);
        }

        public async Task<T> AtomicIncrement<T>(string docId, string fieldName, long increment) where T : class, IStringId
        {
            IServerRepository repo = FindRepo(typeof(T)) as IServerRepository;

            return await repo.AtomicIncrement<T>(docId, fieldName, increment);

        }
    }
}
