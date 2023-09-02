using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;

namespace Genrpg.Shared.Units.Entities
{
    [MessagePackObject]
    public class UnitSettings : BaseGameData
    {
        public override void Set(GameData gameData) { gameData.Set(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public List<UnitType> UnitTypes { get; set; }
        [Key(2)] public List<TribeType> TribeTypes { get; set; }


        public UnitType GetUnitType (long idkey)
        {
            return _lookup.Get<UnitType>(idkey);
        }

        public TribeType GetTribeType(long idkey)
        {
            return _lookup.Get<TribeType>(idkey);
        }
    }
}
