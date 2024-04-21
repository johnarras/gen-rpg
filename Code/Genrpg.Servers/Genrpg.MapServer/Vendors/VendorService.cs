using Amazon.Runtime.Internal.Util;
using Genrpg.MapServer.Items;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Vendors.MapObjectAddons;
using Genrpg.Shared.Vendors.Settings;
using Genrpg.Shared.Vendors.WorldData;
using Genrpg.Shared.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Vendors
{
    public interface IVendorService : IInitializable
    {
        void UpdateItems(GameState gs, MapObject mapObject);
    }


    public class VendorService : IVendorService
    {
        public async Task Initialize(GameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        private IItemGenService _itemGenService = null;
        protected IRepositoryService _repoService = null;
        private IGameData _gameData;

        public void UpdateItems(GameState gs, MapObject mapObject)
        {

            VendorAddon addon = mapObject.GetAddon<VendorAddon>();

            if (addon == null)
            {
                return;
            }

            double refreshMinutes = _gameData.Get<VendorSettings>(mapObject).VendorRefreshMinutes;

            if (refreshMinutes <= 0)
            {
                return;
            }

            if (addon.LastRefreshTime >= DateTime.UtcNow.AddMinutes(-refreshMinutes))
            {
                return;
            }

            int currItemCount = MathUtils.IntRange(addon.ItemCount, addon.ItemCount * 2, gs.rand);
            long level = mapObject.Level;
            Zone zone = gs.map.Get<Zone>(mapObject.ZoneId);

            if (zone != null)
            {
                level = zone.Level;
            }

            List<VendorItem> newItems = new List<VendorItem>();

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
                    newItems.Add(new VendorItem() { Item = item, Quantity = 1 });
                }
            }
            lock (mapObject.OnActionLock)
            {
                if (addon.LastRefreshTime >= DateTime.UtcNow.AddMinutes(-refreshMinutes))
                {
                    return;
                }

                addon.Items = newItems;
                addon.LastRefreshTime = DateTime.UtcNow;

                if (mapObject.Spawn is MapSpawn mapSpawn)
                {
                    mapSpawn = SerializationUtils.FastMakeCopy(mapSpawn);
                    mapSpawn.AddonString = SerializationUtils.Serialize(mapSpawn.Addons);
                    mapSpawn.Addons = null;
                    _repoService.QueueSave(mapSpawn);
                }
            }
        }
    }
}
