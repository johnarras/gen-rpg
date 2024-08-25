using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
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
    public class RoleBonus
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
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
        [Key(9)] public double MeleeScaling { get; set; }
        [Key(10)] public double RangedScaling { get; set; }
        [Key(11)] public double SpellDamScaling { get; set; }
        [Key(12)] public double HealingScaling { get; set; }
        [Key(13)] public double SummonScaling { get; set; }
        [Key(14)] public int HealthPerLevel { get; set; }
        [Key(15)] public int ManaPerLevel { get; set; }
        [Key(16)] public long MaxArmorScalingTypeId { get; set; }
        [Key(17)] public long CritPercent { get; set; } = 0; 
        [Key(18)] public long ManaStatTypeId { get; set; }

        [Key(19)] public List<RoleBonus> Bonuses { get; set; } = new List<RoleBonus>();
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
                if (cl != null && cl.Bonuses.Any(x => x.EntityTypeId == entityTypeId && x.EntityId == entityId))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Take product of scaling bonuses then subtract 1 to get scaling bonus per level.
        /// </summary>
        /// <param name="scales"></param>
        /// <returns></returns>
        public double GetScalingBonusPerLevel(List<double> scales)
        {
            double retval = 1;

            foreach (double scale in scales)
            {
                retval *= (1 + scale);
            }
            return retval - 1;
        }
          
    }

    

    [MessagePackObject]
    public class RoleSettingsApi : ParentSettingsApi<RoleSettings, Role> { }
    [MessagePackObject]
    public class RoleSettingsLoader : ParentSettingsLoader<RoleSettings, Role> { }



    [MessagePackObject]
    public class RoleSettingsMapper : ParentSettingsMapper<RoleSettings, Role, RoleSettingsApi> { }





}
