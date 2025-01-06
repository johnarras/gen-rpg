using Genrpg.Shared.Crawler.Roles.Constants;
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
    public class CrawlerSpellInfoHelper : BaseInfoHelper
    {
        public override long GetKey() { return EntityTypes.CrawlerSpell; }

        public override List<string> GetInfoLines(long entityId)
        {

            CrawlerSpell spell = _gameData.Get<CrawlerSpellSettings>(_gs.ch).Get(entityId);

            List<string> allLines = new List<string>();


            allLines.Add(spell.Name + ": Level " + spell.Level);
            allLines.Add("Cost: " + spell.PowerCost + " " + spell.PowerPerLevel);

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
                    sb.Append(cl.Name + " ");   
                }
                allLines.Add(sb.ToString());    
            }

            if (races.Count > 0)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("Races: ");

                foreach (Role cl in classes)
                {
                    sb.Append(cl.Name + " ");
                }
                allLines.Add(sb.ToString());
            }


            EntitySettings entitySettings = _gameData.Get<EntitySettings>(_gs.ch);
            ElementTypeSettings elementSettings = _gameData.Get<ElementTypeSettings>(_gs.ch);

            foreach (CrawlerSpellEffect effect in spell.Effects)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("Effect: ");

                if (effect.MinQuantity > 0 && effect.MaxQuantity > 0)
                {
                    sb.Append("[" + effect.MinQuantity + "-" + effect.MaxQuantity + "] ");
                }

                ElementType elemType = elementSettings.Get(effect.ElementTypeId);

                if (elemType != null)
                {
                    sb.Append(elemType.Name + " ");
                }

                EntityType etype = entitySettings.Get(effect.EntityTypeId);
                if (etype != null)
                {
                    sb.Append(etype.Name + " ");

                    List<IIdName> children = _entityService.GetChildList(_gs.ch, etype.IdKey);

                    IIdName child = children.FirstOrDefault(x => x.IdKey == effect.EntityId);

                    if (child != null)
                    {
                        sb.Append(child.Name + " ");
                    }
                }

                allLines.Add(sb.ToString());    
            }



            return allLines;
        }
    }
}
