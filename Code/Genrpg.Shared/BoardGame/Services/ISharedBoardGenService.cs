using Genrpg.Shared.BoardGame.Entities;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.BoardGame.Services
{
    public interface ISharedBoardGenService : IInjectable
    {
        List<long> GetTiles(IFilteredObject filtered, BoardGenData genData);
    }
}
