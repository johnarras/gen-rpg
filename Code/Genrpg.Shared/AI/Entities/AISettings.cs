using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;

namespace Genrpg.Shared.AI.Entities
{
    [MessagePackObject]
    public class AISettings : BaseGameData
    {
        public override void Set(GameData gameData) { gameData.Set(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public float UpdateSeconds { get; set; } = 1.5f;

        [Key(2)] public float IdleWanderChance { get; set; } = 0.25f;

        [Key(3)] public float EnemyScanDistance { get; set; } = 20.0f;

        [Key(4)] public float LeashDistance { get; set; } = 60.0f;

        [Key(5)] public float BaseUnitSpeed { get; set; } = 5.0f;
    }
}
