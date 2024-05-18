using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using MessagePack;

namespace Genrpg.Shared.Crawler.Buffs.Settings
{

    [MessagePackObject]
    public class PartyBuff : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }

    }


    [MessagePackObject]
    public class PartyBuffSettings : ParentSettings<PartyBuff>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class PartyBuffSettingsApi : ParentSettingsApi<PartyBuffSettings, PartyBuff> { }
    [MessagePackObject]
    public class PartyBuffSettingsLoader : ParentSettingsLoader<PartyBuffSettings, PartyBuff> { }


    [MessagePackObject]
    public class PartyBuffSettingsMapper : ParentSettingsMapper<PartyBuffSettings, PartyBuff, PartyBuffSettingsApi> { }



}
