using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Utils
{

    public class SaveAction<T> : IDbAction where T : class, IStringId
    {

        private string _id { get; set; }
        private T _item { get; set; }
        private IRepositorySystem _repoSystem { get; set; }

        public SaveAction(T item, IRepositorySystem repoSystem)
        {
            _repoSystem = repoSystem;
            _id = item.Id;
            _item = SerializationUtils.FastMakeCopy(item);
        }

        public async Task<bool> Execute()
        {
            return await _repoSystem.Save(_item).ConfigureAwait(false);
        }
    }
}
