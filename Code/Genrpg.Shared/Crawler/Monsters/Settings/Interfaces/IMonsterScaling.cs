using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Monsters.Settings.Interfaces
{
    public interface IMonsterScaling
    {
        long MinLevel { get; set; }
        long MaxLevel { get; set; }
        double Chance { get; set; }
        double HpScale { get; set; }
        double DamScale { get; set; }
        List<MonsterAbility> Abilities { get; set; }
    }
}
