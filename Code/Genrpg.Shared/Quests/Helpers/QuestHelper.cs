﻿using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Interfaces;
using System.Linq;
using System.Threading.Tasks;
namespace Genrpg.Shared.Quests.Helpers
{
    public class QuestHelper : IEntityHelper
    {

        public long GetKey() { return EntityType.Quest; }
        public string GetDataPropertyName() { return "Quests"; }

        public IIndexedGameItem Find(GameState gs, long id)
        {
            if (gs.map == null ||
                gs.map.Quests == null)
            {
                return null;
            }

            return gs.map.Quests.FirstOrDefault(x => x.IdKey == id);
        }
    }
}