using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.UserAbilities.Settings
{

    public class UserAbilityTypes
    {
        public const long None = 0;
        public const long MaxStorage = 1;
        public const long GuardTowers = 2;
        public const long WarbandSize = 3;
        public const long WarbandHealth = 4;
        public const long WarbandDamage = 5;
        public const long UpgradeQuantiy = 6;
        public const long RewardChance = 7;
        public const long RewardQuantity = 8;
        public const long WarbandDefendChance = 9;
        public const long WarbandHealing = 10;
        public const long MaxMana = 11;
        public const long SpellDamage = 12;
        public const long WarbandHitChance = 13;
    }


    [MessagePackObject]
    public class UserAbilitySettings : ParentSettings<UserAbilityType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long BaseUpgradeCost { get; set; } = 3;
        [Key(2)] public long LinearUpgradeCost { get; set; } = 5;
        [Key(3)] public long QuadraticUpgradeCost { get; set; } = 2;

        public long GetUpgradeCostForNextLevel(long nextLevel)
        {
            return BaseUpgradeCost + (nextLevel - 1) * (LinearUpgradeCost + (nextLevel - 1) * QuadraticUpgradeCost);
        }
    }

    [MessagePackObject]
    public class UserAbilityType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public long BaseQuantity { get; set; }
        [Key(8)] public long QuantityPerRank { get; set; }

    }

    [MessagePackObject]
    public class UserAbilitySettingsApi : ParentSettingsApi<UserAbilitySettings, UserAbilityType> { }

    [MessagePackObject]
    public class UserAbilitySettingsLoader : ParentSettingsLoader<UserAbilitySettings, UserAbilityType> { }

    [MessagePackObject]
    public class UserAbilitySettingsMapper : ParentSettingsMapper<UserAbilitySettings, UserAbilityType, UserAbilitySettingsApi> { }

}
