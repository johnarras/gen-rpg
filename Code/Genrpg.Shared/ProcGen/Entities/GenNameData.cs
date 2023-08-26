using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class GenNameData
    {
        [Key(0)] public string QualityList { get; set; }
        [Key(1)] public string LevelList { get; set; }
        [Key(2)] public string Suffix { get; set; }
        [Key(3)] public int ItemCategoryId { get; set; }
        [Key(4)] public bool UseAAnSuffix { get; set; }
        [Key(5)] public int QualityUpgradeCost { get; set; }
        [Key(6)] public int MinLevelUpgradeCost { get; set; }
        [Key(7)] public int MaxLevelUpgradeCost { get; set; }
        [Key(8)] public bool GenNameIsSuffix { get; set; }
        [Key(9)] public int MaxNumtoUse { get; set; }
        [Key(10)] public bool AllItemsHaveAllLevels { get; set; }
        [Key(11)] public int CategorySpawnTableId { get; set; }

        public GenNameData()
        {
            QualityList = "";
            LevelList = "";
            Suffix = "";
            ItemCategoryId = 0;
            UseAAnSuffix = false;
            QualityUpgradeCost = 0;
            MinLevelUpgradeCost = 0;
            MaxLevelUpgradeCost = 0;
            GenNameIsSuffix = true;
            MaxNumtoUse = 0;
            AllItemsHaveAllLevels = false;
            CategorySpawnTableId = 0;
        }

    }
}
