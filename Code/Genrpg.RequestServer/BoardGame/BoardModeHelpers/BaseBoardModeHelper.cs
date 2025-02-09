using Genrpg.RequestServer.BoardGame.Entities;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Users.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.BoardGame.BoardModeHelpers
{
    public abstract class BaseBoardModeHelper : IBoardModeHelper
    {

        protected IGameData _gameData;

        public abstract long GetKey();
        public abstract EBonusModeEndTypes BonusModeEndType { get;}
        public abstract long TriggerTileTypeId { get; }
        protected abstract EPlayRollTypes PlayMultTypes { get; }
        public abstract Task EnterMode(WebContext context, RollDiceArgs rules);
        public abstract Task ExitMode(WebContext context, RollDiceArgs rules);
        public virtual bool UseOwnerBoardWhenSwitching() { return true; }

        public virtual async Task<double> GetPlayMult(WebContext webContext)
        {
            CoreUserData userData = await webContext.GetAsync<CoreUserData>();

            double playMult = BoardGameConstants.MinPlayMult;

            if (PlayMultTypes == EPlayRollTypes.Average)
            {
                playMult = userData.AvgMult;
            }
            else if (PlayMultTypes == EPlayRollTypes.Current)
            {
                playMult = userData.PlayMult;
            }
            else if (PlayMultTypes == EPlayRollTypes.One)
            {
                playMult = BoardGameConstants.MinPlayMult;
            }

            // Now give a scaling bonus for 
            if (playMult > BoardGameConstants.MinPlayMult)
            {
                double playMultRemainder = playMult - 1; // 100 at 100x
                double sqrtRemainder = Math.Sqrt(playMultRemainder); // 10 at 100x
                // Divide by some decent-sized number (20 for now)
                double remainderByDiv = sqrtRemainder / _gameData.Get<BoardGameSettings>(webContext.user).PlayMultBonusDivisor; // 0.5 at 100x
                // ADd one
                double finalMult = 1 + remainderByDiv; // So 1.5 at 100x
                playMult *= finalMult; // Mult payouts by 1.5 during rolls at 100x multiplier/
            }
            return playMult;
        }

        public virtual async Task OnPassTile(WebContext webContext, RollDiceArgs args, int tileIndex)
        {
            await Task.CompletedTask;
        }

        public virtual async Task OnLandTile(WebContext webContext, RollDiceArgs args, int tileIndex)
        {
            await Task.CompletedTask;
        }

        public virtual int GetNextTileIndex(WebContext context, BoardData boardData, int tileIndex)
        {
            if (tileIndex < 0 || tileIndex >= boardData.Length)
            {
                tileIndex = 0;
            }

            long currentTileTypeId = boardData.Tiles.Get(tileIndex);
            int nextTileIndex = tileIndex + 1;
       
            if (nextTileIndex >= boardData.Length)
            {
                nextTileIndex = BoardGameConstants.FirstTileIndex;
            }

            return nextTileIndex;
        }

        public async Task SetupRollDiceArgs(WebContext context, RollDiceArgs args)
        {
            args.Response.DiceCount = args.Mode.DiceCount;
            args.Response.DiceSides = 6;
            args.Response.FreeRolls = args.Mode.FreeRolls;
            
            CoreUserData userData = await context.GetAsync<CoreUserData>();

            args.PlayMult = await GetPlayMult(context);

            await Task.CompletedTask;
        }
    }
}
