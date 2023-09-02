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
    public class UnitFactory : BaseMapObjectFactory
    {
        private IStatService _statService;
        public override long GetKey() { return EntityType.Unit; }
        public override MapObject Create(GameState gs, IMapSpawn spawn)
        {
            UnitType utype = gs.data.GetGameData<UnitSettings>().GetUnitType(spawn.EntityId);

            if (utype != null && utype.IdKey == 0)
            {
                utype = null;
            }

            Zone zone = gs.map.Get<Zone>(spawn.ZoneId);

            long level = zone != null ? zone.Level : 1;
            if (utype == null)
            {
                if (spawn.ZoneId > 0)
                {
                    if (zone == null)
                    {
                        return null;
                    }
                    utype = _unitGenService.GetRandomUnitType(gs, gs.map, zone);
                }
            }

            if (zone != null)
            {
                level = zone.GetFinalUnitLevel(gs, spawn.X, spawn.Z, zone.Level);
            }

            if (utype == null)
            {
                return null;
            }

            if (spawn.EntityTypeId != EntityType.NPC)
            {
                level += MathUtils.IntRange(0, 2, gs.rand);
            }

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

            spell.Scale /= 3;
            List<ElementType> etypes = gs.data.GetList<ElementType>();

            spell.ElementTypeId = etypes[gs.rand.Next() % etypes.Count].IdKey;
            spell.FinalScale = spell.Scale;

            SpellData spellData = unit.Get<SpellData>();
            spellData.Add(spell);

            unit.Name = spawn.GetName();
            if (string.IsNullOrEmpty(unit.Name))
            {
                if (zone != null)
                {
                    unit.Name = _unitGenService.GenerateUnitName(gs, utype.IdKey, zone.IdKey, gs.rand, null);
                }
                else
                {
                    unit.Name = utype.Name;
                }
            }

            _statService.CalcStats(gs, unit, true);

            return unit;
        }
    }
}
