using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Units.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.SpellEffectHelpers
{
    public class UnitSpellEffectHelper : BaseSpellEffectHelper
    {
        public override long GetKey() { return EntityTypes.Unit; }

        public override string ShowEffectInfo(CrawlerSpell spell, CrawlerSpellEffect effect)
        {
            StringBuilder sb = new StringBuilder();

            UnitType unitType = _gameData.Get<UnitSettings>(_gs.ch).Get(effect.EntityId);

            if (unitType != null)
            {
                sb.Append($"Summons {effect.MinQuantity} {_infoService.CreateInfoLink(unitType)} {GetRoleScalingText(spell,effect)} (Permanently if out of combat, and for the duration of combat if in combat.");
            }


            return sb.ToString();
        }
    }
}
