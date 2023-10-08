using Genrpg.Shared.Currencies.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Units.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crafting.Entities
{
    [MessagePackObject]
    public class CrafterData : OwnerIdObjectList<CrafterStatus>
    {
        [Key(0)] public override string Id { get; set; }
    }
}
