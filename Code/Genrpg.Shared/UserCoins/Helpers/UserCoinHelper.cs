using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.UserCoins.Settings;

namespace Genrpg.Shared.UserCoins.Helpers
{
    public class UserCoinHelper : IEntityHelper
    {

        public long GetKey() { return EntityTypes.UserCoin; }
        public string GetDataPropertyName() { return "UserCoinTypes"; }

        public IIndexedGameItem Find(GameState gs, IFilteredObject obj, long id)
        {
            return gs.data.Get<UserCoinSettings>(obj).Get(id);
        }
    }
}
