using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Factions.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.NPCs.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.Constants;

namespace Genrpg.Shared.MapObjects.Factories
{
    [MessagePackObject]
    public class NPCFactory : UnitFactory
    {
        public override long GetKey() { return EntityTypes.NPC; }

        public override MapObject Create(GameState gs, IMapSpawn spawn)
        {

            NPCType npcType = gs.map.Get<NPCType>(spawn.EntityId);
            if (npcType == null)
            {
                return null;
            }

            MapSpawn unitSpawn = new MapSpawn()
            {
                MapObjectId = spawn.MapObjectId,
                EntityTypeId = EntityTypes.Unit,
                EntityId = npcType.UnitTypeId,
                X = npcType.MapX,
                Z = npcType.MapZ,
            };

            Unit unit = base.Create(gs, unitSpawn) as Unit;

            if (unit == null)
            {
                return null;
            }
            unit.EntityTypeId = EntityTypes.Unit;
            unit.NPCType = npcType;
            unit.NPCTypeId = npcType.IdKey;
            unit.NPCStatus = new NPCStatus() { MapId = gs.map.Id, IdKey = npcType.IdKey, LastItemRefresh = DateTime.UtcNow.AddHours(-24) };
            unit.FactionTypeId = FactionTypes.Player;
            unit.Level += 10;
            if (unit.Level > gs.map.MaxLevel)
            {
                unit.Level = gs.map.MaxLevel;
            }

            return unit;
        }
    }
}
