using MessagePack;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Factions.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Spawns.Entities;

using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.Constants;

namespace Genrpg.Shared.MapObjects.Factories
{
    [MessagePackObject]
    public class ProxyCharacterFactory : UnitFactory
    {
        public override long GetKey() { return EntityTypes.ProxyCharacter; }

        public override MapObject Create(GameState gs, IMapSpawn spawn)
        {

            MapSpawn unitSpawn = new MapSpawn()
            {
                MapObjectId = spawn.MapObjectId,
                EntityTypeId = EntityTypes.Unit,
                EntityId = spawn.EntityId,
                X = spawn.X,
                Z = spawn.Z,
            };

            Unit unit = base.Create(gs, unitSpawn) as Unit;

            if (unit == null)
            {
                return null;
            }
            unit.FactionTypeId = FactionTypes.Player;

            if (spawn is OnSpawn onSpawn)
            {
                unit.Stats = onSpawn.Stats;
                unit.Level = onSpawn.Level;
                unit.Name = onSpawn.Name;
                unit.Speed = onSpawn.Speed;
                unit.BaseSpeed = onSpawn.Speed;
                unit.AddFlag(UnitFlags.ProxyCharacter);
            }
            return unit;
        }
    }
}
