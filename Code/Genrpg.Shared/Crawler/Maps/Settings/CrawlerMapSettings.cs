using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Dungeons.Constants;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System.Transactions;

namespace Genrpg.Shared.Crawler.Maps.Settings
{
    [MessagePackObject]
    public class CrawlerMapSettings : ParentSettings<CrawlerMapType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public double CorridorDungeonSizeScale { get; set; } = 1.5f;
    }

    [MessagePackObject]
    public class CrawlerMapType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public int MinWidth { get; set; } = 15;
        [Key(8)] public int MaxWidth { get; set; } = 25;
        [Key(9)] public int MinHeight { get; set; } = 15;
        [Key(10)] public int MaxHeight { get; set; } = 25;
        [Key(11)] public int MinFloors { get; set; } = 1;
        [Key(12)] public int MaxFloors { get; set; } = 1;
        [Key(13)] public double QuestItemEntranceUnlockChance { get; set; } = 0;
        [Key(14)] public double RiddleUnlockChance { get; set; }
        [Key(15)] public double SpecialTileChance { get; set; }
        [Key(16)] public double DungeonGenChance { get; set; }
        [Key(17)] public long BuildingTypeId { get; set; }
        [Key(18)] public long ZoneTypeId { get; set; }
        [Key(19)] public long DungeonArtId { get; set; } = DungeonArtTypes.Basic;
        [Key(20)] public double RandomWallsChance { get; set; }
        [Key(21)] public double LoopingChance { get; set; }
        [Key(22)] public double MinWallChance { get; set; }
        [Key(23)] public double MaxWallChance { get; set; }
        [Key(24)] public double MinDoorChance { get; set; } 
        [Key(25)] public double MaxDoorChance { get; set; }
        [Key(26)] public double TrapTileChance { get; set; } 
        [Key(27)] public double EffectTileChance { get; set; }
        [Key(28)] public double MinCorridorDensity { get; set; }
        [Key(29)] public double MaxCorridorDensity { get; set; }
        [Key(30)] public double MinBuildingDensity { get; set; }
        [Key(31)] public double MaxBuildingDensity { get; set; }
    }

    [MessagePackObject]
    public class CrawlerMapSettingsApi : ParentSettingsApi<CrawlerMapSettings, CrawlerMapType> { }
    [MessagePackObject]
    public class CrawlerMapSettingsLoader : ParentSettingsLoader<CrawlerMapSettings, CrawlerMapType> { }

    [MessagePackObject]
    public class CrawlerMapSettingsMapper : ParentSettingsMapper<CrawlerMapSettings, CrawlerMapType, CrawlerMapSettingsApi> { }

}
