using Genrpg.RequestServer.BoardGame.Entities;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Constants;

namespace Genrpg.RequestServer.BoardGame.BoardModeHelpers
{
    public class SidePathBoardModeHelper : BaseBoardModeHelper
    {
        public override long GetKey() { return BoardModes.SidePath; }
        public override EBonusModeEndTypes BonusModeEndType => EBonusModeEndTypes.RollCount;
        public override long TriggerTileTypeId => TileTypes.SidePath;
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
