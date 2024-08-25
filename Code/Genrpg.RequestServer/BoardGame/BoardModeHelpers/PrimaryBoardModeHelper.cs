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
    public class PrimaryBoardModeHelper : BaseBoardModeHelper
    {
        public override long GetKey() { return BoardModes.Primary; }
        public override EBonusModeEndTypes BonusModeEndType => EBonusModeEndTypes.None;
        public override long TriggerTileTypeId => TileTypes.None;
        protected override EPlayRollTypes PlayMultTypes => EPlayRollTypes.Current;

        public override async Task EnterMode(WebContext context, RollDiceArgs rules)
        {
            await Task.CompletedTask;
        }

        public override async Task ExitMode(WebContext context, RollDiceArgs rules)
        {
            await Task.CompletedTask;
        }
    }
}
