using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.BoardGame.Services
{
    public interface IBoardPrizeService : IInjectable
    {
        Task UpdatePrizesForBoard(WebContext context, BoardData boardData = null);
    }
}
