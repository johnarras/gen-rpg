using MessagePack;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GroundObjects.MapObjects;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.MapObjects.Factories;
using Genrpg.Shared.MapMods.MapObjects;

namespace Genrpg.Shared.MapObjects.MapObjectAddons.Factories
{
    [MessagePackObject]
    public class MapModFactory : BaseMapObjectFactory
    {
        public override long GetKey() { return EntityTypes.MapMod; }

        public override MapObject Create(GameState gs, IMapSpawn spawn)
        {
            MapMod mapMod = new MapMod();
            mapMod.CopyDataToMapObjectFromMapSpawn(spawn);
            return mapMod;
        }
    }
}
