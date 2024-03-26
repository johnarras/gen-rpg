using Genrpg.MapServer.Maps;
using Genrpg.MapServer.Spawns.MapObjectAddons;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.MapMods.Constants;
using Genrpg.Shared.MapMods.MapObjectAddons;
using Genrpg.Shared.MapMods.MapObjects;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.MapMods.Helpers
{
    public class SpawnerMapModEffectHelper : IMapModEffectHelper
    {
        private IMapObjectManager _objectManager = null;
        private IPathfindingService _pathfindingService = null;
        protected IRepositoryService _repoService = null;

        const int MinSeparation = 5;

        public long GetKey() { return MapModEffects.Spawner; }

        public void Process(ServerGameState gs, MapMod mapMod, MapModAddon addon, MapModEffect effect)
        {
            if (effect.CurrQuantity >= effect.MaxQuantity)
            {
                return;
            }

            List<MapObject> nearbyObjects = _objectManager.GetObjectsNear(mapMod.X, mapMod.Z, null, addon.Radius+10);

            int childQuantity = 0;

            foreach (MapObject nearby in nearbyObjects)
            {
                DynamicSpawnAddon dynamicAddon = nearby.GetAddon<DynamicSpawnAddon>();

                if (dynamicAddon != null && dynamicAddon.ParentId == mapMod.Id)
                {
                    childQuantity++;
                }
            }

            if (childQuantity >= effect.MaxQuantity)
            {
                return;
            }

            for (int times = 0; times < 10; times++)
            {
                int xpos = (int)(mapMod.X + MathUtils.FloatRange(-addon.Radius, addon.Radius, gs.rand));
                int zpos = (int)(mapMod.Z + MathUtils.FloatRange(-addon.Radius, addon.Radius, gs.rand));

                if (_pathfindingService.CellIsBlocked(gs, xpos, zpos))
                {
                    continue;
                }

                foreach (MapObject mo in nearbyObjects)
                {
                    if (Math.Abs(mo.X - xpos) <= MinSeparation &&
                        Math.Abs(mo.Z - zpos) <= MinSeparation)
                    {
                        continue;
                    }
                }

                DynamicSpawnAddon dynamicAddon = new DynamicSpawnAddon() { ParentId = mapMod.Id };

                List<IMapObjectAddon> newAddons = new List<IMapObjectAddon>(){  dynamicAddon };

                MapSpawn newObjectSpawn = new MapSpawn()
                {
                    EntityTypeId = mapMod.EntityTypeId,
                    EntityId = mapMod.EntityId,
                    X = xpos,
                    Z = zpos,
                    FactionTypeId = mapMod.FactionTypeId,
                    LocationId = mapMod.LocationId,
                    LocationPlaceId = mapMod.LocationPlaceId,
                    ZoneId = mapMod.ZoneId,
                    Id = mapMod.Id + "-" + (++addon.TriggerTimes),
                    Addons = newAddons,
                };

                _objectManager.SpawnObject(gs, newObjectSpawn);

                if (mapMod.Spawn is MapSpawn mapModSpawn)
                {
                    _repoService.QueueSave(mapModSpawn);
                }
                effect.CurrQuantity++;

                break;
            }

            return;
        }
    }
}
