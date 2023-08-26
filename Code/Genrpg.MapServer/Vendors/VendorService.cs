using Genrpg.MapServer.Items;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.NPCs.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Vendors
{
    public interface IVendorService : IService
    {
        void UpdateNPCItems(GameState gs, Unit unit);
    }


    public class VendorService : IVendorService
    {
        private IItemGenService _itemGenService;

        public void UpdateNPCItems(GameState gs, Unit unit)
        {
           
            if (unit.NPCType == null || unit.NPCType.ItemCount < 1 ||
                gs.data.GetGameData<VendorSettings>().VendorRefreshMinutes <= 0)
            {
                return;
            }

            if (unit.NPCStatus == null || unit.NPCStatus.LastItemRefresh > DateTime.UtcNow.AddMinutes(-gs.data.GetGameData<VendorSettings>().VendorRefreshMinutes))
            {
                return;
            }

            lock (unit.OnActionLock)
            {
                NPCStatus newStatus = new NPCStatus() { LastItemRefresh = DateTime.UtcNow, IdKey = unit.NPCType.IdKey, MapId = unit.NPCStatus.MapId };

                int currItemCount = MathUtils.IntRange(unit.NPCType.ItemCount, unit.NPCType.ItemCount * 2, gs.rand);

                long level = unit.NPCType.Level;

                Zone zone = gs.map.Get<Zone>(unit.NPCType.ZoneId);

                if (zone != null)
                {
                    level = zone.Level;
                }

                for (int i = 0; i < currItemCount; i++)
                {
                    ItemGenData igd = new ItemGenData()
                    {
                        Level = level,
                        Quantity = 1,
                    };

                    Item item = _itemGenService.Generate(gs, igd);

                    if (item != null)
                    {
                        VendorItem vi = new VendorItem() { Item = item, Quantity = 1 };
                        newStatus.Items.Add(vi);
                    }
                }
                unit.NPCStatus = newStatus;
            }
        }
    }
}
