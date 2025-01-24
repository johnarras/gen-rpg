using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.UnitEffects.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.SpellEffectHelpers
{
    public class StatSpellEffectHelper : BaseSpellEffectHelper
    {
        public override long GetKey() { return EntityTypes.Stat; }

        public override string ShowEffectInfo(CrawlerSpell spell, CrawlerSpellEffect effect)
        {
            StringBuilder sb = new StringBuilder();

            StatType statType = _gameData.Get<StatSettings>(_gs.ch).Get(effect.EntityId);

            if (statType != null)
            {
                sb.Append($"Adds 1 {_infoService.CreateInfoLink(statType)} per level of the caster to the target for the duration of combat (only largest buff counts).");
            }

            return sb.ToString();
        }
    }
}
