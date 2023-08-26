using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.GameDatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Stats.Entities
{
    [MessagePackObject]
    public class StatSettings : BaseGameData
    {
        public override void Set(GameData gameData) { gameData.Set(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int StatConstantUnitMultiple { get; set; }
        [Key(2)] public List<StatType> StatTypes { get; set; }
        [Key(3)] public List<DerivedStat> DerivedStats { get; set; }

        public StatType GetStatType(long idkey)
        {
            return _lookup.Get<StatType>(idkey);
        }
    }
}
