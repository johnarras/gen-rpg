using Genrpg.MapServer.Quests.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Spawns.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Entities.Services
{
    public class ServerEntityService : EntityService
    {
        IServerQuestService _questService = null;
        public override bool GiveRewards<SR>(GameState gs, MapObject obj, List<SR> resultList)
        {

            foreach (ISpawnResult spawnResult in resultList)
            {
                _questService.UpdateQuest(gs, obj, spawnResult);
            }

            return base.GiveRewards(gs, obj, resultList);
        }
    }
}
