using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.UnitEffects.Services
{
    public interface IStatusEffectService : IInjectable
    {
        public string ShowStatusEffects(Unit unit, bool showAbbreviations);
    }
}
