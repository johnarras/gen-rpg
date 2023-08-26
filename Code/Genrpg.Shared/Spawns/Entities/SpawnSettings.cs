using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.GameDatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spawns.Entities
{
    [MessagePackObject]
    public class SpawnSettings : BaseGameData
    {
        public override void Set(GameData gameData) { gameData.Set(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public float MapSpawnChance { get; set; }
        [Key(2)] public long MonsterLootSpawnTableId { get; set; }
        [Key(3)] public List<SpawnTable> SpawnTables { get; set; }


        public SpawnTable GetSpawnTable(long idkey) { return _lookup.Get<SpawnTable>(idkey); }
    }
}
