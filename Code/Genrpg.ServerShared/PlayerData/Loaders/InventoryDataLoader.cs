using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.PlayerData.Loaders
{
    public class InventoryDataLoader : OwnerDataLoader<InventoryData,Item,InventoryApi>
    {
    }
}
