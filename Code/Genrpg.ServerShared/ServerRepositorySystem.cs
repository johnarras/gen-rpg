using Genrpg.ServerShared.DataStores;
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
using Genrpg.Shared.Logs.Entities;

namespace Genrpg.ServerShared
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
        private string _env = null;
        private ILogSystem _logger = null;

        private static ConcurrentDictionary<string,BlobRepository> _blobRepos = new ConcurrentDictionary<string,BlobRepository>();
        private static ConcurrentDictionary<string,NoSQLRepository> _noSQLRepos = new ConcurrentDictionary<string,NoSQLRepository>();
        private static ConcurrentDictionary<string, object> _repoDict = new ConcurrentDictionary<string, object>();

        
        public ServerRepositorySystem(ILogSystem logger, string env, Dictionary<string,string> connectionStrings)
        {
            _logger = logger;
            _env = env.ToLower();
            _queues = new List<DbQueue>();
            for (int i = 0; i < QueueCount; i++)
            {
                _queues.Add(new DbQueue(logger));
            }

            foreach (string key in connectionStrings.Keys)
            {
                if (key.IndexOf(BlobPrefix) == 0)
                {
                    string typeKey = (_env + key.Replace(BlobPrefix, "")).ToLower();
                    if (!_blobRepos.ContainsKey(typeKey))
                    {
                        string replacementConnection = connectionStrings[key].Replace(ReplacementString, _env.ToLower());
                        BlobRepository blobRepo = new BlobRepository(_logger, replacementConnection);
                        _blobRepos[typeKey] = blobRepo;
                    }
                }
                if (key.IndexOf(NoSQLPrefix) == 0)
                {
                    string typeKey = (_env + key.Replace(NoSQLPrefix, "")).ToLower();
                    if (!_noSQLRepos.ContainsKey(typeKey))
                    {
                        NoSQLRepository noSQLRepo = new NoSQLRepository(_logger, typeKey, connectionStrings[key]);
                        _noSQLRepos[typeKey] = noSQLRepo;
                    }
                }
            }
        }


        public async Task<IRepositoryCollection<T>> Find<T>() where T : class, IStringId
        {
            Type t = typeof(T);
        
            string nm = (_env + t.Name).ToLower();

            if (_repoDict.TryGetValue(nm, out object repoObj))
            {
                return (IRepositoryCollection<T>) repoObj;
            }

            string dataCategoryName = DataCategory.Default;

            DataCategory category = Attribute.GetCustomAttribute(t, typeof(DataCategory), true) as DataCategory;

            if (category != null)
            {
                dataCategoryName = category.Category;
            }
            dataCategoryName = (_env + dataCategoryName).ToLower();

            if (_noSQLRepos.ContainsKey(dataCategoryName))
            {
                IRepositoryCollection<T> noSQLRepo = new NoSQLRepositoryCollection<T>(_noSQLRepos[dataCategoryName], _logger);
                _repoDict[nm] = noSQLRepo;
                return noSQLRepo;
            }
            if (_blobRepos.ContainsKey(dataCategoryName))
            {
                IRepositoryCollection<T> blobRepo = new BlobRepositoryCollection<T>(_blobRepos[dataCategoryName], _logger);
                _repoDict[nm] = blobRepo;
                return blobRepo;
            }

            await Task.CompletedTask;
            return null;
        }

        public async Task<bool> Delete<T>(T t) where T : class, IStringId
        {
            IRepositoryCollection<T> repo = await Find<T>();
            return await repo.Delete(t);
        }
        public async Task<T> Load<T>(string id) where T : class, IStringId
        {
            IRepositoryCollection<T> repo = await Find<T>();
            return await repo.Load(id);
        }

        public async Task<bool> Save<T>(T t) where T : class, IStringId
        {
            IRepositoryCollection<T> repo = await Find<T>();
            return await repo.Save(t);
        }

        public async Task<bool> SaveAll<T>(List<T> t) where T : class, IStringId
        {
            IRepositoryCollection<T> repo = await Find<T>();
            return await repo.SaveAll(t);
        }

        public void QueueSave<T>(T t) where T : class, IStringId
        {
            SaveAction<T> saveAction = new SaveAction<T>(t, this);
            _queues[StrUtils.GetIdHash(t.Id) % QueueCount].Enqueue(saveAction);
        }

        public void QueueDelete<T>(T t) where T : class, IStringId
        {
            DeleteAction<T> deleteAction = new DeleteAction<T>(t, this);
            _queues[StrUtils.GetIdHash(t.Id) % QueueCount].Enqueue(deleteAction);
        }


        public async Task<bool> StringSave<T>(string id, string data) where T : class, IStringId
        {
            IRepositoryCollection<T> repo = await Find<T>();
            return await repo.StringSave(id, data);
        }

        public async Task<List<T>> Search<T>(Expression<Func<T, bool>> func, int quantity = 1000, int skip = 0) where T : class, IStringId
        {
            IRepositoryCollection<T> repo = await Find<T>();
            return await repo.Search(func, quantity, skip);
        }

        public async Task CreateIndex<T>(List<IndexConfig> configs) where T : class, IStringId
        {
            IRepositoryCollection<T> repo = await Find<T>();
            await repo.CreateIndex(configs);
            return;
        }
    }
}
