
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
            ClientRepository<T> repo = new ClientRepository<T>(_logger);
            return await repo.Delete(t);
        }

        public async Task<T> Load<T>(string id) where T : class, IStringId
        {
            ClientRepository<T> repo = new ClientRepository<T>(_logger);
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
            ClientRepository<T> repo = new ClientRepository<T>(_logger);
            return await repo.Save(t);
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

            ClientRepository<T> repo = new ClientRepository<T>(_logger);
            return await repo.StringSave(id, data);
        }

        public async Task<List<T>> LoadAll<T>(List<string> ids) where T : class, IStringId
        {
            ClientRepository<T> repo = new ClientRepository<T>(_logger);
            return await repo.LoadAll(ids);
        }

        public async Task<List<T>> Search<T>(Expression<Func<T, bool>> func, int quantity = 1000, int skip = 0) where T : class, IStringId
        {
            ClientRepository<T> repo = new ClientRepository<T>(_logger);
            return await repo.Search(func);
        }

        public Task CreateIndex<T>(List<IndexConfig> configs) where T : class, IStringId
        {
            throw new NotImplementedException();
        }
    }
}
