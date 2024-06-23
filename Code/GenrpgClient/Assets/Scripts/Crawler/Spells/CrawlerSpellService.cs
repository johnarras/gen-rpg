using Assets.Scripts.Crawler.Services.Combat;
using Assets.Scripts.ProcGen.RandomNumbers;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Constants;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.Spells.Utils;
using Genrpg.Shared.Crawler.Stats.Utils;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Inventory.Settings.LootRanks;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Spells.Procs.Entities;
using Genrpg.Shared.Spells.Procs.Interfaces;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Units.Entities;
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
        private ILogService _logService;
        private IRepositoryService _repoService;
        private ICombatService _combatService;
        protected IGameData _gameData;
        protected IUnityGameState _gs;
        protected IClientRandom _rand;

        public List<CrawlerSpell> GetNonSpellCombatActionsForMember(
            PartyData party, PartyMember member, bool inCombat)
        {
            return GetAbilitiesForMember(party, member, inCombat, false);
        }

        public List<CrawlerSpell> GetSpellsForMember(PartyData party, 
            PartyMember member, bool inCombat)
        {
            return GetAbilitiesForMember(party, member, inCombat, true);
        }

        private List<CrawlerSpell> GetAbilitiesForMember(PartyData party, 
            PartyMember member, bool inCombat, bool chooseSpells)
        { 

            IReadOnlyList<CrawlerSpell> allSpells = _gameData.Get<CrawlerSpellSettings>(null).GetData();

            List<CrawlerSpell> castSpells = allSpells.Where(x => 
            (x.CombatActionId == CombatActions.Cast) == chooseSpells).ToList();

            List<CrawlerSpell> okSpells = new List<CrawlerSpell>();

            List<Class> classes = _gameData.Get<ClassSettings>(null).GetClasses(member.Classes);

            if (_combatService.IsDisabled(member))
            {
                return okSpells;
            }

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

                if (spell.HasFlag(CrawlerSpellFlags.MonsterOnly))
                {
                    continue;
                }

                if (spell.RequiredStatusEffectId > 0 && !member.StatusEffects.HasBit(spell.RequiredStatusEffectId))
                {
                    continue;
                }

                if (_combatService.IsActionBlocked(member, spell.CombatActionId))
                {
                    continue;
                }

                bool foundSpellInClass = false;

                foreach (Class cl in classes)
                {
                    if (cl.Bonuses.Any(x => x.EntityTypeId == EntityTypes.CrawlerSpell && x.EntityId == spell.IdKey))
                    {
                        {
                            foundSpellInClass = true;
                            break;
                        }
                    }

                    if (foundSpellInClass)
                    {
                        break;
                    }
                }

                if (!foundSpellInClass)
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
        public FullSpell GetFullSpell(CrawlerUnit caster, CrawlerSpell spell, long overrideLevel = 0)
        {
            FullSpell fullSpell = new FullSpell() { Spell = spell };

            List<Class> classes = new List<Class>();

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(null);

            ClassSettings classSettings = _gameData.Get<ClassSettings>(null);

            double critChance = 0;

            long attackQuantity = 0;

            long casterLevel = caster.GetAbilityLevel();

            if (caster is PartyMember member)
            {
                classes = _gameData.Get<ClassSettings>(null).GetClasses(member.Classes);

                for (int c = 0; c < classes.Count; c++)
                {
                    critChance += classes[c].CritPercent;
                }

                if (spell.CritChance > 0)
                {
                    double spellCritChance = spell.CritChance;
                    critChance += spellCritChance;
                }
            }

            long abilityLevel = (overrideLevel < 1 ? casterLevel : overrideLevel);

            fullSpell.Level = abilityLevel;

            List<long> actionTypesWithProcsSet = new List<long>();

            ElementTypeSettings elemSettings = _gameData.Get<ElementTypeSettings>(null);

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
                    allProcLists.Add(GetProcsFromSlot(caster, EquipSlots.PoisonVial));
                    allProcLists.Add(GetProcsFromSlot(caster, EquipSlots.Quiver));
                    
                }
                else if (fullEffect.Effect.EntityTypeId == EntityTypes.Shoot)
                {
                    allProcLists.Add(GetProcsFromSlot(caster, EquipSlots.Quiver));
                    allProcLists.Add(GetProcsFromSlot(caster, EquipSlots.PoisonVial));
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

                double attacksPerLevel = 0;

                bool quantityIsBaseDamage = false;

                if (effect.EntityTypeId == EntityTypes.Attack)
                {
                    equipSlotToCheck = EquipSlots.MainHand;
                    statUsedForScaling = StatTypes.Strength;
                    oneEffect.HitType = EHitTypes.Melee;
                    attacksPerLevel += classes.Sum(x => 1.0 / x.LevelsPerMelee);
                }
                else if (effect.EntityTypeId == EntityTypes.Shoot)
                {
                    oneEffect.HitType = EHitTypes.Ranged;
                    equipSlotToCheck = EquipSlots.Ranged;                    
                    statUsedForScaling = StatTypes.Agility;
                    attacksPerLevel += classes.Sum(x => 1.0 / x.LevelsPerRanged);
                }
                else
                {
                    quantityIsBaseDamage = true;
                    oneEffect.HitType = EHitTypes.Spell;
                    equipSlotToCheck = EquipSlots.MainHand;
                    if (effect.EntityTypeId == EntityTypes.Damage)
                    {
                        statUsedForScaling = StatTypes.Intellect;
                        attacksPerLevel += classes.Sum(x => 1.0 / x.LevelsPerDamage);
                    }
                    else if (effect.EntityTypeId == EntityTypes.Healing)
                    {
                        statUsedForScaling = StatTypes.Devotion;
                        attacksPerLevel += classes.Sum(x => 1.0 / x.LevelsPerHeal);
                    }
                }

                if (fullEffect.InitialEffect)
                {
                    oneEffect.CritChance = (long)critChance;
                }
                if (quantityIsBaseDamage)
                {
                    oneEffect.MinQuantity = effect.MinQuantity;
                    oneEffect.MaxQuantity = effect.MaxQuantity;
                }
                else
                {
                    oneEffect.MinQuantity = 0;
                    oneEffect.MaxQuantity = 0;
                }

                Item weapon = caster.GetEquipmentInSlot(equipSlotToCheck);
                if (weapon != null)
                {
                    ItemType itype = _gameData.Get<ItemTypeSettings>(null).Get(weapon.ItemTypeId);

                    // Weapon affects physical attacks, not magical.
                    if (!quantityIsBaseDamage)
                    {
                        oneEffect.MinQuantity += itype.MinVal;
                        oneEffect.MaxQuantity += itype.MaxVal;
                    }

                    // Loot rank affects both.
                    LootRank lootRank = _gameData.Get<LootRankSettings>(null).Get(weapon.LootRankId);

                    if (lootRank != null)
                    {
                        // If the base damage is for the spell, this gets double value.
                        oneEffect.MinQuantity += lootRank.Damage * (quantityIsBaseDamage ? 2 : 1);
                        oneEffect.MaxQuantity += lootRank.Damage * (quantityIsBaseDamage ? 2 : 1);
                    }
                }
                else if (effect.EntityTypeId == EntityTypes.Attack && caster is Monster monster)
                {
                    oneEffect.MinQuantity = monster.MinDam;
                    oneEffect.MaxQuantity = monster.MaxDam;
                }

                oneEffect.MinQuantity += CrawlerStatUtils.GetStatBonus(caster, statUsedForScaling);
                oneEffect.MaxQuantity += CrawlerStatUtils.GetStatBonus(caster, statUsedForScaling);

                long baseDamageBonus = CrawlerStatUtils.GetStatBonus(caster, StatTypes.DamagePower);

                oneEffect.MinQuantity += baseDamageBonus;
                oneEffect.MaxQuantity += baseDamageBonus;

                oneEffect.MinQuantity = Math.Max(oneEffect.MinQuantity, CrawlerCombatConstants.BaseMinDamage);
                oneEffect.MaxQuantity = Math.Max(oneEffect.MaxQuantity, CrawlerCombatConstants.BaseMaxDamage);

                if (fullEffect.InitialEffect && caster is PartyMember partyMember)
                {

                    if (effect.MinQuantity > 0 && effect.MaxQuantity > 0 && !quantityIsBaseDamage)
                    {
                        attackQuantity = MathUtils.LongRange(effect.MinQuantity, effect.MaxQuantity, _rand);
                    }
                    else
                    {
                        double currAttackQuantity = CrawlerCombatConstants.BaseAttackQuantity +
                            attacksPerLevel * (abilityLevel-1); // -1 here since so level 1 doesn't double dip.

                        if (currAttackQuantity > attackQuantity)
                        {
                            attackQuantity = (long)currAttackQuantity;
                        }
                    }
                }
            }

            fullSpell.HitQuantity = Math.Max(1, attackQuantity);
            fullSpell.HitsLeft = fullSpell.HitQuantity;
            return fullSpell;
        }

        private List<IProc> GetProcsFromSlot(CrawlerUnit member, long equipSlotId)
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

        public async Awaitable CastSpell(PartyData party, UnitAction action, long overrideLevel = 0, int depth = 0)
        {
            try
            {
                float displayDelay = 0.05f;

                action.IsComplete = true;
                if (action.Spell == null)
                {
                    return;
                }

                if (_combatService.IsDisabled(action.Caster))
                {
                    await ShowText(party, $"{action.Caster.Name} is disabled!", displayDelay);
                    return;
                }

                if (_combatService.IsActionBlocked(action.Caster, action.Spell.CombatActionId))
                {
                    await ShowText (party, $"{action.Caster.Name} was blocked from performing that action!", displayDelay);
                    return;
                }


                FullSpell fullSpell = GetFullSpell(action.Caster, action.Spell, overrideLevel);
                
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

                if (!fullSpell.Spell.HasFlag(CrawlerSpellFlags.SuppressCastText))
                {
                    await ShowText(party, $"{action.Caster.Name} casts {fullSpell.Spell.Name}", displayDelay);
                }

                if (action.Caster is PartyMember pmember)
                {
                    long powerCost = action.Spell.GetPowerCost(pmember.Level);

                    if (powerCost > 0)
                    {
                        long currMana = pmember.Stats.Curr(StatTypes.Mana);
                        _statService.Add(pmember, StatTypes.Mana, StatCategories.Curr, -Math.Min(powerCost, currMana));
                        party.StatusPanel.RefreshUnit(pmember);
                    }
                }

                if (party.Combat != null)
                {
                    if (!string.IsNullOrEmpty(action.Caster.PortraitName))
                    {
                        party.WorldPanel.SetPicture(action.Caster.PortraitName);
                    }
                
                    if (action.FinalTargets.Count == 0 || action.FinalTargets[0].DefendRank != EDefendRanks.Taunt)
                    {
                        List<CombatGroup> groups = new List<CombatGroup>();

                        if (action.Spell.TargetTypeId == TargetTypes.AllEnemies)
                        {
                            groups = action.Caster.FactionTypeId == FactionTypes.Player ? party.Combat.Enemies : party.Combat.Allies;

                        }
                        else if (action.Spell.TargetTypeId == TargetTypes.EnemyGroup)
                        {
                            groups = action.FinalTargetGroups;
                        }
                        else if (action.Spell.TargetTypeId == TargetTypes.AllAllies)
                        {
                            groups = action.Caster.FactionTypeId == FactionTypes.Player ? party.Combat.Allies : party.Combat.Enemies;
                        }

                        if (groups.Count > 0)
                        {
                            groups = groups.Where(x => x.Range >= action.Spell.MinRange && x.Range <= action.Spell.MaxRange).ToList();

                            action.FinalTargets = new List<CrawlerUnit>();

                            foreach (CombatGroup group in groups)
                            {
                                action.FinalTargets.AddRange(group.Units);
                            }
                        }
                    }
                }

                foreach (CrawlerUnit target in action.FinalTargets)
                {
                    await CastSpellOnUnit(party, action.Caster, fullSpell, target, displayDelay);
                }
            }
            catch (Exception e)
            {
                _logService.Exception(e, "CastSpell");
            }
        }

        private async Awaitable ShowText(PartyData party, string text, float delay = 0.0f)
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

        public async Awaitable CastSpellOnUnit(PartyData party, CrawlerUnit caster, FullSpell spell, CrawlerUnit target,float delay = 0.5f)
        {
            if (spell.Spell.TargetTypeId == TargetTypes.EnemyGroup || 
                spell.Spell.TargetTypeId == TargetTypes.AllAllies ||
                spell.Spell.TargetTypeId == TargetTypes.AllEnemies)
            {

                spell.HitsLeft = Math.Max(spell.HitQuantity, 1);
            }

            if (caster.StatusEffects.HasBit(StatusEffects.Cursed))
            {
                spell.HitsLeft = Math.Max(1, spell.HitsLeft / 2);
            }

            bool isEnemyTarget = CrawlerSpellUtils.IsEnemyTarget(spell.Spell.TargetTypeId);

            if (isEnemyTarget && target.StatusEffects.HasBit(StatusEffects.Dead))
            {
                return;
            }

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(null);
            ClassSettings classSettings = _gameData.Get<ClassSettings>(null);

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

            long casterHit = caster.Stats.Max(StatTypes.Hit);
            CrawlerUnit healTarget = target;

            bool casterIsWeakened = _combatService.IsActionWeak(caster, spell.Spell.CombatActionId);

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

                    long finalMinQuantity = effect.MinQuantity;
                    long finalMaxQuantity = effect.MaxQuantity;

                    if (caster.StatusEffects.HasBit(StatusEffects.Poisoned))
                    {
                        finalMinQuantity = Math.Max(1, finalMinQuantity * 3 / 4);
                        finalMaxQuantity = Math.Max(1, finalMaxQuantity * 3 / 4);
                    }
                    if (caster.StatusEffects.HasBit(StatusEffects.Diseased))
                    {
                        finalMinQuantity = Math.Max(1, finalMinQuantity * 3 / 4);
                        finalMaxQuantity = Math.Max(1, finalMaxQuantity * 3 / 4);
                    }

                    if (_rand.NextDouble()*100 > fullEffect.PercentChance)
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
                            _rand.NextDouble() * 100 < hit.CritChance && !casterIsWeakened)
                        {
                            newQuantity = target.Stats.Curr(StatTypes.Health);
                            AddToActionDict(actionList, "CRITS!", newQuantity);
                            didKill = true;
                        }
                        else
                        {
                            long defenseStatId = StatTypes.Armor;
                            newQuantity = MathUtils.LongRange(finalMinQuantity, finalMaxQuantity, _rand);
                            if (effect.EntityTypeId == EntityTypes.Damage)
                            {
                                defenseStatId = StatTypes.Resist;
                            }

                            if (casterIsWeakened)
                            {
                                newQuantity = Math.Max(1, newQuantity / 2);
                            }

                            double damageScale = 1.0f;
                            if (target.DefendRank == EDefendRanks.Defend)
                            {
                                damageScale = combatSettings.DefendDamageScale;
                            }
                            else if (target.DefendRank == EDefendRanks.Taunt)
                            {
                                damageScale = combatSettings.TauntDamageScale;
                            }
                            newQuantity = (long)Math.Max(1, newQuantity * damageScale);

                            long defenseStat = target.Stats.Max(defenseStatId);

                            if (casterHit < defenseStat)
                            {
                                double ratio = MathUtils.Clamp(combatSettings.MinHitToDefenseRatio
                                    , 1.0 * casterHit / defenseStat,
                                    combatSettings.MaxHitToDefenseRatio);

                                double newQuantityFract = ratio * newQuantity;

                                newQuantity = (long)newQuantityFract;

                                newQuantityFract -= newQuantity;

                                if (_rand.NextDouble() < newQuantityFract)
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
                        _statService.Add(target, StatTypes.Health, StatCategories.Curr, -newQuantity);
                    }
                    else if (effect.EntityTypeId == EntityTypes.Unit)
                    {

                        PartyMember partyMember = caster as PartyMember;
                        long unitTypeId = effect.EntityId;

                        if (partyMember == null && unitTypeId == 0)
                        {
                            unitTypeId = caster.UnitTypeId;
                        }

                        UnitType unitType = _gameData.Get<UnitSettings>(null).Get(unitTypeId);

                        if (unitType == null)
                        {
                            fullAction = $"{caster.Name} tries to summon an unknown ally.";
                            continue;
                        }

                        if (party.Combat != null)
                        {
                            long quantity = MathUtils.LongRange(finalMinQuantity, finalMaxQuantity, _rand);
                            _combatService.AddCombatUnits(party, unitType, quantity, caster.FactionTypeId);


                        }
                        else if (partyMember != null)
                        {
                            partyMember.Summons.Clear();
                            partyMember.Summons.Add(new PartySummon()
                            {
                                Id = partyMember.Id + "." + _rand.Next(),
                                Name = unitType.Name,
                                UnitTypeId = unitType.IdKey
                            });
                        }
                    }
                    else if (effect.EntityTypeId == EntityTypes.Healing)
                    {
                        if (maxHealing < 1)
                        {
                            break;
                        }
                        newQuantity += MathUtils.LongRange(finalMinQuantity, finalMaxQuantity, _rand);

                        if (casterIsWeakened)
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
                        _statService.Add(healTarget, StatTypes.Health, StatCategories.Curr, newQuantity);
                        AddToActionDict(actionList, "Heals", newQuantity);
                    }
                    else if (currHitTimes == 0)
                    {
                        if (casterIsWeakened && _rand.NextDouble() < 0.5f)
                        {
                            continue;
                        }

                        if (effect.EntityTypeId == EntityTypes.StatusEffect)
                        {
                            StatusEffect statusEffect = _gameData.Get<StatusEffectSettings>(null).Get(effect.EntityId);
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
                        await ShowText(party, fullAction, delay);
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
                        await ShowText(party, $"{caster.Name} {actionName} {healTarget.Name} {actionListItem.TotalHits}x for {actionListItem.TotalQuantity}", delay);
                    }

                    if (isDead)
                    {
                        target.StatusEffects.SetBit(StatusEffects.Dead);
                        await ShowText(party, $"{target.Name} is DEAD!\n", delay);
                    }
                    break;
                }
            }

            await Task.CompletedTask;
        }
    }
}
