﻿using ClientEvents;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Combat.Utils;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Constants;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.Spells.Utils;
using Genrpg.Shared.Crawler.Stats.Utils;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.Settings;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Inventory.Settings.LootRanks;
using Genrpg.Shared.Spells.Casting;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Spells.Procs.Entities;
using Genrpg.Shared.Spells.Procs.Interfaces;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Spells.Settings.Spells;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Genrpg.Shared.Crawler.Spells.Services
{
    public class CrawlerSpellService : ICrawlerSpellService
    {

        private IStatService _statService;      

        public async Task Setup(GameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public List<CrawlerSpell> GetNonSpellCombatActionsForMember(GameState gs, 
            PartyData party, PartyMember member, bool inCombat)
        {
            return GetAbilitiesForMember(gs, party, member, inCombat, false);
        }

        public List<CrawlerSpell> GetSpellsForMember(GameState gs, PartyData party, 
            PartyMember member, bool inCombat)
        {
            return GetAbilitiesForMember(gs, party, member, inCombat, true);
        }

        private List<CrawlerSpell> GetAbilitiesForMember(GameState gs, PartyData party, 
            PartyMember member, bool inCombat, bool chooseSpells)
        { 

            IReadOnlyList<CrawlerSpell> allSpells = gs.data.Get<CrawlerSpellSettings>(null).GetData();

            List<CrawlerSpell> castSpells = allSpells.Where(x => 
            (x.CombatActionId == CombatActions.Cast) == chooseSpells).ToList();

            List<CrawlerSpell> okSpells = new List<CrawlerSpell>();

            List<Class> classes = gs.data.Get<ClassSettings>(null).GetClasses(member.Classes);

            List<long> classIds = classes.Select(x => x.IdKey).ToList();
            classIds.Add(0);

            foreach (CrawlerSpell spell in castSpells)
            {
                if (spell.IdKey < 1)
                {
                    continue;
                }

                if (spell.Level > member.Level)
                {
                    continue;
                }

                if (spell.RequiredStatusEffectId > 0 && !member.StatusEffects.HasBit(spell.RequiredStatusEffectId))
                {
                    continue;
                }

                if (!classIds.Contains(spell.ClassId))
                {
                    continue;
                }

                if (!inCombat)
                {
                    if (CrawlerSpellUtils.IsEnemyTarget(spell.TargetTypeId))
                    {
                        continue;
                    }
                }
                else
                {
                    if (CrawlerSpellUtils.IsNonCombatTarget(spell.TargetTypeId))
                    {
                        continue;
                    }
                }
                okSpells.Add(spell);
            }

            List<CrawlerSpell> dupeList = new List<CrawlerSpell>(okSpells);

            foreach (CrawlerSpell dupeSpell in dupeList)
            {
                if (dupeSpell.ReplacesCrawlerSpellId > 0)
                {
                    CrawlerSpell removeSpell = okSpells.FirstOrDefault(x => x.IdKey == dupeSpell.ReplacesCrawlerSpellId);

                    if (removeSpell != null)
                    {
                        okSpells.Remove(removeSpell);
                    }
                }
            }

            okSpells = okSpells.OrderBy(x => x.Name).ToList();
            return okSpells;
        }

        // Figure out what this unit's combat hit will look like.
        public FullSpell GetFullSpell(GameState gs, CrawlerUnit caster, CrawlerSpell spell, long overrideLevel = 0)
        {
            FullSpell fullSpell = new FullSpell() { Spell = spell };

            List<Class> classes = new List<Class>();

            CrawlerCombatSettings combatSettings = gs.data.Get<CrawlerCombatSettings>(null);

            long critChance = 0;

            if (caster is PartyMember member)
            {
                long critStat = caster.Stats.Curr(StatTypes.Crit);
                critChance = (long)(combatSettings.CritPercentWithStatAtLevel * critStat / caster.Level);
                classes = gs.data.Get<ClassSettings>(null).GetClasses(member.Classes);

            }

            List<long> actionTypesWithProcsSet = new List<long>();

            long attackQuantity = 0;
            long abilityLevel = (overrideLevel < 1 ? caster.GetAbilityLevel() : overrideLevel);
            fullSpell.Level = abilityLevel;

            ElementTypeSettings elemSettings = gs.data.Get<ElementTypeSettings>(null);

            // Make full effect list to let us weave procs into the combined spell's effects.
            List<FullEffect> startFullEffectList = new List<FullEffect>();

            foreach (CrawlerSpellEffect effect in spell.Effects)
            {
                startFullEffectList.Add(new FullEffect() { Effect = effect, PercentChance = 100, InitialEffect = true });
            }

            List<FullEffect> endFullEffectList = new List<FullEffect>();

            foreach (FullEffect fullEffect in startFullEffectList)
            {
                endFullEffectList.Add(fullEffect);

                ElementType etype = elemSettings.Get(fullEffect.Effect.ElementTypeId);

                if (etype != null && etype.Procs != null)
                {
                    foreach (SpellProc proc in etype.Procs)
                    {
                        endFullEffectList.Add(CreateFullEffectFromProc(proc));
                    }
                }

                if (actionTypesWithProcsSet.Contains(fullEffect.Effect.EntityTypeId))
                {
                    continue;
                }

                actionTypesWithProcsSet.Add(fullEffect.Effect.EntityTypeId);

                List<List<IProc>> allProcLists = new List<List<IProc>>();

                if (fullEffect.Effect.EntityTypeId == EntityTypes.Attack)
                {
                    allProcLists.Add(GetProcsFromSlot(gs, caster, EquipSlots.PoisonVial));
                    allProcLists.Add(GetProcsFromSlot(gs, caster, EquipSlots.Quiver));
                    
                }
                else if (fullEffect.Effect.EntityTypeId == EntityTypes.Shoot)
                {
                    allProcLists.Add(GetProcsFromSlot(gs, caster, EquipSlots.Quiver));
                    allProcLists.Add(GetProcsFromSlot(gs, caster, EquipSlots.PoisonVial));
                }

                foreach (List<IProc> procList in allProcLists)
                {
                    if (procList.Count < 1)
                    {
                        continue;
                    }

                    foreach (IProc proc in procList)
                    {
                        endFullEffectList.Add(CreateFullEffectFromProc(proc));
                    }
                }
            }

            foreach (FullEffect fullEffect in endFullEffectList)
            {
                CrawlerSpellEffect effect = fullEffect.Effect;
                ElementType elemType = elemSettings.Get(effect.ElementTypeId);
                if (elemType == null)
                {
                    elemType = elemSettings.Get(ElementTypes.Physical);
                }
                OneEffect oneEffect = new OneEffect();

                fullEffect.Hit = oneEffect;
                fullEffect.ElementType = elemType;
                fullSpell.Effects.Add(fullEffect);

                oneEffect.MinQuantity = CrawlerCombatConstants.BaseMinDamage;
                oneEffect.MaxQuantity = CrawlerCombatConstants.BaseMaxDamage;
                

                long equipSlotToCheck = 0;
                long statUsedForScaling = 0;
                List<int> levelsPerAttack = new List<int>();
                if (fullEffect.InitialEffect)
                {
                    oneEffect.CritChance = critChance + spell.CritChance;
                }
                if (effect.EntityTypeId == EntityTypes.Attack)
                {
                    equipSlotToCheck = EquipSlots.MainHand;
                    statUsedForScaling = StatTypes.Strength;

                    levelsPerAttack = classes.Select(x => x.LevelsPerMelee).ToList();
                    oneEffect.HitType = EHitTypes.Melee;
                }
                else if (effect.EntityTypeId == EntityTypes.Shoot)
                {
                    oneEffect.HitType = EHitTypes.Ranged;
                    equipSlotToCheck = EquipSlots.Ranged;
                    statUsedForScaling = StatTypes.Strength;
                    levelsPerAttack = classes.Select(x => x.LevelsPerRanged).ToList();
                }
                else
                {
                    oneEffect.MinQuantity = effect.MinQuantity;
                    oneEffect.MaxQuantity = effect.MaxQuantity;

                    oneEffect.HitType = EHitTypes.Spell;
                    oneEffect.CritChance = 0;
                    if (attackQuantity == 0)
                    {
                        attackQuantity = abilityLevel;
                    }
                    continue;
                }

                Item weapon = caster.GetEquipmentInSlot(equipSlotToCheck);
                if (weapon != null)
                {
                    ItemType itype = gs.data.Get<ItemTypeSettings>(null).Get(weapon.ItemTypeId);

                    oneEffect.MinQuantity = itype.MinVal;
                    oneEffect.MaxQuantity = itype.MaxVal;

                    LootRank lootRank = gs.data.Get<LootRankSettings>(null).Get(weapon.LootRankId);

                    if (lootRank != null)
                    {
                        oneEffect.MinQuantity += lootRank.Damage;
                        oneEffect.MaxQuantity += lootRank.Damage;
                    }
                }
                else if (effect.EntityTypeId == EntityTypes.Attack && caster is Monster monster)
                {
                    oneEffect.MinQuantity = monster.MinDam;
                    oneEffect.MaxQuantity = monster.MaxDam;
                }

                oneEffect.MinQuantity += CrawlerStatUtils.GetStatBonus(caster, statUsedForScaling);
                oneEffect.MaxQuantity += CrawlerStatUtils.GetStatBonus(caster, statUsedForScaling);

                oneEffect.MinQuantity = Math.Max(oneEffect.MinQuantity, CrawlerCombatConstants.BaseMinDamage);
                oneEffect.MaxQuantity = Math.Max(oneEffect.MaxQuantity, CrawlerCombatConstants.BaseMaxDamage);

                if (effect.MinQuantity == 0 && effect.MaxQuantity == 0 && fullEffect.InitialEffect)
                {
                    long tempAttackQuantity = CrawlerCombatConstants.BaseAttackQuantity;

                    if (levelsPerAttack.Count > 0)
                    {
                        double attacksPerLevel = 0;

                        foreach (long levels in levelsPerAttack)
                        {
                            if (levels > 0)
                            {
                                attacksPerLevel += 1.0 / levels;
                            }
                        }
                        

                        tempAttackQuantity = 1 + (int)Math.Floor(attacksPerLevel * abilityLevel);

                        if (tempAttackQuantity > 0 &&
                            (tempAttackQuantity < attackQuantity ||
                            attackQuantity == 0))
                        {
                            attackQuantity = tempAttackQuantity;
                        }
                    }
                }
                else
                {
                    attackQuantity = MathUtils.LongRange(effect.MinQuantity, effect.MaxQuantity, gs.rand);
                }
            }

            fullSpell.HitQuantity = Math.Max(1, attackQuantity);
            fullSpell.HitsLeft = fullSpell.HitQuantity;
            return fullSpell;
        }

        private List<IProc> GetProcsFromSlot(GameState gs, CrawlerUnit member, long equipSlotId)
        {
            Item item = member.GetEquipmentInSlot(equipSlotId);

            if (item == null || item.Procs == null || item.Procs.Count < 1)
            {
                return new List<IProc>();
            }

            return new List<IProc>(item.Procs);
        }

        private FullEffect CreateFullEffectFromProc(IProc proc)
        {

            CrawlerSpellEffect procEffect = new CrawlerSpellEffect()
            {
                EntityTypeId = proc.EntityTypeId,
                EntityId = proc.EntityId,
                ElementTypeId = proc.ElementTypeId,
                MinQuantity = proc.MinQuantity,
                MaxQuantity = proc.MaxQuantity,
            };
            FullEffect fullProcEffect = new FullEffect()
            {
                PercentChance = proc.PercentChance,
                Effect = procEffect,
            };
            return fullProcEffect;
        }

        public async UniTask CastSpell(GameState gs, PartyData party, UnitAction action, long overrideLevel = 0, int depth = 0)
        {
            try
            {
                float delay = 0.2f;

                action.IsComplete = true;
                if (action.Spell == null)
                {
                    return;
                }

                if (!CombatUtils.CanPerformAction(action.Caster))
                {
                    return;
                }

                if (action.Spell.CombatActionId == CombatActions.Cast && 
                    action.Caster.StatusEffects.HasBit(StatusEffects.Silenced))
                {
                    await ShowText(gs, party, $"{action.Caster.Name} is Silenced!", delay);
                    return;
                }

                if (action.Spell.CombatActionId == CombatActions.Attack &&
                    action.Caster.StatusEffects.HasBit(StatusEffects.Rooted))
                {
                    await ShowText(gs, party, $"{action.Caster.Name} is Rooted!", delay);
                    return;
                }

                FullSpell fullSpell = GetFullSpell(gs, action.Caster, action.Spell, overrideLevel);
                
                bool foundOkTarget = false;
                if (!CrawlerSpellUtils.IsNonCombatTarget(fullSpell.Spell.TargetTypeId))
                {
                    foreach (CrawlerUnit unit in action.FinalTargets)
                    {
                        if (unit.StatusEffects.HasBit(StatusEffects.Dead))
                        {
                            continue;
                        }
                        foundOkTarget = true;
                        break;
                    }     
                }

                if (!foundOkTarget)
                {
                    return;
                }

                if (!fullSpell.Spell.SuppressCastText)
                {
                    await ShowText(gs, party, $"{action.Caster.Name} casts {fullSpell.Spell.Name}", delay);
                }

                if (action.Caster is PartyMember pmember)
                {
                    long powerCost = action.Spell.GetPowerCost(pmember.Level);

                    if (powerCost > 0)
                    {
                        long currMana = pmember.Stats.Curr(StatTypes.Mana);
                        _statService.Add(gs, pmember, StatTypes.Mana, StatCategories.Curr, -Math.Min(powerCost, currMana));
                        party.StatusPanel.RefreshUnit(pmember);
                    }
                }

                if (party.Combat != null && !string.IsNullOrEmpty(action.Caster.PortraitName))
                {
                    party.WorldPanel.SetPicture(action.Caster.PortraitName);
                }

                foreach (CrawlerUnit target in action.FinalTargets)
                {
                    await CastSpellOnUnit(gs, party, action.Caster, fullSpell, target, 0, delay);

                }
            }
            catch (Exception e)
            {
                gs.logger.Exception(e, "CastSpell");
            }
        }

        private async UniTask ShowText(GameState gs, PartyData party, string text, float delay = 0.0f)
        {
            party.ActionPanel.AddText(text);

            DateTime startTime = DateTime.UtcNow;

            while ((DateTime.UtcNow-startTime).TotalSeconds < delay)
            {
                await Task.Delay(100);

                if (party.SpeedupListener.TriggerSpeedupNow())
                {
                    break;
                }
            }
        }

        private void AddToActionDict(Dictionary<string, ActionListItem> dict, string actionName, long quantity)
        {
            if (string.IsNullOrEmpty(actionName))
            {
                return;
            }

            if (!dict.ContainsKey(actionName))
            {
                dict[actionName] = new ActionListItem();
            }

            dict[actionName].TotalQuantity += quantity;
            dict[actionName].TotalHits++;
        }

        internal class ActionListItem
        {
            public long TotalQuantity { get; set; }
            public long TotalHits { get; set; }
        }

        public async UniTask CastSpellOnUnit(GameState gs, PartyData party, CrawlerUnit caster, FullSpell spell, CrawlerUnit target,
           long overrideLevel = 0, float delay = 0.5f)
        {
            bool isSingleTarget = true;
            if (spell.Spell.TargetTypeId == TargetTypes.EnemyGroup || 
                spell.Spell.TargetTypeId == TargetTypes.Party ||
                spell.Spell.TargetTypeId == TargetTypes.AllEnemies)
            {
                spell.HitsLeft = 1;
                isSingleTarget = false;
            }

            bool isEnemyTarget = CrawlerSpellUtils.IsEnemyTarget(spell.Spell.TargetTypeId);

            if (isEnemyTarget && target.StatusEffects.HasBit(StatusEffects.Dead))
            {
                return;
            }


            CrawlerCombatSettings combatSettings = gs.data.Get<CrawlerCombatSettings>(null);

            long currHealth = target.Stats.Curr(StatTypes.Health);
            long maxHealth = target.Stats.Max(StatTypes.Health);

            long maxDamage = currHealth;
            long maxHealing = maxHealth - currHealth;

            bool haveDamageOrHealing = spell.Effects.Any(x => 
            x.Effect.EntityTypeId == EntityTypes.Damage || 
            x.Effect.EntityTypeId == EntityTypes.Healing ||
            x.Effect.EntityTypeId == EntityTypes.Attack ||
            x.Effect.EntityTypeId == EntityTypes.Shoot);
            
            if (!haveDamageOrHealing)
            {
                spell.HitsLeft = 1;
            }
            
            long totalDamage = 0;
            long totalHealing = 0;


            int currHitTimes = 0;
            long newQuantity = 0;
            string fullAction = null;

            long casterHit = caster.Stats.Curr(StatTypes.Hit);
            CrawlerUnit healTarget = target;

            bool casterIsWeak = caster.StatusEffects.HasBit(StatusEffects.Weak);
            bool casterIsFeebleMind = caster.StatusEffects.HasBit(StatusEffects.FeebleMind);

            Dictionary<string,ActionListItem> actionList = new Dictionary<string,ActionListItem>();

            while (spell.HitsLeft > 0)
            {
                bool didKill = false;
                foreach (FullEffect fullEffect in spell.Effects)
                {
                    if (didKill)
                    {
                        break;
                    }

                    newQuantity = 0;
                    fullAction = null;
                    healTarget = target;
                    CrawlerSpellEffect effect = fullEffect.Effect;
                    OneEffect hit = fullEffect.Hit;

                    if (gs.rand.NextDouble()*100 > fullEffect.PercentChance)
                    {
                        continue;
                    }

                    if (effect.EntityTypeId == EntityTypes.Attack ||
                        effect.EntityTypeId == EntityTypes.Shoot ||
                        effect.EntityTypeId == EntityTypes.Damage)
                    {
                        if (target.StatusEffects.HasBit(StatusEffects.Dead))
                        {
                            continue;
                        }

                        if (target.DefendRank == EDefendRanks.None && hit.CritChance > 0 &&
                            gs.rand.NextDouble() * 100 < hit.CritChance)
                        {
                            newQuantity = target.Stats.Curr(StatTypes.Health);
                            AddToActionDict(actionList, "CRITS!", newQuantity);
                            didKill = true;
                        }
                        else
                        {
                            long defenseStatId = StatTypes.Armor;
                            long mult = (isSingleTarget || !fullEffect.InitialEffect ? 1 : spell.Level);
                            newQuantity = MathUtils.LongRange(hit.MinQuantity * mult, hit.MaxQuantity * mult, gs.rand);
                            if (effect.EntityTypeId == EntityTypes.Damage)
                            {
                                defenseStatId = StatTypes.Resist;
                                if (casterIsFeebleMind)
                                {
                                    newQuantity = Math.Max(1, newQuantity / 2);
                                }
                            }
                            else
                            {
                                if (casterIsWeak)
                                {
                                    newQuantity = Math.Max(1, newQuantity / 2);
                                }
                            }
                            if (target.DefendRank == EDefendRanks.Defend)
                            {
                                newQuantity = (long)Math.Max(1, newQuantity * combatSettings.DefendDamageScale);
                            }
                            else if (target.DefendRank == EDefendRanks.Taunt)
                            {
                                newQuantity = (long)Math.Max(1, newQuantity * combatSettings.TauntDamageScale);
                            }


                            long defenseStat = target.Stats.Curr(defenseStatId);

                            if (casterHit < defenseStat)
                            {
                                double ratio = MathUtils.Clamp(combatSettings.MinHitToDefenseRatio
                                    , 1.0 * casterHit / defenseStat,
                                    combatSettings.MaxHitToDefenseRatio);

                                double newQuantityFract = ratio * newQuantity;

                                newQuantity = (long)newQuantityFract;

                                newQuantityFract -= newQuantity;

                                if (gs.rand.NextDouble() < newQuantityFract)
                                { 
                                    newQuantity++;
                                }

                                newQuantity = Math.Max(1, newQuantity);
                            }

                            string actionWord = (effect.EntityTypeId == EntityTypes.Attack ? "Attacks" :
                                effect.EntityTypeId == EntityTypes.Shoot ? "Shoots" :
                                 fullEffect.ElementType.ObserverActionName);
                            AddToActionDict(actionList, actionWord, newQuantity);
                        }
                        totalDamage += newQuantity;
                        _statService.Add(gs, target, StatTypes.Health, StatCategories.Curr, -newQuantity);
                    }
                    else if (effect.EntityTypeId == EntityTypes.Healing)
                    {
                        if (maxHealing < 1)
                        {
                            break;
                        }
                        long mult = (isSingleTarget || !fullEffect.InitialEffect ? 1 : spell.Level);
                        newQuantity += MathUtils.LongRange(hit.MinQuantity * mult, hit.MaxQuantity * mult, gs.rand);

                        if (casterIsFeebleMind)
                        {
                            newQuantity = Math.Max(1, newQuantity / 2);
                        }
                        if (newQuantity > maxHealing)
                        {
                            newQuantity = maxHealing;
                        }
                        maxHealing -= newQuantity;

                        totalHealing += newQuantity;

                        healTarget = target;
                        if (isEnemyTarget)
                        {
                            healTarget = caster;
                        }
                        _statService.Add(gs, healTarget, StatTypes.Health, StatCategories.Curr, newQuantity);
                        AddToActionDict(actionList, "Heals", newQuantity);
                    }
                    else if (currHitTimes == 0)
                    {
                        if (casterIsFeebleMind && gs.rand.NextDouble() < 0.5f)
                        {
                            continue;
                        }
                        if (effect.EntityTypeId == EntityTypes.StatusEffect)
                        {
                            StatusEffect statusEffect = gs.data.Get<StatusEffectSettings>(null).Get(effect.EntityId);
                            if (statusEffect == null)
                            {
                                continue;
                            }
                            if (effect.MaxQuantity < 0)
                            {
                                if (target.StatusEffects.HasBit(effect.EntityId))
                                {
                                    target.RemoveStatusBit(effect.EntityId);
                                    fullAction = $"{caster.Name} Cleanses {target.Name} of {statusEffect.Name}";
                                }
                            }
                            else
                            {
                                IDisplayEffect currentEffect = target.Effects.FirstOrDefault(x =>
                                x.EntityTypeId == EntityTypes.StatusEffect &&
                                x.EntityId == effect.EntityId);
                                if (currentEffect != null)
                                {
                                    if (currentEffect.MaxDuration > 0)
                                    {
                                        if (hit.MaxQuantity > currentEffect.MaxDuration)
                                        {
                                            currentEffect.MaxDuration = effect.MaxQuantity;
                                        }
                                        if (hit.MaxQuantity > currentEffect.DurationLeft)
                                        {
                                            currentEffect.DurationLeft = effect.MaxQuantity;
                                        }
                                    }
                                }
                                else
                                {
                                    DisplayEffect displayEffect = new DisplayEffect()
                                    {
                                        MaxDuration = effect.MaxQuantity,
                                        DurationLeft = effect.MaxQuantity, // MaxQuantity == 0 means infinite
                                        EntityTypeId = EntityTypes.StatusEffect,
                                        EntityId = effect.EntityId,
                                    };
                                    target.AddEffect(displayEffect);
                                    fullAction = $"{target.Name} is affected by {statusEffect.Name}";
                                }
                            }
                        }
                        else if (currHitTimes == 0 && effect.EntityTypeId == EntityTypes.Stat && effect.MaxQuantity > 0)
                        {

                        }
                    }
                    if (!string.IsNullOrEmpty(fullAction))
                    {
                        await ShowText(gs, party, fullAction, delay);
                    }
                }
                currHitTimes++;
                spell.HitsLeft--;

                bool isDead = target.Stats.Get(StatTypes.Health, StatCategories.Curr) <= 0;
                if (spell.HitsLeft < 1 || isDead)
                {
                    party.StatusPanel.RefreshUnit(target);
                    foreach (string actionName in actionList.Keys)
                    {
                        ActionListItem actionListItem = actionList[actionName];
                        await ShowText(gs, party, $"{caster.Name} {actionName} {healTarget.Name} {actionListItem.TotalHits}x for {actionListItem.TotalQuantity}", delay);
                    }

                    if (isDead)
                    {
                        target.StatusEffects.SetBit(StatusEffects.Dead);
                        await ShowText(gs, party, $"{target.Name} is DEAD!\n", delay);
                    }
                    break;
                }
            }

            await Task.CompletedTask;
        }
    }
}