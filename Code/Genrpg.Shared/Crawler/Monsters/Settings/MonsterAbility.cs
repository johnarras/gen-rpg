using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Monsters.Settings
{
    [MessagePackObject]
    public class MonsterAbility
    {
        [Key(0)] public long AbilityTypeId { get; set; }
    }
}
