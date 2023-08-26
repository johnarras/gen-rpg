using MessagePack;
using Genrpg.Shared.DataStores.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Data;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Inventory.Entities
{
    [MessagePackObject]
    public class InventoryApi : OwnerApiList<InventoryData,Item>
    {
        [Key(0)] public List<Item> AllItems { get; set; } = new List<Item>();

        public override void SaveAll(IRepositorySystem repoSystem)
        {
        }
        public override void Delete(IRepositorySystem repoSystem)
        {
        }
    }
}
