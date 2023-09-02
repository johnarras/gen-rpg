using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;

namespace Genrpg.Shared.Currencies.Entities
{
    [MessagePackObject]
    public class CurrencySettings : BaseGameData
    {
        public override void Set(GameData gameData) { gameData.Set(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public List<CurrencyType> CurrencyTypes { get; set; }

        public CurrencyType GetCurrencyType(long idkey) { return _lookup.Get<CurrencyType>(idkey); }
    }
}
