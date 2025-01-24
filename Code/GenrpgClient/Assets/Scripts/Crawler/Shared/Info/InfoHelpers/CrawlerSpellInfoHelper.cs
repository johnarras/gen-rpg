using Genrpg.Shared.Crawler.Info.InfoHelpers;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Services;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Spells.Settings.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.Helpers
{
    public class CrawlerSpellInfoHelper : BaseInfoHelper<CrawlerSpellSettings, CrawlerSpell>
    {

        private IRoleService _roleService;

        public override long GetKey() { return EntityTypes.CrawlerSpell; }

        public override List<string> GetInfoLines(long entityId)
        {

            CrawlerSpell spell = _gameData.Get<CrawlerSpellSettings>(_gs.ch).Get(entityId);

            List<string> allLines = new List<string>();

            RoleScalingType scalingType = _gameData.Get<RoleScalingTypeSettings>(_gs.ch).Get(spell.RoleScalingTypeId);

            allLines.Add(spell.Name + ": Tier " + spell.Level + " " + _infoService.CreateInfoLink(scalingType) + " Scaling");
            if (spell.PowerPerLevel == 0)
            {
                allLines.Add("Cost: " + spell.PowerCost);
            }
            else
            {
                allLines.Add("Cost: " + spell.PowerCost + " +" + spell.PowerPerLevel + "/Tier");
            }

            TargetType ttype = _gameData.Get<TargetTypeSettings>(_gs.ch).Get(spell.TargetTypeId);

            allLines.Add("Target: " + ttype.Name + " " + ttype.Desc);

            allLines.Add("Desc: " + spell.Desc);

            IReadOnlyList<Role> allRoles = _gameData.Get<RoleSettings>(_gs.ch).GetData();

            List<Role> classes = new List<Role>();
            List<Role> races = new List<Role>();

            foreach (Role role in allRoles)
            {
                if (role.BinaryBonuses.Any(x=>x.EntityTypeId == EntityTypes.Spell && x.EntityId == spell.IdKey))
                {
                    if (role.RoleCategoryId == RoleCategories.Class)
                    {
                        classes.Add(role);
                    }
                    else if (role.RoleCategoryId == RoleCategories.Origin)
                    {
                        races.Add(role);    
                    }
                }
            }

            if (classes.Count > 0)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("Classes: ");

                foreach (Role cl in classes)
                {
                    sb.Append(_infoService.CreateInfoLink(cl) + " ");   
                }
                allLines.Add(sb.ToString());    
            }

            if (races.Count > 0)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("Races: ");

                foreach (Role cl in classes)
                {
                    sb.Append(_infoService.CreateInfoLink(cl) + " ");
                }
                allLines.Add(sb.ToString());
            }


            EntitySettings entitySettings = _gameData.Get<EntitySettings>(_gs.ch);
            ElementTypeSettings elementSettings = _gameData.Get<ElementTypeSettings>(_gs.ch);

            foreach (CrawlerSpellEffect effect in spell.Effects)
            {
                string effectText = _infoService.GetEffectText(spell, effect);

                if (!string.IsNullOrEmpty(effectText))
                {
                    allLines.Add("Effect:  " + effectText);
                }                
            }
            return allLines;
        }
    }
}
