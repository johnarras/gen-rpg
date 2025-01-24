using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Info.EffectHelpers;
using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.GameSettings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Info.SpellEffectHelpers
{
    public abstract class BaseSpellEffectHelper : ISpellEffectHelper
    {
        protected IInfoService _infoService;
        protected IEntityService _entityService;
        protected IGameData _gameData;
        protected IClientGameState _gs;
        protected ICrawlerService _crawlerService;


        public abstract long GetKey();
        public abstract string ShowEffectInfo(CrawlerSpell spell, CrawlerSpellEffect effect);

        protected virtual string GetRoleScalingText(CrawlerSpell spell, CrawlerSpellEffect effect, string prefix = " per ")
        {
            long roleScalingTypeId = 0;
            if (effect.EntityTypeId == EntityTypes.Damage)
            {
                if (spell.CombatActionId == CombatActions.Attack)
                {
                    roleScalingTypeId = RoleScalingTypes.Melee;
                }
                else if (spell.CombatActionId == CombatActions.Shoot)
                {
                    roleScalingTypeId = RoleScalingTypes.Ranged;
                }
                else
                {
                    roleScalingTypeId = RoleScalingTypes.SpellDam;
                }
            }
            else if (effect.EntityTypeId == EntityTypes.Healing)
            {
                roleScalingTypeId = RoleScalingTypes.Healing;
            }
            else if (effect.EntityTypeId == EntityTypes.StatusEffect)
            {
                if (effect.MinQuantity < 0)
                {
                    roleScalingTypeId = RoleScalingTypes.Healing;
                }
            }
            else if (effect.EntityTypeId == EntityTypes.Unit)
            {
                roleScalingTypeId = RoleScalingTypes.Summon;
            }

            RoleScalingType scalingType = _gameData.Get<RoleScalingTypeSettings>(_gs.ch).Get(roleScalingTypeId);

            if (scalingType != null)
            {
                return prefix + $"{_infoService.CreateInfoLink(scalingType)} Tier";
            }
            else
            {
                return "";
            }

        }
    }
}
