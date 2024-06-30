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
    public class ClassBonus
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public long Quantity { get; set; }
    }

    [MessagePackObject]
    public class Class : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public string Abbrev { get; set; }

        [Key(8)] public int LevelsPerMelee { get; set; }
        [Key(9)] public int LevelsPerRanged { get; set; }
        [Key(10)] public int LevelsPerDamage { get; set; }
        [Key(11)] public int LevelsPerHeal { get; set; }
        [Key(12)] public int LevelsPerSummon { get; set; }
        [Key(13)] public int HealthPerLevel { get; set; }
        [Key(14)] public int ManaPerLevel { get; set; }
        [Key(15)] public long MaxArmorScalingTypeId { get; set; }
        [Key(16)] public long CritPercent { get; set; } = 0; 
        [Key(17)] public long ManaStatTypeId { get; set; }

        [Key(18)] public List<ClassBonus> Bonuses { get; set; } = new List<ClassBonus>();

        [Key(19)] public List<AllowedWeapon> AllowedWeapons { get; set; } = new List<AllowedWeapon>();

    }


    [MessagePackObject]
    public class ClassSettings : ParentSettings<Class>
    {
        [Key(0)] public override string Id { get; set; }

        public List<Class> GetClasses(List<UnitClass> unitClasses)
        {
            List<Class> classes = new List<Class>();

            foreach (UnitClass uc in unitClasses)
            {
                Class cl = Get(uc.ClassId);
                if (cl != null && !classes.Contains(cl))
                {
                    classes.Add(cl);
                }
            }
            return classes;
        }
    }

    [MessagePackObject]
    public class ClassSettingsApi : ParentSettingsApi<ClassSettings, Class> { }
    [MessagePackObject]
    public class ClassSettingsLoader : ParentSettingsLoader<ClassSettings, Class> { }



    [MessagePackObject]
    public class ClassSettingsMapper : ParentSettingsMapper<ClassSettings, Class, ClassSettingsApi> { }





}
