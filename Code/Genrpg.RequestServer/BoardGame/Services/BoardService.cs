using Genrpg.RequestServer.BoardGame.Helpers.TileTypeHelpers;
using Genrpg.Shared.HelperClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.RequestServer.BoardGame.Services
{
    public class BoardService : IBoardService
    {
        protected IBoardGenService _boardGenService = null!;

        SetupDictionaryContainer<long, ITileTypeHelper> _tileTypeHelpers = new SetupDictionaryContainer<long, ITileTypeHelper>();

        public ITileTypeHelper GetTileTypeHelper(long tileTypeId)
        {
            if (_tileTypeHelpers.TryGetValue(tileTypeId, out ITileTypeHelper helper))
            {
                return helper;
            }
            return null;
        }

    }
}
