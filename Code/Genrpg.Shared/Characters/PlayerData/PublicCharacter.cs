using Genrpg.Shared.DataStores.Categories.ContentData;
using Genrpg.Shared.DataStores.Constants;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Characters.PlayerData
{
    [MessagePackObject]
    public class PublicCharacter : BaseGameContentData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public long FactionTypeId { get; set; }
        [Key(3)] public long UnitTypeId { get; set; }
        [Key(4)] public long SexTypeId { get; set; }

    }
}
