using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.UserAbilities.Settings;
using Genrpg.Shared.Users.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.UserAbilities.Services
{
    public class UserAbilityService : IUserAbilityService
    {
        private IGameData _gameData = null;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public long GetAbilityTotal(IFilteredObject filtered, long userAbilityId, long upgradeRank)
        {
            UserAbilityType abilityType = _gameData.Get<UserAbilitySettings>(filtered).Get(userAbilityId);

            if (abilityType == null)
            {
                return upgradeRank;
            }

            return abilityType.BaseQuantity + upgradeRank * abilityType.QuantityPerRank;
        }

    }
}
