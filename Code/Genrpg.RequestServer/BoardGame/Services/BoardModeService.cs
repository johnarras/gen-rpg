using Genrpg.RequestServer.BoardGame.BoardGen;
using Genrpg.RequestServer.BoardGame.BoardModeHelpers;
using Genrpg.RequestServer.BoardGame.Entities;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Services.NoUsers;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.Entities;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Utils.Data;
using System.Net.Mime;

namespace Genrpg.RequestServer.BoardGame.Services
{
    public class BoardModeService : IBoardModeService
    {

        private IBoardGenService _boardGenService = null!;
        private IRepositoryService _repoService = null!;
        private IBoardPrizeService _prizeService = null!;
        private SetupDictionaryContainer<long, IBoardModeHelper> _boardModeHelpers = new SetupDictionaryContainer<long, IBoardModeHelper>();
        
        public IBoardModeHelper GetBoardModeHelper(long boardModeId)
        {
            if (_boardModeHelpers.TryGetValue(boardModeId, out IBoardModeHelper helper))
            {
                return helper;
            }
            return null;
        }

        public async Task SwitchToBoardMode(WebContext context, SwitchBoardModeArgs args)
        {

            if (string.IsNullOrEmpty(args.OwnerId))
            {
                args.OwnerId = context.user.Id;
            }

            IBoardModeHelper helper = GetBoardModeHelper(args.BoardModeId);

            BoardData currentBoardData = await context.GetAsync<BoardData>();
            BoardStackData stackData = await context.GetAsync<BoardStackData>();

            if (currentBoardData.BoardModeId != BoardModes.Primary)
            {
                return;
            }

            BoardData newBoardData = null;

            if (helper.UseOwnerBoardWhenSwitching())
            {
                BoardData otherBoardData = currentBoardData;
                if (!string.IsNullOrEmpty(args.OwnerId) && args.OwnerId != context.user.Id)
                {
                    otherBoardData = await _repoService.Load<BoardData>(args.OwnerId);

                    if (otherBoardData == null)
                    {
                        otherBoardData = currentBoardData;
                    }
                }

                newBoardData = new BoardData()
                {
                    Id = context.user.Id,
                    OwnerId = args.OwnerId,
                    BoardModeId = args.BoardModeId,
                    ZoneTypeId = otherBoardData.ZoneTypeId,
                    Tiles = otherBoardData.Tiles,
                };

                newBoardData.Events = new SmallIndexBitList();
                newBoardData.Bonuses = new SmallIndexBitList();
                
            }
            else // Adventure only.
            {
                BoardGenArgs genArgs = new BoardGenArgs()
                {
                    BoardModeId = args.BoardModeId,
                    ForceZoneTypeId = args.ZoneTypeId,
                    EntityId = args.Quantity,
                    OwnerId = context.user.Id,
                    Seed = context.rand.Next(),
                };

                newBoardData = await _boardGenService.GenerateBoard(context, genArgs);
            }

            stackData.Boards.Add(currentBoardData);

            NextBoardData nextData = new NextBoardData() { NextBoard = newBoardData };

            await _prizeService.UpdatePrizesForBoard(context, newBoardData);

            context.Set<NextBoardData>(nextData);   

            await Task.CompletedTask;
        }
    }
}
