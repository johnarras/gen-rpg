using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Currencies.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.UserCoins.Entities;
using System.Threading.Tasks;
namespace Genrpg.Shared.UserCoins.Helpers
{
    public class UserCoinHelper : IEntityHelper
    {

        public long GetKey() { return EntityType.UserCoin; }
        public string GetDataPropertyName() { return "UserCoinTypes"; }

        public IIndexedGameItem Find(GameState gs, long id)
        {
            return gs.data.GetGameData<UserCoinSettings>().GetUserCoinType(id);
        }
    }
}
