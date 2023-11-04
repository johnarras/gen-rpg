using MessagePack;

using System;
using System.Collections.Generic;
using Genrpg.Shared.Characters.Entities;

using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Interfaces;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Entities.Constants;

namespace Genrpg.Shared.MapObjects.Messages
{
    [MessagePackObject]
    public sealed class OnSpawn : BaseMapApiMessage, IMapSpawn
    {
        [Key(0)] public string MapObjectId { get; set; }
        [Key(1)] public DateTime LastSpawnTime { get; set; }
        [Key(2)] public long EntityTypeId { get; set; }
        [Key(3)] public long EntityId { get; set; }
        [Key(4)] public float X { get; set; }
        [Key(5)] public float Z { get; set; }
        [Key(6)] public long ZoneId { get; set; }
        [Key(7)] public int SpawnSeconds { get; set; }
        [Key(8)] public string Name { get; set; }
        [Key(9)] public float Y { get; set; }
        [Key(10)] public float Rot { get; set; }
        [Key(11)] public float Speed { get; set; }
        [Key(12)] public long NPCTypeId { get; set; }
        [Key(13)] public long FactionTypeId { get; set; }
        [Key(14)] public StatGroup Stats { get; set; }
        [Key(15)] public bool IsPlayer { get; set; }
        [Key(16)] public string TargetId { get; set; }
        [Key(17)] public int TempFlags { get; set; }
        [Key(18)] public long Level { get; set; }
        [Key(19)] public int OverrideZonePercent { get; set; }
        [Key(20)] public AttackerInfo FirstAttacker { get; set; }
        [Key(21)] public List<SpawnResult> Loot { get; set; }
        [Key(22)] public List<SpawnResult> SkillLoot { get; set; }
        [Key(23)] public List<ActiveSpellEffect> Effects { get; set; }

        public OnSpawn()
        {

        }

        public OnSpawn(MapObject wo)
        {
            CopyDataFrom(wo);
        }

        private void CopyDataFrom(IMapObject obj)
        {
            MapObjectId = obj.Id;
            Name = obj.Name;
            EntityTypeId = obj.EntityTypeId;
            EntityId = obj.EntityId;
            X = obj.X;
            Y = obj.Y;
            Z = obj.Z;
            Rot = obj.Rot;
            Speed = obj.Speed;
            ZoneId = obj.ZoneId;

            if (obj is Unit unit)
            {
                if (unit.NPCTypeId > 0)
                {
                    NPCTypeId = unit.NPCTypeId;
                }
                FactionTypeId = unit.FactionTypeId;
                Stats = unit.Stats;
                TargetId = unit.TargetId;
                FirstAttacker = unit.GetFirstAttacker();
                Loot = unit.Loot;
                SkillLoot = unit.SkillLoot;
                TempFlags = unit.Flags;
                Effects = unit.SpellEffects;
                Level = unit.Level;
                if (obj is Character ch)
                {
                    IsPlayer = true;
                    Name = ch.Name;
                    EntityTypeId = EntityTypes.ProxyCharacter;
                }
            }
        }

        public string GetName()
        {
            return Name;
        }

        public bool IsDirty()
        {
            return false;
        }

        public void SetDirty(bool val)
        {

        }
    }
}
