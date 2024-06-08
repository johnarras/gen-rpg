using Assets.Scripts.Crawler.Services.Combat;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;

namespace Genrpg.Shared.Crawler.Loot.Services
{
    public interface ILootGenService : IInitializable
    {
        Item GenerateItem(LootGenData lootGenData);
        CombatLoot GiveLoot(PartyData party);
    }
}
