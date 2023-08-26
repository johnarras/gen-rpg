using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Entities;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.Levels.Entities
{
    [MessagePackObject]
    public class LevelData : IIndexedGameItem
    {
        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }
        [Key(4)] public List<SpawnResult> RewardList { get; set; }

        [Key(5)] public long CurrExp { get; set; }
        [Key(6)] public float MobCount { get; set; }
        [Key(7)] public long MobExp { get; set; }
        [Key(8)] public float QuestCount { get; set; }
        [Key(9)] public long QuestExp { get; set; }
        [Key(10)] public long KillMoney { get; set; }

        [Key(11)] public int StatAmount { get; set; }
        [Key(12)] public int MonsterStatScale { get; set; }

        [Key(13)] public int AbilityPoints { get; set; }

        [Key(14)] public string Art { get; set; }


        public LevelData()
        {
            RewardList = new List<SpawnResult>();
        }
    }
}
