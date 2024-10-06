
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers;
using Genrpg.Shared.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.Constants;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Casting
{
    public class SpecialSpellCastingStateHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.SpecialSpellCast; }
        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            SelectSpellAction selectSpellAction = action.ExtraData as SelectSpellAction;

            if (selectSpellAction == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Missing Special Select Spell" };
            }

            CrawlerSpell spell = selectSpellAction.Spell;

            CrawlerSpellEffect specialEffect = spell.Effects.FirstOrDefault(x => x.EntityTypeId == EntityTypes.SpecialMagic);

            if (specialEffect == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Missing Special Select Spell Effect" };
            }

            ISpecialMagicHelper helper = _spellService.GetSpecialEffectHelper(specialEffect.EntityId);
            if (helper != null)
            {
                return await helper.HandleEffect(stateData, selectSpellAction, spell, specialEffect, token);
            }
            return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "That spell is missing a special effect." };
        }
    }
}
