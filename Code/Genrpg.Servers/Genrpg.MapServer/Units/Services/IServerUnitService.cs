using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Units.Services
{
    public interface IServerUnitService : IInitializable
    {
        void CheckForDeath(IRandom rand, ActiveSpellEffect eff, Unit unit);
    }
}
