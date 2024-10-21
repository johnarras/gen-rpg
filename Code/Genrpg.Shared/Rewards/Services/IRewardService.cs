using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Loot.Messages;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.Shared.Rewards.Services
{
    public interface IRewardService : IInjectable
    {
        bool GiveRewards<RL>(IRandom rand, MapObject obj, List<RL> resultList) where RL : RewardList;
        bool GiveReward(IRandom rand, MapObject obj, Reward res);
        bool GiveReward(IRandom rand, MapObject obj, long entityType, long entityId, long quantity, object extraData = null);
        bool Add(MapObject obj, long entityTypeId, long entityId, long quantity);
        bool Set(MapObject obj, long entityTypeId, long entityId, long quantity);
        void OnAddQuantity<TUpd>(MapObject obj, TUpd upd, long entityTypeId, long entityId, long diff) where TUpd : class, IStringId;
    }
}
