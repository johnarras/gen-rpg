using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.UserCoins.Settings;
using Genrpg.Shared.GameSettings;

namespace Genrpg.Shared.UserCoins.Helpers
{
    public class UserCoinHelper : IEntityHelper
    {

        private IGameData _gameData = null;
        public long GetKey() { return EntityTypes.UserCoin; }
        public string GetDataPropertyName() { return "UserCoinTypes"; }

        public IIndexedGameItem Find(IFilteredObject obj, long id)
        {
            return _gameData.Get<UserCoinSettings>(obj).Get(id);
        }
    }
}
