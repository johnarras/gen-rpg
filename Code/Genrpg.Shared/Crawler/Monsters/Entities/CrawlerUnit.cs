using MessagePack;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Spells.Entities;

namespace Genrpg.Shared.Crawler.Monsters.Entities
{
    [MessagePackObject]
    public class CrawlerUnit : Unit
    {
        [Key(0)] public long UnitTypeId { get; set; }

        [Key(1)] public EDefendRanks DefendRank { get; set; }
        [Key(2)] public long HideExtraRange { get; set; }
        [Key(3)] public string PortraitName { get; set; }


        [Key(4)] public UnitAction Action { get; set; }

        public long GetAbilityLevel()
        {
            return Level + Stats.Curr(StatTypes.Leadership);
        }

        public virtual Item GetEquipmentInSlot(long equipSlotId)
        {
            return null;
        }
    }
}