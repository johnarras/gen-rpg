using MessagePack;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GroundObjects.MapObjects;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.MapObjects.Factories;

namespace Genrpg.Shared.GroundObjects.Factories
{
    [MessagePackObject]
    public class GroundObjectFactory : BaseMapObjectFactory
    {
        public override long GetKey() { return EntityTypes.GroundObject; }

        public override MapObject Create(GameState gs, IMapSpawn spawn)
        {
            GroundObject obj = new GroundObject(_repoService);
            obj.CopyDataToMapObjectFromMapSpawn(spawn);
            Zone zone = gs.map.Get<Zone>(obj.ZoneId);
            if (zone != null)
            {
                obj.Level = zone.Level;
            }

            return obj;
        }
    }
}
