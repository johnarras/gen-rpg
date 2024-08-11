using Genrpg.RequestServer.BoardGame.Services;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.PlayerData.LoadUpdateHelpers;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.PlayerData;

namespace Genrpg.RequestServer.BoardGame.Helpers
{
    public class UpdateBoardOnLoginHelper : IUserLoadUpdater
    {
        protected IBoardGenService _boardGenService = null!;
        protected IRepositoryService _repoService = null!;

        public int UserUpdatePriority => 0;
        public Type GetKey() { return GetType(); }

        public async Task Update(WebContext context, List<IUnitData> unitData)
        {
            BoardData boardData = await context.GetAsync<BoardData>();

            if (boardData.Length == 0)
            {
                boardData = await _boardGenService.GenerateBoard(context);
                await _repoService.Save(boardData);

                BoardData existingData = (BoardData)unitData.FirstOrDefault(x => x is BoardData boardData2);
                if (existingData != null)
                {
                    unitData.Remove(existingData);
                }
                unitData.Add(boardData);
            }

        }
    }
}
