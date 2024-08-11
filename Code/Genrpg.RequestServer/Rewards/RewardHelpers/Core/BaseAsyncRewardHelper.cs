using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.Interfaces;
using Genrpg.RequestServer.Rewards.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Rewards.RewardHelpers.Core
{
    public abstract class BaseAsyncRewardHelper : IAsyncRewardHelper
    {
        protected IRepositoryService _repoService = null;
        protected IGameData _gameData = null;
        protected IWebRewardService _serverRewardService = null;

        public abstract long GetKey();

        public abstract Task GiveRewardsAsync(WebContext context, long entityId, long quantity, object extraData = null);
    }
}
