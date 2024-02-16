using MessagePack;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Combat.Entities
{
    [MessagePackObject]
    public class CombatResults
    {
        [Key(0)] public CombatState StartState { get; set; }
        [Key(1)] public List<CombatUpdate> Updates { get; set; }
        [Key(2)] public CombatState EndState { get; set; }
    }
}
