using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.DbQueues.Actions
{
    public class DeleteAction<T> : IDbAction where T : class, IStringId
    {
        private T _obj { get; set; }
        private IRepositorySystem _repoSystem { get; set; }

        public DeleteAction(T item, IRepositorySystem repoSystem)
        {
            _obj = item;
            _repoSystem = repoSystem;
        }

        public async Task<bool> Execute()
        {
            return await _repoSystem.Delete(_obj).ConfigureAwait(false);
        }
    }
}
