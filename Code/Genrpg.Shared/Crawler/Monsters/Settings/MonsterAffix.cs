using Genrpg.Shared.Crawler.Monsters.Constants;
using Genrpg.Shared.Crawler.Monsters.Settings.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Monsters.Settings
{

    [MessagePackObject]
    public class MonsterAffix : IMonsterScaling
    {
        [Key(0)] public string Name { get; set; }

        [Key(1)] public double Weight { get; set; } = 1.0f;

        [Key(2)] public long MinLevel { get; set; } = 0;
        [Key(3)] public long MaxLevel { get; set; } = MonsterConstants.MaxLevel;
        [Key(4)] public double Chance { get; set; } = 1.0;
        [Key(5)] public double HpScale { get; set; } = 1.0;
        [Key(6)] public double DamScale { get; set; } = 1.0;
        [Key(7)] public List<MonsterAbility> Abilities { get; set; } = new List<MonsterAbility>();
    }

}
