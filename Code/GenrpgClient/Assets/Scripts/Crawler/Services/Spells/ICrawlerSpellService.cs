using Cysharp.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Spells.Services
{
    public interface ICrawlerSpellService : ISetupService
    {
        List<CrawlerSpell> GetSpellsForMember(GameState gs, PartyData party, PartyMember member, bool inCombat);
        List<CrawlerSpell> GetNonSpellCombatActionsForMember(GameState gs, PartyData party, PartyMember member, bool inCombat);
        FullSpell GetFullSpell (GameState gs, CrawlerUnit unit, CrawlerSpell spell, long overrideLevel = 0);
        UniTask CastSpell(GameState gs, PartyData party, UnitAction action, long overrideLevel = 0, int depth = 0);
    }
}
