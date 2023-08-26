using Genrpg.Shared.Spawns.Entities;
using System.Collections.Generic;


namespace ClientEvents
{
    public class ShowLootEvent
    {
        public List<SpawnResult> Rewards { get; set; }
    }
}
