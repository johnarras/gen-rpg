using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loading;

namespace Genrpg.Shared.AI.Entities
{
    [MessagePackObject]
    public class AISettings : BaseGameSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public float UpdateSeconds { get; set; } = 1.5f;

        [Key(2)] public float IdleWanderChance { get; set; } = 0.25f;

        [Key(3)] public float EnemyScanDistance { get; set; } = 20.0f;

        [Key(4)] public float LeashDistance { get; set; } = 60.0f;

        [Key(5)] public float BaseUnitSpeed { get; set; } = 5.0f;

        [Key(6)] public float BringAFriendRadius { get; set; } = 20.0f;
    }


    [MessagePackObject]
    public class AISettingsLoader : GameDataLoader<AISettings>
    {
    }
}
