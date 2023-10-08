using MessagePack;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;

namespace Genrpg.Shared.Chat.Entities
{
    [MessagePackObject]
    public class ChatType : ChildSettings, IIndexedGameItem
    {
        public const long None = 0;
        public const long Tell = 1;
        public const long Say = 2;
        public const long Yell = 3;
        public const long Zone = 4;
        public const long Map = 5;
        public const long World = 6;
        public const long Guild = 7;
        public const long Party = 8;
        public const long Raid = 9;

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public string Desc { get; set; }
        [Key(4)] public string Icon { get; set; }
        [Key(5)] public string Art { get; set; }
        [Key(6)] public string Color { get; set; }

    }

    [MessagePackObject]
    public class ChatSettings : ParentSettings<ChatType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<ChatType> Data { get; set; } = new List<ChatType>();

        public ChatType GetChatType(long idkey) { return _lookup.Get<ChatType>(idkey); }
    }

    [MessagePackObject]
    public class ChatApi : ParentSettingsApi<ChatSettings, ChatType> { }

    [MessagePackObject]
    public class ChatSettingsLoader : ParentSettingsLoader<ChatSettings, ChatType, ChatApi> { }
}
