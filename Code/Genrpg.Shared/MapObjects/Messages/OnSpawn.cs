using MessagePack;

using System;
using System.Collections.Generic;

using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Interfaces;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Stats.Messages;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.MapObjects.Messages
{
    [MessagePackObject]
    public sealed class OnSpawn : BaseMapApiMessage, IMapSpawn
    {
        [Key(0)] public string ObjId { get; set; }
        [Key(1)] public DateTime LastSpawnTime { get; set; }
        [Key(2)] public long EntityTypeId { get; set; }
        [Key(3)] public long EntityId { get; set; }
        [Key(4)] public float X { get; set; }
        [Key(5)] public float Z { get; set; }
        [Key(6)] public long ZoneId { get; set; }
        [Key(7)] public string LocationId { get; set; }
        [Key(8)] public string LocationPlaceId { get; set; }
        [Key(9)] public int SpawnSeconds { get; set; }
        [Key(10)] public string Name { get; set; }
        [Key(11)] public float Y { get; set; }
        [Key(12)] public short Rot { get; set; }
        [Key(13)] public float Speed { get; set; }
        [Key(14)] public long FactionTypeId { get; set; }
        [Key(15)] public bool IsPlayer { get; set; }
        [Key(16)] public string TargetId { get; set; }
        [Key(17)] public int TempFlags { get; set; }
        [Key(18)] public long Level { get; set; }
        [Key(19)] public int OverrideZonePercent { get; set; }
        [Key(20)] public AttackerInfo FirstAttacker { get; set; }
        [Key(21)] public List<SpawnResult> Loot { get; set; }
        [Key(22)] public List<SpawnResult> SkillLoot { get; set; }
        [Key(23)] public List<DisplayEffect> Effects { get; set; }
        [Key(24)] public List<FullStat> Stats { get; set; }
        [Key(25)] public long AddonBits { get; set; }
        public long GetAddonBits() { return AddonBits; }
        
        // Do not send Addons on spawn.
        public List<IMapObjectAddon> GetAddons() { return new List<IMapObjectAddon>(); }

        public OnSpawn()
        {

        }

        public OnSpawn(MapObject wo)
        {
            CopyDataFromMapObjectToMapSpawn(wo);
        }

        private void CopyDataFromMapObjectToMapSpawn(IMapObject obj)
        {
            ObjId = obj.Id;

            Name = obj.Name;
            EntityTypeId = obj.EntityTypeId;
            EntityId = obj.EntityId;
            X = obj.X;
            Y = obj.Y;
            Z = obj.Z;
            Rot = (short)obj.Rot;
            Speed = obj.Speed;
            ZoneId = obj.ZoneId;
            LocationId = obj.LocationId;
            LocationPlaceId = obj.LocationPlaceId;
            AddonBits = obj.AddonBits;

            if (obj is Unit unit)
            {
                FactionTypeId = unit.FactionTypeId;
                Stats = unit.Stats.GetSnapshot();
                TargetId = unit.TargetId;
                FirstAttacker = unit.GetFirstAttacker();
                Loot = unit.Loot;
                SkillLoot = unit.SkillLoot;
                TempFlags = unit.GetFlags();

                Effects = new List<DisplayEffect>();
                foreach (IDisplayEffect eff in unit.Effects)
                {
                    Effects.Add(SerializationUtils.ConvertType<IDisplayEffect, DisplayEffect>(eff));
                }

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
