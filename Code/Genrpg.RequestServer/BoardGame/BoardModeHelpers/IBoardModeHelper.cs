using Genrpg.RequestServer.BoardGame.Entities;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.BoardGame.BoardModeHelpers
{
    public interface IBoardModeHelper : ISetupDictionaryItem<long>
    {
        Task OnPassTile(WebContext context, RollDiceArgs args, int tileIndex);
        Task OnLandTile(WebContext context, RollDiceArgs args, int tileIndex);
        int GetNextTileIndex(WebContext context, RollDiceArgs args, int tileIndex);
        Task SetupRollDiceArgs(WebContext context, RollDiceArgs args);
        public EBonusModeEndTypes BonusModeEndType { get; }
        public long TriggerTileTypeId { get; }
        Task EnterMode(WebContext context, RollDiceArgs args);
        Task ExitMode(WebContext context, RollDiceArgs args);
        Task<double> GetPlayMult(WebContext context);
        bool UseOwnerBoardWhenSwitching();
    }
}
