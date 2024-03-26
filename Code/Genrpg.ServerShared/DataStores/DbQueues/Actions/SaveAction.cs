using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.DbQueues.Actions
{

    public class SaveAction<T> : IDbAction where T : class, IStringId
    {

        private List<T> _items { get; set; } = new List<T>();
        private IRepositoryService _repoSystem { get; set; }

        public SaveAction(T item, IRepositoryService repoSystem)
        {
            _repoSystem = repoSystem;
            _items.Add(item);
        }

        public SaveAction(List<T> items, IRepositoryService repoSystem)
        {
            _repoSystem = repoSystem;
            _items = new List<T>(items);
        }

        public async Task<bool> Execute()
        {
            return await _repoSystem.TransactionSave(_items).ConfigureAwait(false);
        }
    }
}
