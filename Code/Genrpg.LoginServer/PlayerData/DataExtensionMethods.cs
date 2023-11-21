using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.Entities;
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
        public static async Task<T> GetAsync<T>(this Character ch, GameState gs) where T : IUnitData
        {
            if (_playerDataService == null)
            {
                _playerDataService = gs.loc.Get<IPlayerDataService>();
            }

            IUnitDataLoader loader = _playerDataService.GetLoader<T>();

            IUnitData newData = await loader.LoadData(gs.repo, ch);

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
