using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
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
        bool GiveReward(IRandom rand, MapObject obj, long entityId, long quantity, object extraData = null);
    }

    public interface IQuantityRewardHelper : IRewardHelper
    {
        bool Add(MapObject obj, long entityId, long quantity);
        bool Set(MapObject obj, long entityId, long quantity);
        long Get(MapObject obj, long entityId);
    }
}
