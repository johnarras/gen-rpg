using Genrpg.Shared.AI.Settings;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.PlayerData.LoadUpdateHelpers
{
    public class CoreCharacterLoadUpdater : BaseCharacterLoadUpdater
    {
        public override int Priority => 1;

        public override async Task Update(GameState gs, Character ch)
        {
            ch.FactionTypeId = FactionTypes.Player;
            ch.BaseSpeed = gs.data.GetGameData<AISettings>(ch).BaseUnitSpeed;
            ch.Speed = ch.BaseSpeed;
            ch.RemoveFlag(UnitFlags.Evading);
            ch.EntityTypeId = EntityTypes.Unit;
            ch.EntityId = 1;
            await Task.CompletedTask;
        }
    }
}
