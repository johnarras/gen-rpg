using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Crawler.TimeOfDay.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.States.Constants;

namespace Genrpg.Shared.Crawler.Combat.Services
{
    public interface IProcessCombatRoundCombatService : IInjectable
    {
        Task<bool> ProcessCombatRound(PartyData partyData, CancellationToken token);
    }
    public class ProcessCombatRoundCombatService : IProcessCombatRoundCombatService
    {
        private ICrawlerStatService _statService;
        private ICrawlerSpellService _spellService;
        private ICrawlerCombatService _combatService;
        private ICrawlerService _crawlerService;
        protected IClientGameState _gs;
        protected IClientRandom _rand;
        private IGameData _gameData;
        private ITimeOfDayService _timeService;

        public async Task<bool> ProcessCombatRound(PartyData party, CancellationToken token)
        {
            if (party.Combat == null)
            {
                return false;
            }

            if (party.Combat.PartyGroup.CombatGroupAction == ECombatGroupActions.Run)
            {
                long totalLuck = party.Combat.PartyGroup.Units.Sum(x => x.Stats.Max(StatTypes.Luck));
                if (totalLuck > 0)
                {
                    double averageLuck = 1.0 * totalLuck / party.Combat.PartyGroup.Units.Count;

                    if (_rand.NextDouble() * party.Combat.Level < averageLuck)
                    {
                        party.Combat = null;
                        _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, token);
                        return true;
                    }
                }
            }

            _combatService.SetInitialActions(party);

            // First order things.

            _combatService.SetMonsterActions(party);
            if (party.Combat.PartyGroup.CombatGroupAction == ECombatGroupActions.Advance)
            {
                CrawlerSpell chargeSpell = _gameData.Get<CrawlerSpellSettings>(_gs.ch).GetData().FirstOrDefault(x => x.Name == "Charge");

                int advanceRange = CrawlerCombatConstants.RangeDelta;

                if (chargeSpell != null)
                {

                    RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);

                    int chargeCharacters = 0;

                    List<PartyMember> activeParty = party.GetActiveParty();

                    foreach (PartyMember member in activeParty)
                    {
                        if (roleSettings.HasBonus(member.Roles, EntityTypes.CrawlerSpell, chargeSpell.IdKey))
                        {
                            chargeCharacters++;
                            break;
                        }
                    }

                    advanceRange *= 1 + chargeCharacters;
                }

                foreach (CombatGroup group in party.Combat.Enemies)
                {
                    if (group.Range > CrawlerCombatConstants.MinRange)
                    {
                        // Yes this can compress groups that are really far away, feels more rewarding
                        // even if it's weird that spread out groups get piled on top of each other.
                        group.Range = Math.Max(CrawlerCombatConstants.MinRange, group.Range - advanceRange);
                    }
                }
                party.ActionPanel.AddText($"You Advance. {advanceRange}'.");
            }
            foreach (CombatGroup group in party.Combat.Enemies)
            {
                if (group.CombatGroupAction == ECombatGroupActions.Advance)
                {
                    if (group.Range > CrawlerCombatConstants.MinRange)
                    {
                        group.Range -= CrawlerCombatConstants.RangeDelta;
                    }
                    party.ActionPanel.AddText($"Group of {group.PluralName} Advances {CrawlerCombatConstants.MinRange}");
                    party.ActionPanel.AddText(group.ShowStatus());
                }
            }



            List<CrawlerUnit> allUnits = party.Combat.GetAllUnits();

            // Remove dead
            allUnits = allUnits.Where(x => !x.StatusEffects.HasBit(StatusEffects.Dead)).ToList();

            List<CrawlerUnit> attackSequence = SequenceUnitActions(allUnits);

            for (int s = 0; s < attackSequence.Count; s++)
            {
                CrawlerUnit unit = attackSequence[s];

                if (unit.Action == null || unit.Action.IsComplete)
                {
                    continue;
                }

                await _spellService.CastSpell(party, unit.Action, unit.Level);
            }

            await _combatService.EndCombatRound(party);
            return true;
        }

        private List<CrawlerUnit> SequenceUnitActions(List<CrawlerUnit> allUnits)
        {
            // Randomize order
            allUnits = allUnits.OrderBy(x => HashUtils.NewGuid()).ToList();

            // Descending by speed.
            allUnits = allUnits.OrderByDescending(x => x.Stats.Max(StatTypes.Speed)).ToList();

            List<CrawlerUnit> normalUnits = new List<CrawlerUnit>();
            List<CrawlerUnit> slowedUnits = new List<CrawlerUnit>();

            foreach (CrawlerUnit unit in allUnits)
            {
                if (unit.StatusEffects.HasBit(StatusEffects.Slowed))
                {
                    slowedUnits.Add(unit);
                }
                else
                {
                    normalUnits.Add(unit);
                }
            }

            List<CrawlerUnit> retval = new List<CrawlerUnit>();
            retval.AddRange(normalUnits);
            retval.AddRange(slowedUnits);

            return retval;
        }
    }
}
