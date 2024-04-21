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
        bool StartCombat(GameState gs, PartyData partyData, CombatState combatState);
        void EndCombatRound(GameState gs, PartyData party);
        bool SetMonsterActions(GameState gs, PartyData party);
        bool ReadyForCombat(GameState gs, PartyData party);
        List<UnitAction> GetActionsForPlayer(GameState gs, PartyData party, CrawlerUnit unit);
        UnitAction GetActionFromSpell(GameState gs, PartyData party, CrawlerUnit unit, CrawlerSpell spell,
            List<UnitAction> currentActions = null);
        void SetInitialActions(GameState gs, PartyData party);
        void AddCombatUnits(GameState gs, PartyData partyData, UnitType unitType, long unitQuantity, long factionTypeId,
            int currRange = CrawlerCombatConstants.MinRange);
    }
}
