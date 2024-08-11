using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spawns.Interfaces
{
    public interface IRewardHelper : ISetupDictionaryItem<long>
    {
        // This will handle any extra results we need to send to the client.
        bool GiveReward(IRandom rand, Unit unit, long entityId, long quantity, object extraData = null);
    }

    public interface IQuantityRewardHelper : IRewardHelper
    {
        bool Add(Unit unit, long entityId, long quantity);
        bool Set(Unit unit, long entityId, long quantity);
        long Get(Unit unit, long entityId);
    }
}
