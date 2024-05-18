using MessagePack;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Chat.Settings
{
    [MessagePackObject]
    public class ChatType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public string Color { get; set; }

    }

    [MessagePackObject]
    public class ChatSettings : ParentSettings<ChatType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class ChatSettingsApi : ParentSettingsApi<ChatSettings, ChatType> { }

    [MessagePackObject]
    public class ChatSettingsLoader : ParentSettingsLoader<ChatSettings, ChatType> { }

    [MessagePackObject]
    public class ChatSettingsMapper : ParentSettingsMapper<ChatSettings, ChatType, ChatSettingsApi> { }
}
