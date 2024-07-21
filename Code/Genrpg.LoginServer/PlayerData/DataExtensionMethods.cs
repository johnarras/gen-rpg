using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Loaders;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.PlayerData
{
    public static class DataExtensionMethods
    {

        private static IPlayerDataService _playerDataService = null;
        private static IRepositoryService _repoService = null;
        public static async Task<T> GetAsync<T>(this Character ch, GameState gs) where T : IUnitData
        {
            if (_playerDataService == null)
            {
                _playerDataService = gs.loc.Get<IPlayerDataService>();
            }

            if (_repoService == null)
            {
                _repoService = gs.loc.Get<IRepositoryService>();
            }

            IUnitDataLoader loader = _playerDataService.GetLoader<T>();

            IUnitData newData = await loader.LoadFullData(ch);

            if (newData == null)
            {
                newData = loader.Create(ch);
            }

            T typedData = (T)newData;

            if (typedData != null)
            {
                ch.Set(typedData);
                return typedData;
            }

            return default(T);
        }
    }
}
