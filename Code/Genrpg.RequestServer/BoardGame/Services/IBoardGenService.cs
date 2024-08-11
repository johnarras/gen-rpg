using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Entities;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.BoardGame.Services
{
    public interface IBoardGenService : IInjectable
    {
        Task<BoardData> GenerateBoard(WebContext context, BoardGenData genData = null);
    }
}
