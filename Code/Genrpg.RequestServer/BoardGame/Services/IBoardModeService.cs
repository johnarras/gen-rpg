using Genrpg.RequestServer.BoardGame.BoardModeHelpers;
using Genrpg.RequestServer.BoardGame.Entities;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.BoardGame.Services
{
    public interface IBoardModeService : IInjectable
    {
        IBoardModeHelper GetBoardModeHelper(long boardModeId);

        Task SwitchToBoardMode(WebContext context, SwitchBoardModeArgs args);
    }
}
