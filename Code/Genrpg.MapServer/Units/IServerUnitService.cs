﻿using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Units
{
    public interface IServerUnitService : ISetupService
    {
        void CheckForDeath(GameState gs, ActiveSpellEffect eff, Unit unit);
    }
}
