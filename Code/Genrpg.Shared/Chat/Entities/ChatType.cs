using MessagePack;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;

namespace Genrpg.Shared.Chat.Entities
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

        public ChatType GetChatType(long idkey) { return _lookup.Get<ChatType>(idkey); }
    }

    [MessagePackObject]
    public class ChatApi : ParentSettingsApi<ChatSettings, ChatType> { }

    [MessagePackObject]
    public class ChatSettingsLoader : ParentSettingsLoader<ChatSettings, ChatType, ChatApi> { }
}
