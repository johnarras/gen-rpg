using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.SpellEffectHandlers
{
    /// <summary>
    /// Use this to convery the effect a spell has into a SpellEffect on the SpellHitData
    /// </summary>
    public interface ISpellEffectHandler : ISetupDictionaryItem<long>
    {
        bool IsModifyStatEffect();
        bool UseStatScaling();
        float GetTickLength();

        List<ActiveSpellEffect> CreateEffects(IRandom rand, SpellHit spellHit);

        bool HandleEffect(IRandom rand, ActiveSpellEffect eff);

    }
}
