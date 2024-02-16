using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Monsters.Settings.Interfaces
{
    public interface IMonsterInfo : IMonsterScaling
    {
        double PrefixChance { get; set; }
        double SuffixChance { get; set; }
        List<MonsterAffix> Prefixes { get; set; }
        List<MonsterAffix> Suffixes { get; set; }
    }
}
