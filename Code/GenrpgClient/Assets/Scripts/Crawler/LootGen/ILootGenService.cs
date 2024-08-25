using Assets.Scripts.Crawler.Services.Combat;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Loot.Services
{


    public interface ILootGenService : IInjectable
    {
        Item GenerateItem(ItemGenData lootGenData);
        PartyLoot GiveCombatLoot(PartyData party);
        PartyLoot GiveLoot(PartyData party, LootGenData genData);
        List<string> GenerateItemNames(IRandom rand, int itemCount);
    }
}
