using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Charms.PlayerData
{
    [MessagePackObject]
    public class PlayerCharmBonusList
    {
        [Key(0)] public long CharmUseId { get; set; }

        [Key(1)] public List<PlayerCharmBonus> Bonuses { get; set; } = new List<PlayerCharmBonus>();
        
    }
}
