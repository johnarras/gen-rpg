using MessagePack;

using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.AI.Entities;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.Entities.Constants;

namespace Genrpg.Shared.MapObjects.Factories
{
    [MessagePackObject]
    public class ZoneUnitFactory : BaseMapObjectFactory
    {
        private IStatService _statService;
        public override long GetKey() { return EntityTypes.ZoneUnit; }
        public override MapObject Create(GameState gs, IMapSpawn spawn)
        {

            Zone spawnZone = gs.map.Get<Zone>(spawn.EntityId);

            if (spawnZone == null)
            {
                spawnZone = gs.map.Get<Zone>(spawn.ZoneId);
            }

            if (spawnZone == null)
            {
                return null;
            }

            Zone levelZone = gs.map.Get<Zone>(spawn.ZoneId);

            if (levelZone == null)
            {
                return null;
            }

            UnitType utype = _unitGenService.GetRandomUnitType(gs, gs.map, spawnZone);

            if (utype == null)
            {
                return null;
            }

            long level = levelZone.GetFinalUnitLevel(gs, spawn.X, spawn.Z, levelZone.Level);


            Unit unit = new Unit();
            unit.Level = level;
            unit.CopyDataFromMapSpawn(spawn);
            unit.EntityTypeId = EntityTypes.Unit;
            unit.EntityId = utype.IdKey;
            unit.BaseSpeed = gs.data.GetGameData<AISettings>(unit).BaseUnitSpeed;
            unit.Speed = unit.BaseSpeed;

            if (spawn is OnSpawn onSpawn)
            {
                unit.Flags = onSpawn.TempFlags;
            }

            SpellType spellType = gs.data.GetGameData<SpellTypeSettings>(unit).GetSpellType(1);

            Spell spell = SerializationUtils.ConvertType<SpellType, Spell>(spellType);

            List<ElementType> etypes = gs.data.GetGameData<ElementTypeSettings>(unit).GetData();

            spell.ElementTypeId = etypes[gs.rand.Next() % etypes.Count].IdKey;
            spell.Id = HashUtils.NewGuid();

            SpellData spellData = unit.Get<SpellData>();
            spellData.Add(spell);

            unit.Name = _unitGenService.GenerateUnitName(gs, utype.IdKey, spawnZone.IdKey, gs.rand, null);

            _statService.CalcStats(gs, unit, true);

            return unit;
        }
    }
}
