using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.GameDatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Factions.Entities
{
    [MessagePackObject]
    public class FactionSettings : BaseGameData
    {
        public override void Set(GameData gameData) { gameData.Set(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public List<FactionType> FactionTypes { get; set; }
        [Key(2)] public List<RepLevel> RepLevels { get; set; }

        public FactionType GetFactionType(long idkey) { return _lookup.Get<FactionType>(idkey); }
        public RepLevel GetRepLevel(long idkey) { return _lookup.Get<RepLevel>(idkey); }
    }
}
