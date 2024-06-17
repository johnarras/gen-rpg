using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;
using System.Threading;

namespace Assets.Scripts.Crawler.Services.Combat
{
    public class CombatLoot
    {
        public long Gold { get; set; }
        public long Exp { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();
    }

    public interface ICombatService : IInitializable
    {
        void CheckForEncounter(bool atEndOfMove, CancellationToken token);
        bool StartCombat(PartyData partyData, CombatState combatState);
        void EndCombatRound(PartyData party);
        bool SetMonsterActions(PartyData party);
        bool ReadyForCombat(PartyData party);
        bool IsDisabled(CrawlerUnit unit);
        bool IsActionBlocked(CrawlerUnit unit, long combatActionId);
        bool IsActionWeak(CrawlerUnit unit, long combatActionId);
        List<UnitAction> GetActionsForPlayer(PartyData party, CrawlerUnit unit);
        UnitAction GetActionFromSpell(PartyData party, CrawlerUnit unit, CrawlerSpell spell,
            List<UnitAction> currentActions = null);
        void SetInitialActions(PartyData party);
        void AddCombatUnits(PartyData partyData, UnitType unitType, long unitQuantity, long factionTypeId,
            int currRange = CrawlerCombatConstants.MinRange);
    }
}
