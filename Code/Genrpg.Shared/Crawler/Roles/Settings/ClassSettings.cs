using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
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
        [Key(10)] public int HealthPerLevel { get; set; }
        [Key(11)] public int ManaPerLevel { get; set; }

        [Key(12)] public long BuffStatTypeId { get; set; }
        [Key(13)] public long BuffStatPercent { get; set; } = 100;
        [Key(14)] public long DefaultLevelPercentBuff { get; set; } = 50;
        [Key(15)] public long PartyBuffId { get; set; }
        [Key(16)] public long MaxArmorScalingTypeId { get; set; }

        [Key(17)] public List<AllowedWeapon> AllowedWeapons { get; set; } = new List<AllowedWeapon>();
        [Key(18)] public List<AllowedEquipSlot> AllowedEquipSlots { get; set; } = new List<AllowedEquipSlot>();

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
    public class ClassSettingsLoader : ParentSettingsLoader<ClassSettings, Class, ClassSettingsApi> { }



}
