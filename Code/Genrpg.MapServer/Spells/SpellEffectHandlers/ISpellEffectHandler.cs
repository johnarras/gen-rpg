using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Spells.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.SpellEffectHandlers
{
    /// <summary>
    /// Use this to convery the effect a spell has into a SpellEffect on the SpellHitData
    /// </summary>
    public interface ISpellEffectHandler : ISetupDictionaryItem<long>
    {
        void Init(GameState gs);
        bool IsModifyStatEffect();
        bool UseStatScaling();
        float GetTickLength();

        List<ActiveSpellEffect> CreateEffects(GameState gs, SpellHit spellHit);

        bool HandleEffect(GameState gs, ActiveSpellEffect eff);

    }
}
