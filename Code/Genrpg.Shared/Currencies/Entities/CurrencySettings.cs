using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Currencies.Entities
{
    [MessagePackObject]
    public class CurrencySettings : ParentSettings<CurrencyType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<CurrencyType> Data { get; set; }

        public CurrencyType GetCurrencyType(long idkey) { return _lookup.Get<CurrencyType>(idkey); }
    }

    [MessagePackObject]
    public class CurrencyType : ChildSettings, IIndexedGameItem
    {
        public const int None = 0;
        public const int Gems = 1;
        public const int Money = 2;
        public const int Exp = 3;
        public const int Ability = 4;


        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string PluralName { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }

    }

    [MessagePackObject]
    public class CurrencySettingsApi : ParentSettingsApi<CurrencySettings, CurrencyType> { }

    [MessagePackObject]
    public class CurrencySettingsLoader : ParentSettingsLoader<CurrencySettings, CurrencyType, CurrencySettingsApi> { }

}
