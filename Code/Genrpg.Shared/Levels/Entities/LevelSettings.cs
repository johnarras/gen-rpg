using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.GameDatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Levels.Entities
{
    [MessagePackObject]
    public class LevelSettings : BaseGameData
    {
        public override void Set(GameData gameData) { gameData.Set(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int MaxLevel { get; set; }
        [Key(2)] public List<LevelData> Levels { get; set; }

        public LevelData GetLevel (long idkey)
        {
            return _lookup.Get<LevelData>(idkey);
        }
    }
}
