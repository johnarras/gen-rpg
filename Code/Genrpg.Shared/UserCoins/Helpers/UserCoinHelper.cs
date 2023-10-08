using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Currencies.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.UserCoins.Entities;
using System.Threading.Tasks;
namespace Genrpg.Shared.UserCoins.Helpers
{
    public class UserCoinHelper : IEntityHelper
    {

        public long GetKey() { return EntityType.UserCoin; }
        public string GetDataPropertyName() { return "UserCoinTypes"; }

        public IIndexedGameItem Find(GameState gs, IFilteredObject obj, long id)
        {
            return gs.data.GetGameData<UserCoinSettings>(obj).GetUserCoinType(id);
        }
    }
}
