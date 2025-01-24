using Genrpg.Shared.Crawler.Info.EffectHelpers;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Spells.Settings.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.SpellEffectHelpers
{
    public abstract class BaseNumericSpellEffectHelper : BaseSpellEffectHelper
    {

        virtual protected string TierSuffix() { return $"{_infoService.CreateInfoLink(_gameData.Get<RoleScalingTypeSettings>(_gs.ch).Get(GetKey()))} per Tier"; }

        public override string ShowEffectInfo(CrawlerSpell spell, CrawlerSpellEffect effect)
        {
            StringBuilder sb = new StringBuilder();
            if (effect.MinQuantity > 0 && effect.MaxQuantity > 0)
            {
                sb.Append("[" + effect.MinQuantity + "-" + effect.MaxQuantity + "] ");
            }

            ElementType elemType = _gameData.Get<ElementTypeSettings>(_gs.ch).Get(effect.ElementTypeId);

            if (elemType != null)
            {
                sb.Append(_infoService.CreateInfoLink(elemType) + " ");
            }

            EntityType etype = _gameData.Get<EntitySettings>(_gs.ch).Get(effect.EntityTypeId);
            if (etype != null)
            {
                sb.Append(etype.Name + " ");

                List<IIdName> children = _entityService.GetChildList(_gs.ch, etype.IdKey);

                IIdName child = children.FirstOrDefault(x => x.IdKey == effect.EntityId);

                if (child != null)
                {
                    sb.Append(_infoService.CreateInfoLink(child) + " ");
                }
            }
            sb.Append(GetRoleScalingText(spell,effect));
            return sb.ToString();
        }
    }
}
