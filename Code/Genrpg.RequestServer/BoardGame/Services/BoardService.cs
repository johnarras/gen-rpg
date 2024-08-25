using Genrpg.RequestServer.BoardGame.BoardModeHelpers;
using Genrpg.RequestServer.BoardGame.Helpers.TileTypeHelpers;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.RequestServer.BoardGame.Services
{
    public class BoardService : IBoardService
    {
        private IBoardGenService _boardGenService = null!;
        private IGameData _gameData;
        private SetupDictionaryContainer<long, ITileTypeHelper> _tileTypeHelpers = new SetupDictionaryContainer<long, ITileTypeHelper>();

        public ITileTypeHelper GetTileTypeHelper(long tileTypeId)
        {
            if (_tileTypeHelpers.TryGetValue(tileTypeId, out ITileTypeHelper helper))
            {
                return helper;
            }
            return null;
        }

        public List<long> GetTileTypesWithPrizes(WebContext context)
        {
            return _gameData.Get<TileTypeSettings>(context.user).GetData().Where(x => x.HasPrizes).Select(X => X.IdKey).ToList();
        }
    }
}
