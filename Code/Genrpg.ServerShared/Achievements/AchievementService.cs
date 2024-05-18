using Genrpg.Shared.Achievements.Constants;
using Genrpg.Shared.Achievements.Messages;
using Genrpg.Shared.Achievements.PlayerData;
using Genrpg.Shared.Achievements.Settings;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Achievements
{
    public class AchievementService : IAchievementService
    {
        private IGameData _gameData;
        private IRepositoryService _repoService;
        public async Task Initialize(GameState gs, CancellationToken toke)
        {
            await Task.CompletedTask;
        }

        public void UpdateAchievement(GameState gs, MapObject mapObject, long achievementTypeId, long quantity)
        {
            if (!(mapObject is Character ch))
            {
                return;
            }

            AchievementStatus status = ch.Get<AchievementData>().Get(achievementTypeId);

            AchievementType type = _gameData.Get<AchievementSettings>(ch).Get(achievementTypeId);

            if (type?.Category == AchievementCategories.Max)
            {
                if (quantity > status.Quantity)
                {
                    status.Quantity = quantity;
                    _repoService.QueueSave(status);
                    ch.AddMessage(new OnUpdateAchievement() { AchievementTypeId = (int)status.IdKey, Quantity = quantity });
                    // Send to clients
                }
            }
            else
            {
                status.Quantity += quantity;
                _repoService.QueueSave(status);
                ch.AddMessage(new OnUpdateAchievement() { AchievementTypeId = (int)status.IdKey, Quantity = quantity });
                // Send to client
            }
        }
    }
}
