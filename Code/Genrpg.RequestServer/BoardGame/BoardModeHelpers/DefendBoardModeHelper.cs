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
    public class DefendBoardModeHelper : BaseBoardModeHelper
    {
        public override long GetKey() { return BoardModes.Defend; }
        public override EBonusModeEndTypes BonusModeEndType => EBonusModeEndTypes.StartTile;
        public override long TriggerTileTypeId => TileTypes.Defend;
        protected override EPlayRollTypes PlayMultTypes => EPlayRollTypes.Average;

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
