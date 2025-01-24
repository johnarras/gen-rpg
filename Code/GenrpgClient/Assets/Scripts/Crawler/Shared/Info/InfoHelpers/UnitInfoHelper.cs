using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Spells.Constants;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Utils;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public class UnitInfoHelper : BaseInfoHelper<UnitSettings, UnitType>
    {

        private ICrawlerCombatService _combatService;


        public override long GetKey() { return EntityTypes.Unit; }

        public override List<string> GetInfoLines(long entityId)
        {

            UnitType unitType = _gameData.Get<UnitSettings>(_gs.ch).Get(entityId);
            List<string> infoLines = new List<string>();

            infoLines.Add(unitType.Name);
            infoLines.Add(unitType.Desc);

            FullMonsterStats stats = _combatService.GetFullMonsterStats(unitType, 1000);

            infoLines.Add("Min Range: " + stats.Range);

            if (stats.IsGuardian)
            {
                infoLines.Add("GUARDIAN");
            }

            CrawlerSpellSettings spellSettings = _gameData.Get<CrawlerSpellSettings>(_gs.ch);
            ElementTypeSettings elementSettings = _gameData.Get<ElementTypeSettings>(_gs.ch);
            StatusEffectSettings statusSettings = _gameData.Get<StatusEffectSettings>(_gs.ch);

            infoLines.Add("Abilities:");


            foreach (UnitEffect unitSpell in stats.Spells)
            {
                CrawlerSpell spell = spellSettings.Get(unitSpell.EntityId);

                if (spell != null && spell.IdKey > CrawlerSpells.ShootId)
                {
                    infoLines.Add(" " + _infoService.CreateInfoLink(spell) + ": " + spell.Desc);
                }
            }

            StringBuilder sb = new StringBuilder();

            foreach (ElementType etype in elementSettings.GetData())
            {
                if (FlagUtils.IsSet(stats.ResistBits, etype.IdKey))
                {
                    sb.Append(_infoService.CreateInfoLink(etype) + " ");
                }
            }

            if (sb.Length > 0)
            {
                infoLines.Add("Resistances: " + sb.ToString());
            }

            sb.Clear();

            foreach (ElementType etype in elementSettings.GetData())
            {
                if (FlagUtils.IsSet(stats.VulnBits, etype.IdKey))
                {
                    sb.Append(_infoService.CreateInfoLink(etype) + " ");
                }
            }

            if (sb.Length > 0)
            {
                infoLines.Add("Vulnerabilities: " + sb.ToString());
            }

            sb.Clear();

            foreach (FullEffect applyEffect in stats.ApplyEffects)
            {
                StatusEffect statusEffect = statusSettings.Get(applyEffect.Effect.EntityId);

                if (statusEffect != null)
                {
                    sb.Append(_infoService.CreateInfoLink(statusEffect) + " ");
                }
            }

            if (sb.Length > 0)
            {
                infoLines.Add("On Hit Effects: " + sb.ToString());
            }



            return infoLines;
        }

    }
}
