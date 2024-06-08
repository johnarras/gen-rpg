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
using Genrpg.Shared.MapObjects.Factories;
using Genrpg.Shared.GameSettings;

namespace Genrpg.Shared.Units.Factories
{
    [MessagePackObject]
    public class ZoneUnitFactory : BaseMapObjectFactory
    {
        private IStatService _statService = null;
        public override long GetKey() { return EntityTypes.ZoneUnit; }
        public override MapObject Create(IRandom rand, IMapSpawn spawn)
        {

            Zone spawnZone = _mapProvider.GetMap().Get<Zone>(spawn.EntityId);

            if (spawnZone == null)
            {
                spawnZone = _mapProvider.GetMap().Get<Zone>(spawn.ZoneId);
            }

            if (spawnZone == null)
            {
                return null;
            }

            if (_mapProvider.GetMap().OverrideZoneId > 0 && _mapProvider.GetMap().OverrideZonePercent >= spawn.OverrideZonePercent)
            {
                Zone newSpawnZone = _mapProvider.GetMap().Get<Zone>(_mapProvider.GetMap().OverrideZoneId);
                if (newSpawnZone != null)
                {
                    spawnZone = newSpawnZone;
                }
            }

            Zone levelZone = _mapProvider.GetMap().Get<Zone>(spawn.ZoneId);

            if (levelZone == null)
            {
                return null;
            }

            UnitType utype = _unitGenService.GetRandomUnitType(rand, _mapProvider.GetMap(), spawnZone);

            if (utype == null)
            {
                return null;
            }

            long level = levelZone.GetFinalUnitLevel(rand, spawn.X, spawn.Z, levelZone.Level, _mapProvider.GetMap().MaxLevel);


            Unit unit = new Unit(_repoService);
            unit.Level = level;
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

            IReadOnlyList<ElementType> etypes = _gameData.Get<ElementTypeSettings>(unit).GetData();

            spell.ElementTypeId = etypes[rand.Next() % etypes.Count].IdKey;
            spell.Id = HashUtils.NewGuid();

            SpellData spellData = unit.Get<SpellData>();
            spellData.Add(spell);

            unit.Name = _unitGenService.GenerateUnitName(rand, utype.IdKey, spawnZone.IdKey, null);

            _statService.CalcStats(unit, true);

            return unit;
        }
    }
}
