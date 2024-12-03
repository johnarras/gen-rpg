using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.Crawler.Roguelikes.Settings
{
    [MessagePackObject]
    public class RoguelikeSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public double LootScale { get; set; }
        [Key(2)] public double MonsterQuantityScale { get; set; }
        [Key(3)] public double RandomEncounterChanceMult { get; set; }

    }


    [MessagePackObject]
    public class RoguelikeSettingsLoader : NoChildSettingsLoader<RoguelikeSettings> { }



    [MessagePackObject]
    public class RoguelikeSettingsMapper : NoChildSettingsMapper<RoguelikeSettings> { }
}
