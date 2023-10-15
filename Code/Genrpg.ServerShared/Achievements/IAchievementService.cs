using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Achievements
{
    public interface IAchievementService : IService
    {
        void UpdateAchievement(GameState gs, MapObject mapObject, long achievementTypeId, long quantity);
    }
}
