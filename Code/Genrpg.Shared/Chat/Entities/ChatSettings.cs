using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.GameDatas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Chat.Entities
{
    [MessagePackObject]
    public class ChatSettings : BaseGameData
    {
        public override void Set(GameData gameData) { gameData.Set(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public List<ChatType> ChatTypes { get; set; }

        public ChatType GetChatType(long idkey) { return _lookup.Get<ChatType>(idkey); }
    }
}
