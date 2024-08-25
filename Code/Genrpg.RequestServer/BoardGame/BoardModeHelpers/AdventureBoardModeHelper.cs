using Genrpg.RequestServer.BoardGame.Entities;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.BoardGame.BoardModeHelpers
{
    public class AdventureBoardModeHelper : BaseBoardModeHelper
    {

        public override long GetKey() { return BoardModes.Adventure; }
        public override EBonusModeEndTypes BonusModeEndType => EBonusModeEndTypes.HomeTile;
        public override long TriggerTileTypeId => TileTypes.Adventure;
        protected override EPlayRollTypes PlayMultTypes => EPlayRollTypes.Average;
        public override bool UseOwnerBoardWhenSwitching() { return false; }

        public override async Task EnterMode(WebContext context, RollDiceArgs args)
        {
            await Task.CompletedTask;
        }

        public override async Task ExitMode(WebContext context, RollDiceArgs args)
        {
            await Task.CompletedTask;
        }
    }
}
