using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Monsters.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roguelikes.Constants;
using Genrpg.Shared.Crawler.Roguelikes.Services;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Stats.Constants;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Roles.Services
{

    public interface IRoleService : IInjectable
    {
        double GetScalingBonusPerLevel(PartyData partyData, CrawlerUnit unit, long roleScalingTypeId);
        double GetScalingTier(PartyData partyData, CrawlerUnit unit, long roleScalingTypeId);    
    }

    public class RoleService : IRoleService
    {

        private IGameData _gameData;
        private IClientGameState _gs;
        private ILogService _logService;

        private IRoguelikeUpgradeService _roguelikeUpgradeService;

        public long GetBaseScalingTier(PartyData partyData, CrawlerUnit unit, long roleScalingTypeId)
        {

            return (long)GetScalingTier(partyData, unit, roleScalingTypeId);    
        }

        public double GetScalingTier(PartyData partyData, CrawlerUnit unit, long roleScalingTypeId)
        {

            double scalingPerLevel = GetScalingBonusPerLevel(partyData, unit, roleScalingTypeId);
            return (1 + scalingPerLevel * (unit.Level - 1));
        }

        public double GetScalingBonusPerLevel(PartyData partyData, CrawlerUnit unit, long roleScalingTypeId)
        {
            if (!unit.IsPlayer())
            {
                double baseVal = _gameData.Get<CrawlerMonsterSettings>(_gs.ch).ScalingPerLevel;
                double unitTypeVal = unit.Stats.Max(StatTypes.RoleScalingPercent);

                if (unitTypeVal != 0)
                {
                    baseVal += unitTypeVal / 100.0;
                }
                return baseVal;
            }

            double roleScaling = 0;

            RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);

            List<Role> roles = roleSettings.GetRoles(unit.Roles);

            foreach (Role role in roles)
            {
                RoleBonusAmount bonusAmount = role.AmountBonuses.FirstOrDefault(x=>x.EntityTypeId == EntityTypes.RoleScaling && x.EntityId == roleScalingTypeId);
                if (bonusAmount != null)
                {
                    roleScaling += bonusAmount.Amount;
                }
            }

            if (partyData.GameMode == ECrawlerGameModes.Roguelite)
            {
                roleScaling += _roguelikeUpgradeService.GetBonus(partyData, RoguelikeUpgrades.AttackQuantity);
            }

            return roleScaling;



        }
    }
}
