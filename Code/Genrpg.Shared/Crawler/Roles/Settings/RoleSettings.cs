using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Units.Entities;
using MessagePack;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Roles.Settings
{
    [MessagePackObject]
    public class AllowedWeapon
    {
        [Key(0)] public long ItemTypeId { get; set; }
    }


    [MessagePackObject]
    public class AllowedEquipSlot
    {
        [Key(0)] public long EquipSlotId { get; set; }
    }

    [MessagePackObject]
    public class RoleBonusBinary
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
    }

    [MessagePackObject]
    public class RoleBonusAmount
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public double Amount { get; set; }
    }

    [MessagePackObject]
    public class Role : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public string Abbrev { get; set; }

        [Key(8)] public long RoleCategoryId { get; set; }
        [Key(9)] public int HealthPerLevel { get; set; }
        [Key(10)] public int ManaPerLevel { get; set; }
        [Key(11)] public long MaxArmorScalingTypeId { get; set; }
        [Key(12)] public long CritPercent { get; set; } = 0; 
        [Key(13)] public long ManaStatTypeId { get; set; }
        [Key(14)] public bool Guardian { get; set; } = false;

        [Key(15)] public double TrainingXpScale { get; set; }
        [Key(16)] public double TrainingGoldScale { get; set; }
        [Key(17)] public string StartStatBonuses { get; set; }

        [Key(18)] public List<RoleBonusBinary> BinaryBonuses { get; set; } = new List<RoleBonusBinary>();

        [Key(19)] public List<RoleBonusAmount> AmountBonuses { get; set; } = new List<RoleBonusAmount>();
    }


    [MessagePackObject]
    public class RoleSettings : ParentSettings<Role>
    {
        [Key(0)] public override string Id { get; set; }

        public List<Role> GetRoles(List<UnitRole> unitRoles)
        {
            List<Role> roles = new List<Role>();

            foreach (UnitRole uc in unitRoles)
            {
                Role cl = Get(uc.RoleId);
                if (cl != null && !roles.Contains(cl))
                {
                    roles.Add(cl);
                }
            }
            return roles;
        }

        public bool HasBonus(List<UnitRole> roles, long entityTypeId, long entityId)
        {
            foreach (UnitRole uc in roles)
            {
                Role cl = Get(uc.RoleId);
                if (cl != null && cl.BinaryBonuses.Any(x => x.EntityTypeId == entityTypeId && x.EntityId == entityId))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// This is a function in case I want to make this more complex.
        /// </summary>
        /// <param name="scales"></param>
        /// <returns></returns>
        public double GetScalingBonusPerLevel(List<double> scales)
        {

            return scales.Sum(x => x);
        }
          
    }


    [MessagePackObject]
    public class RoleSettingsApi : ParentSettingsApi<RoleSettings, Role> { }
    [MessagePackObject]
    public class RoleSettingsLoader : ParentSettingsLoader<RoleSettings, Role> { }

    [MessagePackObject]
    public class RoleSettingsMapper : ParentSettingsMapper<RoleSettings, Role, RoleSettingsApi> { }

    public class RoleHelper : BaseEntityHelper<RoleSettings, Role>
    {
        public override long GetKey() { return EntityTypes.Role; }
    }

}
