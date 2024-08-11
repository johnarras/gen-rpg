using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.Shared.Rewards.Services
{
    public interface IRewardService : IInjectable
    {
        bool GiveRewards<SR>(IRandom rand, MapObject obj, List<SR> resultList) where SR : ISpawnResult;
        bool GiveReward(IRandom rand, Unit unit, ISpawnResult res);
        bool GiveReward(IRandom rand, Unit unit, long entityType, long entityId, long quantity, object extraData = null);
        bool Add(Unit unit, long entityTypeId, long entityId, long quantity);
        bool Set(Unit unit, long entityTypeId, long entityId, long quantity);
        void OnSetQuantity<TUpd>(Unit unit, TUpd upd, long entityTypeId, long diff) where TUpd : class, IStringId;
    }
}
