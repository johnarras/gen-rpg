using MessagePack;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.GroundObjects.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;

namespace Genrpg.Shared.MapObjects.Factories
{
    [MessagePackObject]
    public class GroundObjectFactory : BaseMapObjectFactory
    {
        public override long GetKey() { return EntityTypes.GroundObject; }

        public override MapObject Create(GameState gs, IMapSpawn spawn)
        {
            GroundObject obj = new GroundObject();
            obj.CopyDataFromMapSpawn(spawn);
            Zone zone = gs.map.Get<Zone>(obj.ZoneId);
            if (zone != null)
            {
                obj.Level = zone.Level;
            }

            return obj;
        }
    }
}
