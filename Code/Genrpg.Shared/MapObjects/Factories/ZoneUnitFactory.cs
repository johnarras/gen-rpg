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
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.AI.Entities;
using Genrpg.Shared.MapObjects.Messages;

namespace Genrpg.Shared.MapObjects.Factories
{
    [MessagePackObject]
    public class ZoneUnitFactory : BaseMapObjectFactory
    {
        private IStatService _statService;
        public override long GetKey() { return EntityType.ZoneUnit; }
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
            unit.EntityTypeId = EntityType.Unit;
            unit.EntityId = utype.IdKey;
            unit.BaseSpeed = gs.data.GetGameData<AISettings>().BaseUnitSpeed;
            unit.Speed = unit.BaseSpeed;

            if (spawn is OnSpawn onSpawn)
            {
                unit.Flags = onSpawn.TempFlags;
            }

            Spell spell = gs.data.GetGameData<SpellSettings>().GetSpell(1);

            spell = SerializationUtils.FastMakeCopy(spell);

            if (gs.rand.NextDouble() < 0.1f)
            {
                spell.Duration = gs.rand.Next(1, 4);
                spell.Scale /= 2;
            }
            List<ElementType> etypes = gs.data.GetList<ElementType>();

            spell.ElementTypeId = etypes[gs.rand.Next() % etypes.Count].IdKey;
            spell.FinalScale = spell.Scale;

            SpellData spellData = unit.Get<SpellData>();
            spellData.Add(spell);

            unit.Name = _unitGenService.GenerateUnitName(gs, utype.IdKey, spawnZone.IdKey, gs.rand, null);

            _statService.CalcStats(gs, unit, true);

            return unit;
        }
    }
}
