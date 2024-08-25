using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.Entities;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Services;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Settings;

namespace Genrpg.RequestServer.BoardGame.Services
{
    public class BoardGenService : IBoardGenService
    {
        protected IBoardService _boardService = null!;
        protected IGameData _gameData = null!;
        protected ISharedBoardGenService _sharedBoardGenService = null!;


        public Type GetKey() { return GetType(); }

        public int UserUpdatePriority { get; } = 0;

        public async Task<BoardData> GenerateBoard(WebContext context, BoardGenArgs args = null)
        {
            if (args == null)
            {
                // These are reasonable defaults for making a regular board for this user.
                args = new BoardGenArgs()
                {
                    BoardModeId = BoardModes.Primary,
                };
            }

            BoardData boardData = await context.GetAsync<BoardData>();
            CoreUserData userData = await context.GetAsync<CoreUserData>();

            BoardGenSettings genSettings = _gameData.Get<BoardGenSettings>(context.user);

            BoardMode boardMode = _gameData.Get<BoardModeSettings>(context.user).Get(boardData.BoardModeId);
            if (boardMode == null)
            {
                boardMode = _gameData.Get<BoardModeSettings>(context.user).Get(BoardModes.Primary);
            }
            if (boardData.BoardModeId > BoardModes.Primary)
            {
                BoardStackData boardStackData = await context.GetAsync<BoardStackData>();
                boardStackData.Boards.Add(boardData);

                boardData = new BoardData() { Id = context.user.Id };

                context.Set(boardData);
            }

            boardData.OwnerId = args.OwnerId;
            boardData.BoardModeId = args.BoardModeId;
            boardData.Seed = args.Seed;

            if (string.IsNullOrEmpty(boardData.OwnerId))
            {
                boardData.OwnerId = context.user.Id;
            }

            List<long> tiletypeIds = _sharedBoardGenService.GenerateTiles(context.user, args);

            boardData.CreateData(tiletypeIds);

            if (args.ForceZoneTypeId == 0)
            {
                IReadOnlyList<ZoneType> zoneTypes = _gameData.Get<ZoneTypeSettings>(context.user).GetData();

                List<ZoneType> okZoneTypes = zoneTypes.Where(x => x.IdKey > 0 && x.IdKey != boardData.ZoneTypeId).ToList();

                if (okZoneTypes.Count < 1)
                {
                    boardData.ZoneTypeId = 1;
                }
                else
                {
                    boardData.ZoneTypeId = okZoneTypes[context.rand.Next() % okZoneTypes.Count].IdKey;
                }
            }
            else
            {
                boardData.ZoneTypeId = args.ForceZoneTypeId;
            }

            return boardData;
        }

    }
}
