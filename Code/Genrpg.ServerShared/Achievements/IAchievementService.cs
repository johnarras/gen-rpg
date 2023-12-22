using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;

namespace Genrpg.ServerShared.Achievements
{
    public interface IAchievementService : IService
    {
        void UpdateAchievement(GameState gs, MapObject mapObject, long achievementTypeId, long quantity);
    }
}
