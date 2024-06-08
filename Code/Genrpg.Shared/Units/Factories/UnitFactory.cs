using MessagePack;

using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.AI.Settings;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Spells.Settings.Spells;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Genrpg.Shared.Zones.WorldData;
using System.Linq;
using Genrpg.Shared.MapObjects.Factories;

namespace Genrpg.Shared.Units.Factories
{
    [MessagePackObject]
    public class UnitFactory : BaseMapObjectFactory
    {
        private IStatService _statService = null;
        public override long GetKey() { return EntityTypes.Unit; }
        public override MapObject Create(IRandom rand, IMapSpawn spawn)
        {
            UnitType utype = _gameData.Get<UnitSettings>(null).Get(spawn.EntityId);

            if (utype != null && utype.IdKey == 0)
            {
                utype = null;
            }

            Zone zone = _mapProvider.GetMap().Get<Zone>(spawn.ZoneId);

            long level = zone != null ? zone.Level : 1;
            if (utype == null)
            {
                if (spawn.ZoneId > 0)
                {
                    if (zone == null)
                    {
                        return null;
                    }
                    utype = _unitGenService.GetRandomUnitType(rand, _mapProvider.GetMap(), zone);
                }
            }

            if (zone != null)
            {
                level = zone.GetFinalUnitLevel(rand, spawn.X, spawn.Z, zone.Level, _mapProvider.GetMap().MaxLevel);
            }

            if (utype == null)
            {
                return null;
            }

            Unit unit = new Unit(_repoService);
            unit.Level = level;

            if (spawn.GetAddons().Any())
            {
                unit.Level += 3;
            }

            unit.CopyDataToMapObjectFromMapSpawn(spawn);
            unit.EntityTypeId = EntityTypes.Unit;
            unit.EntityId = utype.IdKey;
            unit.BaseSpeed = _gameData.Get<AISettings>(unit).BaseUnitSpeed;
            unit.Speed = unit.BaseSpeed;

            if (spawn is OnSpawn onSpawn)
            {
                unit.AddFlag(onSpawn.TempFlags);
            }

            SpellType spellType = _gameData.Get<SpellTypeSettings>(unit).Get(1);

            Spell spell = SerializationUtils.ConvertType<SpellType, Spell>(spellType);

            foreach (SpellEffect effect in spell.Effects)
            {
                effect.Scale /= 3;
            }

            IReadOnlyList<ElementType> etypes = _gameData.Get<ElementTypeSettings>(unit).GetData();

            spell.ElementTypeId = etypes[(rand.Next() % (etypes.Count-1) + 1)].IdKey;
            spell.Id = HashUtils.NewGuid();

            SpellData spellData = unit.Get<SpellData>();
            spellData.Add(spell);

            unit.Name = spawn.Name;
            if (string.IsNullOrEmpty(unit.Name))
            {
                if (zone != null)
                {
                    unit.Name = _unitGenService.GenerateUnitName(rand, utype.IdKey, zone.IdKey, null);
                }
                else
                {
                    unit.Name = utype.Name;
                }
            }

            _statService.CalcStats(unit, true);

            return unit;
        }
    }
}
