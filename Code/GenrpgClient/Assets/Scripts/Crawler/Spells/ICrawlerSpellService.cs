
using Assets.Scripts.Crawler.StateHelpers.Casting.SpecialMagicHelpers;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Genrpg.Shared.Crawler.Spells.Services
{
    public interface ICrawlerSpellService : IInjectable
    {
        List<CrawlerSpell> GetSpellsForMember(PartyData party, PartyMember member, bool inCombat);
        List<CrawlerSpell> GetNonSpellCombatActionsForMember(PartyData party, PartyMember member, bool inCombat);
        FullSpell GetFullSpell (CrawlerUnit unit, CrawlerSpell spell, long overrideLevel = 0);
        Awaitable CastSpell(PartyData party, UnitAction action, long overrideLevel = 0, int depth = 0);
        ISpecialMagicHelper GetSpecialEffectHelper(long effectEntityId);
        void RemoveSpellPowerCost(PartyData party, PartyMember member, CrawlerSpell spell);
        void SetupCombatData(PartyData partyData, PartyMember member);
        long GetPowerCost(PartyData party, PartyMember member, CrawlerSpell spell);
    }
}
