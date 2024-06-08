using MessagePack;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GroundObjects.MapObjects;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.Buildings.MapObjects;
using Genrpg.Shared.MapObjects.Factories;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Buildings.Factories
{
    [MessagePackObject]
    public class BuildingObjectFactory : BaseMapObjectFactory
    {
        public override long GetKey() { return EntityTypes.Building; }

        public override MapObject Create(IRandom rand, IMapSpawn spawn)
        {
            Building obj = new Building(_repoService);
            obj.CopyDataToMapObjectFromMapSpawn(spawn);
            Zone zone = _mapProvider.GetMap().Get<Zone>(obj.ZoneId);
            if (zone != null)
            {
                obj.Level = zone.Level;
            }

            return obj;
        }
    }
}
