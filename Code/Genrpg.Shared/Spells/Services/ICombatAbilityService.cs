using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Services
{
    public interface ICombatAbilityService : IInjectable
    {
        int GetRank(Unit unit, long abilityCategoryId, long abilityTypeId);
        void SetRank(Unit unit, long abilityCategoryId, long abilityTypeId, int rank);
        void AddRank(Unit unit, long abilityCategoryId, long abilityTypeId, int points);
    }
}
