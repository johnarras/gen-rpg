﻿using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.BoardGame.Services
{
    public interface IDiceRollService : IInjectable
    {
        Task RollDice(WebContext context, DiceRollParams rollData);
    }
}
