using Genrpg.Shared.Core.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;
using System;
using System.Xml;

namespace Genrpg.Shared.BoardGame.Settings
{



    [MessagePackObject]
    public class ActivationSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public long StartTotalCharge { get; set; }
        [Key(2)] public long LandCharge { get; set; }
        [Key(3)] public long PassCharge { get; set; }
        [Key(4)] public long ChargePerLevel { get; set; }
        [Key(5)] public long MaxTotalCharge { get; set; }
        [Key(6)] public float ChargeDiceMultPower { get; set; } = 0.5f;
        [Key(7)] public float RewardDiceMultPower { get; set; } = 0.5f;

        public int GetMaxCharge(long level, double activationCostScale)
        {
            return (int)(Math.Min(MaxTotalCharge, StartTotalCharge + ChargePerLevel * (level - 1)) * activationCostScale);
        }
    }


    [MessagePackObject]
    public class ActivationSettingsLoader : NoChildSettingsLoader<ActivationSettings> { }



    [MessagePackObject]
    public class ActivationSettingsMapper : NoChildSettingsMapper<ActivationSettings> { }
}
