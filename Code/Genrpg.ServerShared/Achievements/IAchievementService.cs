using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;

namespace Genrpg.ServerShared.Achievements
{
    public interface IAchievementService : IInitializable
    {
        void UpdateAchievement(MapObject mapObject, long achievementTypeId, long quantity);
    }
}
