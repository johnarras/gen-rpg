﻿using Assets.Scripts.UI.Crawler.States;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Assertions.Must;

namespace Assets.Scripts.Crawler.Services.Combat
{
    public class ProcessCombatRoundCombatService : IProcessCombatRoundCombatService
    {
        private ICrawlerStatService _statService;
        private ICrawlerSpellService _spellService;
        private ICombatService _combatService;
        private ICrawlerService _crawlerService;

        public async Task Setup(GameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public async UniTask<bool> ProcessCombatRound(GameState gs, PartyData party, CancellationToken token)
        {
            if (party.Combat == null)
            {
                return false;
            }

            if (party.Combat.Allies[0].CombatGroupAction == ECombatGroupActions.Run)
            {
                long totalLuck = party.Combat.Allies[0].Units.Sum(x => x.Stats.Get(StatTypes.Luck, StatCategories.Curr));
                if (totalLuck > 0)
                {
                    double averageLuck = 1.0*totalLuck/party.Combat.Allies[0].Units.Count;

                    if (gs.rand.NextDouble()*party.Combat.Level < averageLuck)
                    {
                        _crawlerService.ChangeState(ECrawlerStates.TavernMain, token);
                        return true;
                    }
                }
            }

            _combatService.SetInitialActions(gs, party);

            // First order things.

            _combatService.SetMonsterActions(gs, party);

            foreach (CombatGroup group in party.Combat.Enemies)
            {
                if (group.CombatGroupAction == ECombatGroupActions.Advance)
                {
                    if (group.Range > CrawlerCombatConstants.MinRange)
                    {
                        group.Range -= CrawlerCombatConstants.RangeDelta;
                    }
                    party.ActionPanel.AddText("Group of " + group.PluralName + " Advances");
                    party.ActionPanel.AddText(group.ShowStatus());
                }
            }

            if (party.Combat.Allies[0].CombatGroupAction == ECombatGroupActions.Advance)
            {
                foreach (CombatGroup group in party.Combat.Enemies)
                {
                    if (group.Range > CrawlerCombatConstants.MinRange)
                    {
                        group.Range -= CrawlerCombatConstants.RangeDelta;
                    }
                }
                party.ActionPanel.AddText("You Advance.");
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

                await _spellService.CastSpell(gs, party, unit.Action, unit.Level);

            }

            _combatService.EndCombatRound(gs, party);

            await Task.CompletedTask;
            return true;
        }

        private List<CrawlerUnit> SequenceUnitActions(List<CrawlerUnit> allUnits)
        {
            // Randomize order
            allUnits = allUnits.OrderBy(x => HashUtils.NewGuid()).ToList();

            // Descending by speed.
            allUnits = allUnits.OrderByDescending(x => x.Stats.Max(StatTypes.Speed)).ToList();

            List<CrawlerUnit> hastedUnits = new List<CrawlerUnit>();
            List<CrawlerUnit> normalUnits = new List<CrawlerUnit>();
            List<CrawlerUnit> slowedUnits = new List<CrawlerUnit>();

            foreach (CrawlerUnit unit in allUnits)
            {
                if (unit.StatusEffects.HasBit(StatusEffects.Hasted))
                {
                    if (unit.StatusEffects.HasBit(StatusEffects.Slowed))
                    {
                        normalUnits.Add(unit);
                    }
                    else
                    {
                        hastedUnits.Add(unit);
                    }
                }
                else
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
            }

            List<CrawlerUnit> retval = new List<CrawlerUnit>();
            retval.AddRange(hastedUnits);
            retval.AddRange(normalUnits);
            retval.AddRange(slowedUnits);

            return retval;
        }
    }
}