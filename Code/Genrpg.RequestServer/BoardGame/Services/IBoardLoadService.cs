﻿using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.BoardGame.Services
{
    public interface IBoardLoadService : IInjectable
    {
        Task AfterBoardLoad(WebContext context);
    }
}
