using Genrpg.RequestServer.Core;
using Genrpg.Shared.AI.Settings;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.PlayerData.LoadUpdateHelpers
{
    public class CoreCharacterLoadUpdater : BaseCharacterLoadUpdater
    {
        private IGameData _gameData = null;
        public override int Priority => 1;

        public override async Task Update(WebContext context, Character ch)
        {
            ch.FactionTypeId = FactionTypes.Player;
            ch.BaseSpeed = _gameData.Get<AISettings>(ch).BaseUnitSpeed;
            ch.Speed = ch.BaseSpeed;
            ch.RemoveFlag(UnitFlags.Evading);
            ch.EntityTypeId = EntityTypes.Unit;
            ch.EntityId = 1;
            await Task.CompletedTask;
        }
    }
}
