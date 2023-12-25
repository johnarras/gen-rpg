
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using System.Threading.Tasks;
using Genrpg.Shared.Logs.Interfaces;

namespace Assets.Scripts.Model
{
    public class ClientRepositorySystem : IRepositorySystem
    {
        private ILogSystem _logger;
        public ClientRepositorySystem(ILogSystem logger)
        {
            _logger = logger;
        }

        public async Task<bool> Delete<T>(T t) where T : class, IStringId
        {
            ClientRepositoryCollection<T> repo = GetRepository<T>();
            return await repo.Delete(t);
        }

        public async Task<object> LoadWithType(Type t, string id)
        {
            IClientRepositoryCollection repo = GetRepositoryFromType(t);
            return await repo.LoadWithType(t, id);
        }

        public async Task<T> Load<T>(string id) where T : class, IStringId
        {
            ClientRepositoryCollection<T> repo = GetRepository<T>();
            return await repo.Load(id);
        }

        public void QueueDelete<T>(T t) where T : class, IStringId
        {
            Delete(t).Wait();
        }

        public void QueueSave<T>(T t) where T : class, IStringId
        {
            Save(t).Wait();
        }

        public async Task<bool> Save<T>(T t) where T : class, IStringId
        {
            try
            {
                IClientRepositoryCollection repo = GetRepositoryFromType(t.GetType());
                return await repo.Save(t);
            }
            catch (Exception e)
            {
                Debug.Log("EXC: " + e.Message + " " + e.StackTrace);
            }
            return false;
        }

        public async Task<bool> SaveAll<T>(List<T> list) where T : class, IStringId
        {
            foreach (T t in list)
            {
                await Save(t);
            }
            return true;
        }

        public async Task<bool> StringSave<T>(string id, string data) where T : class, IStringId
        {
            ClientRepositoryCollection<T> repo = GetRepository<T>();
            return await repo.StringSave(id, data);
        }

        public async Task<List<T>> LoadAll<T>(List<string> ids) where T : class, IStringId
        {
            ClientRepositoryCollection<T> repo = GetRepository<T>();
            return await repo.LoadAll(ids);
        }

        public async Task<List<T>> Search<T>(Expression<Func<T, bool>> func, int quantity = 1000, int skip = 0) where T : class, IStringId
        {
            ClientRepositoryCollection<T> repo = GetRepository<T>();
            return await repo.Search(func);
        }

        public Task CreateIndex<T>(List<IndexConfig> configs) where T : class, IStringId
        {
            throw new NotImplementedException();
        }

        Task<bool> IRepositorySystem.TransactionSave<T>(List<T> list)
        {
            throw new NotImplementedException();
        }

        void IRepositorySystem.QueueTransactionSave<T>(List<T> list, string queueId)
        {
            throw new NotImplementedException();
        }

        private Dictionary<Type, object> _repoCache = new Dictionary<Type, object>();
        public IClientRepositoryCollection GetRepositoryFromType (Type t)
        {
            if (_repoCache.TryGetValue(t, out object repo))
            {
                return (IClientRepositoryCollection) repo;
            }

            Type baseRepoType = typeof(ClientRepositoryCollection<>);
            Type genericType = baseRepoType.MakeGenericType(t);
            object newRepo = Activator.CreateInstance(genericType, new object[] { _logger });

            _repoCache[t] = newRepo;

            return (IClientRepositoryCollection) newRepo;
        }

        public ClientRepositoryCollection<T> GetRepository<T>() where T : class, IStringId
        {
            return (ClientRepositoryCollection<T>)GetRepositoryFromType(typeof(T));
        }

        public async Task<bool> DeleteAll<T>(Expression<Func<T, bool>> func) where T : class, IStringId
        {
            await Task.CompletedTask;
            return false;
        }
    }
}
