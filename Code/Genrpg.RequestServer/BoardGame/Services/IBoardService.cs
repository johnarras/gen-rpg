using Genrpg.RequestServer.BoardGame.BoardModeHelpers;
using Genrpg.RequestServer.BoardGame.Helpers.TileTypeHelpers;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.RequestServer.BoardGame.Services
{
    public interface IBoardService : IInjectable
    {
        ITileTypeHelper GetTileTypeHelper(long tileTypeId);

        List<long> GetTileTypesWithPrizes(WebContext context);
    }
}
