using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Entities;

namespace Genrpg.Shared.NPCs.Entities
{

    [MessagePackObject]
    public class NPCStatus : BaseWorldData, IStatusItem, IId, IStringOwnerId
    {     
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string OwnerId { get; set; }
        [Key(2)] public string MapObjectId { get; set; }
        [Key(3)] public long IdKey { get; set; }
        [Key(4)] public string? MapId { get; set; }

        [Key(5)] public DateTime LastItemRefresh { get; set; }
        [Key(6)] public List<VendorItem> Items { get; set; }

        public NPCStatus()
        {
            Items = new List<VendorItem>();
        }

        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Save(this); }
    }
}
