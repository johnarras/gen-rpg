using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roguelikes.Constants;
using Genrpg.Shared.Crawler.Roguelikes.Services;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Roles.Services
{

    public interface IRoleService : IInjectable
    {
        double GetScalingBonusPerLevel(PartyData partyData, CrawlerUnit unit, long roleScalingTypeId);
    }

    public class RoleService : IRoleService
    {

        private IGameData _gameData;
        private IClientGameState _gs;

        private IRoguelikeUpgradeService _roguelikeUpgradeService;

        public double GetScalingBonusPerLevel(PartyData partyData, CrawlerUnit unit, long roleScalingTypeId)
        {
            if (!unit.IsPlayer())
            {
                return _gameData.Get<CrawlerCombatSettings>(_gs.ch).MonsterScalingPerLevel;
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

            if (_gs.GameMode == EGameModes.Roguelike)
            {
                roleScaling += _roguelikeUpgradeService.GetBonus(partyData, RoguelikeUpgrades.AttackQuantity);
            }

            return roleScaling;



        }
    }
}
