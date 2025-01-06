using MessagePack;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Spells.Entities;
using Newtonsoft.Json;
using Genrpg.Shared.DataStores.Entities;

namespace Genrpg.Shared.Crawler.Monsters.Entities
{
    [MessagePackObject]
    public class CrawlerUnit : Unit
    {
        [Key(0)] public long UnitTypeId { get; set; }

        [Key(1)] public EDefendRanks DefendRank { get; set; }
        [Key(2)] public long HideExtraRange { get; set; }
        [Key(3)] public string PortraitName { get; set; }


        [JsonIgnore]
        [Key(4)] public UnitAction Action { get; set; }

        [Key(5)] public string CombatGroupId { get; set; }

        [Key(6)] public bool IsGuardian { get; set; }

        [Key(7)] public long VulnBits { get; set; }

        [Key(8)] public long ResistBits { get; set; }

        public CrawlerUnit(IRepositoryService repositoryService) : base(repositoryService) { }


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
