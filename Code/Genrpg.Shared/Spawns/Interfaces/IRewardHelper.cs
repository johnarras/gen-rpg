using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spawns.Interfaces
{
    public interface IRewardHelper : ISetupDictionaryItem<long>
    {
        // This will handle any extra results we need to send to the client.
        bool GiveReward(GameState gs, Character ch, long entityId, long quantity, object extraData = null);
    }
}
