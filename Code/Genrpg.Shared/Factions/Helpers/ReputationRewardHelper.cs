using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.PlayerData;
using Genrpg.Shared.Rewards.Helpers;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Factions.Helpers
{
    public class ReputationRewardHelper : BaseQuantityRewardHelper<ReputationData,ReputationStatus>
    {
        public override long GetKey() { return EntityTypes.Reputation; }
    }
}
