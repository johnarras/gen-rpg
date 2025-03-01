using Assets.Scripts.Crawler.ClientEvents.ActionPanelEvents;
using Assets.Scripts.Crawler.ClientEvents.WorldPanelEvents;
using Assets.Scripts.Crawler.Constants;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Services;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Constants;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
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
using Genrpg.Shared.Spells.Settings.Targets;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Genrpg.Shared.Crawler.Spells.Services
{
    public interface ICrawlerSpellService : IInjectable
    {
        List<CrawlerSpell> GetSpellsForMember(PartyData party, PartyMember member);
        List<CrawlerSpell> GetNonSpellCombatActionsForMember(PartyData party, PartyMember member);
        FullSpell GetFullSpell(PartyData party, CrawlerUnit unit, CrawlerSpell spell, long overrideLevel = 0);
        Task CastSpell(PartyData party, UnitAction action, long overrideLevel = 0, int depth = 0);
        ISpecialMagicHelper GetSpecialEffectHelper(long effectEntityId);
        void RemoveSpellPowerCost(PartyData party, CrawlerUnit member, CrawlerSpell spell);
        void SetupCombatData(PartyData party, PartyMember member);
        long GetPowerCost(PartyData party, CrawlerUnit unit, CrawlerSpell spell);
        bool IsEnemyTarget(long targetTypeId);
        bool IsNonCombatTarget(long targetTypeId);
        long GetSummonQuantity(PartyData party, PartyMember member, UnitType unitType);
    }



    public class CrawlerSpellService : ICrawlerSpellService
    {

        class ExtraMessageBits
        {
            public const long Resists = (1 << 0);
            public const long Vulnerable = (1 << 1);
            public const long Misses = (1 << 2);
        }


        private ILogService _logService = null;
        private ICrawlerCombatService _combatService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected IClientRandom _rand = null;
        protected ICrawlerStatService _crawlerStatService = null;
        private ITextService _textService = null;
        private IRoleService _roleService;
        private IDispatcher _dispatcher;
        private ICrawlerService _crawlerService;

        private SetupDictionaryContainer<long, ISpecialMagicHelper> _effectHelpers = new SetupDictionaryContainer<long, ISpecialMagicHelper>();

        public ISpecialMagicHelper GetSpecialEffectHelper(long specialEffectId)
        {
            if (_effectHelpers.TryGetValue(specialEffectId, out ISpecialMagicHelper specialEffectHelper))
            {
                return specialEffectHelper;
            }
            return null;
        }

        public List<CrawlerSpell> GetNonSpellCombatActionsForMember(
            PartyData party, PartyMember member)
        {
            return GetAbilitiesForMember(party, member, false);
        }

        public List<CrawlerSpell> GetSpellsForMember(PartyData party,
            PartyMember member)
        {
            return GetAbilitiesForMember(party, member, true);
        }

        private List<CrawlerSpell> GetAbilitiesForMember(PartyData party,
            PartyMember member, bool chooseSpells)
        {
            EActionCategories actionCategory = party.GetActionCategory();

            IReadOnlyList<CrawlerSpell> allSpells = _gameData.Get<CrawlerSpellSettings>(null).GetData();

            List<CrawlerSpell> castSpells = allSpells.Where(x =>
            (x.CombatActionId == CombatActions.Cast) == chooseSpells).ToList();

            List<CrawlerSpell> okSpells = new List<CrawlerSpell>();

            RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);

            Dictionary<long, long> roleScalingTiers = new Dictionary<long, long>();

            IReadOnlyList<RoleScalingType> roleScalingTypes = _gameData.Get<RoleScalingTypeSettings>(_gs.ch).GetData();

            foreach (RoleScalingType roleScaling in roleScalingTypes)
            {
                roleScalingTiers[roleScaling.IdKey] = (long)_roleService.GetScalingTier(party, member, roleScaling.IdKey);
            }

            List<Role> roles = roleSettings.GetRoles(member.Roles);

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

                if (!roleScalingTiers.ContainsKey(spell.RoleScalingTypeId))
                {
                    _logService.Info("Bad RoleScalingType on " + spell.Name + ": " + spell.RoleScalingTypeId);
                    continue;
                }

                if (spell.Level > roleScalingTiers[spell.RoleScalingTypeId])
                {
                    continue;
                }

                if (_combatService.IsActionBlocked(party, member, spell.CombatActionId))
                {
                    continue;
                }

                if (!roleSettings.HasBonus(member.Roles, EntityTypes.CrawlerSpell, spell.IdKey))
                {
                    continue;
                }

                if (actionCategory == EActionCategories.NonCombat)
                {
                    if (IsEnemyTarget(spell.TargetTypeId))
                    {
                        continue;
                    }

                    // No stat buffs outside of combat for the moment to keep it simpler
                    if (spell.Effects.Any(x => x.EntityTypeId == EntityTypes.Stat))
                    {
                        continue;
                    }
                }
                else // in combat
                {
                    if (IsNonCombatTarget(spell.TargetTypeId))
                    {
                        continue;
                    }

                    // Only defensive things during preparation round.
                    if (actionCategory == EActionCategories.Preparing && 
                        IsEnemyTarget(spell.TargetTypeId))
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

            if (!chooseSpells)
            {
                okSpells = okSpells.OrderBy(x => x.CombatActionId).ThenBy(x=>x.TargetTypeId).ToList();
            }
            return okSpells;
        }

        // Figure out what this unit's combat hit will look like.
        public FullSpell GetFullSpell(PartyData party, CrawlerUnit caster, CrawlerSpell spell, long overrideLevel = 0)
        {
            FullSpell fullSpell = new FullSpell() { Spell = spell };

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(null);

            RoleScalingType scalingType = _gameData.Get<RoleScalingTypeSettings>(_gs.ch).Get(spell.RoleScalingTypeId);

            TargetType targetType = _gameData.Get<TargetTypeSettings>(_gs.ch).Get(spell.TargetTypeId);

            double critChance = 0;

            double attackQuantity = 0;

            long casterLevel = caster.GetAbilityLevel();

            if (caster is PartyMember member)
            {
                critChance += _gameData.Get<RoleSettings>(_gs.ch).GetRoles(member.Roles).Sum(x => x.CritPercent);
                if (spell.TargetTypeId == TargetTypes.Enemy && member.HideExtraRange > 0)
                {
                    critChance += combatSettings.HiddenSingleTargetCritPercent;
                }
                if (spell.CombatActionId != CombatActions.Hide)
                {
                    member.HideExtraRange = 0;
                }

                if (party.Combat != null)
                {
                    member.LastCombatCrawlerSpellId = spell.IdKey;
                }
            }

            CombatAction action = _gameData.Get<CombatActionSettings>(_gs.ch).Get(spell.CombatActionId);


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

                List<IProc> procList = GetProcsFromSlot(caster, scalingType.ScalingEquipSlotId);

                foreach (IProc proc in procList)
                {
                    endFullEffectList.Add(CreateFullEffectFromProc(proc));
                }
            }

            Monster monster = caster as Monster;

            if (monster != null && IsEnemyTarget(spell.TargetTypeId))
            {
                endFullEffectList.AddRange(monster.ApplyEffects);
            }

            long statUsedForScaling = scalingType.ScalingStatTypeId;
            double attacksPerLevel = _roleService.GetScalingBonusPerLevel(party, caster, scalingType.IdKey);
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

                long equipSlotToCheck = scalingType.ScalingEquipSlotId;

                bool finalQuantityIsNegativeAttackCount = false;


                if (effect.EntityTypeId == EntityTypes.Attack)
                {
                    oneEffect.HitType = EHitTypes.Melee;
                }
                else if (effect.EntityTypeId == EntityTypes.Shoot)
                {
                    oneEffect.HitType = EHitTypes.Ranged;
                }
                else
                {
                    oneEffect.HitType = EHitTypes.Spell;
                    if (effect.EntityTypeId == EntityTypes.StatusEffect && effect.MaxQuantity < 0)
                    {
                        finalQuantityIsNegativeAttackCount = true;
                    }
                }

                if (fullEffect.InitialEffect)
                {

                    long luck = caster.Stats.Max(StatTypes.Luck);

                    double luckRatio = luck * 1.0 / caster.Level;

                    luckRatio = Math.Min(luckRatio, combatSettings.MaxLuckCritRatio);

                    critChance += luckRatio * combatSettings.LuckCritChanceAtLevel;

                    oneEffect.CritChance = (long)critChance;
                }

                if (action.QuantityIsBaseAmount)
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
                    LootRank lootRank = _gameData.Get<LootRankSettings>(null).Get(weapon.LootRankId);

                    double minVal = itype.MinVal;
                    double maxVal = itype.MaxVal;

                    if (lootRank != null)
                    {
                        minVal += lootRank.Damage;
                        maxVal += lootRank.Damage;
                    }

                    minVal *= action.WeaponDamageScale;
                    maxVal *= action.WeaponDamageScale;

                    oneEffect.MinQuantity += (long)(minVal);
                    oneEffect.MaxQuantity += (long)(maxVal);

                }
                else if (effect.EntityTypeId == EntityTypes.Attack && monster != null)
                {
                    oneEffect.MinQuantity = monster.MinDam;
                    oneEffect.MaxQuantity = monster.MaxDam;
                }

                double statBonus = _crawlerStatService.GetStatBonus(party, caster, statUsedForScaling) * targetType.StatBonusScale;
                oneEffect.MinQuantity += (long)(Math.Floor(action.StatBonusDamageScale*statBonus));
                oneEffect.MaxQuantity += (long)Math.Ceiling(action.StatBonusDamageScale*statBonus);

                long baseDamageBonus = _crawlerStatService.GetStatBonus(party, caster, StatTypes.DamagePower);

                oneEffect.MinQuantity += baseDamageBonus;
                oneEffect.MaxQuantity += baseDamageBonus;

                oneEffect.MinQuantity = Math.Max(oneEffect.MinQuantity, CrawlerCombatConstants.BaseMinDamage);
                oneEffect.MaxQuantity = Math.Max(oneEffect.MaxQuantity, CrawlerCombatConstants.BaseMaxDamage);

                if (fullEffect.InitialEffect)
                {
                    if (effect.MinQuantity > 0 && effect.MaxQuantity > 0 && !action.QuantityIsBaseAmount)
                    {
                        attackQuantity = MathUtils.LongRange(effect.MinQuantity, effect.MaxQuantity, _rand);
                    }
                    else
                    {
                        double currAttackQuantity = (caster.IsPlayer() ? combatSettings.BasePlayerAttacks : combatSettings.BaseMonsterAttacks) + 
                            attacksPerLevel * (abilityLevel - 1); // -1 here since so level 1 doesn't double dip.

                        // Higher level spells only get "ticks" based on the levels after you learn them.
                        currAttackQuantity -= (spell.Level - 1);

                        if (currAttackQuantity > attackQuantity)
                        {
                            attackQuantity = currAttackQuantity;
                        }
                    }
                    // Used for cures.
                    if (finalQuantityIsNegativeAttackCount)
                    {
                        effect.MinQuantity = -(long)attackQuantity;
                        effect.MaxQuantity = -(long)attackQuantity;
                    }
                }
            }

            long intAttackQuantity = (long)(attackQuantity);
            if (_rand.NextDouble() < (attackQuantity - (long)attackQuantity))
            {
                attackQuantity++;
            }

            long luckBonus = _crawlerStatService.GetStatBonus(party, caster, StatTypes.Luck);

            long luckyAttackCount = 0;
            for (int a = 0; a < attackQuantity; a++)
            {
                if (_rand.NextDouble() * 100 < luckBonus)
                {
                    luckyAttackCount++;
                }
            }
            attackQuantity += luckyAttackCount;

            fullSpell.HitQuantity = Math.Max(1, (long)attackQuantity);
            fullSpell.LuckyHitQuantity = luckyAttackCount;
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

        public void RemoveSpellPowerCost(PartyData party, CrawlerUnit member, CrawlerSpell spell)
        {

            long powerCost = GetPowerCost(party, member, spell);

            if (powerCost > 0)
            {
                long currMana = member.Stats.Curr(StatTypes.Mana);
                _crawlerStatService.Add(party, member, StatTypes.Mana, StatCategories.Curr, -Math.Min(powerCost, currMana));
            }
        }

        public long GetPowerCost(PartyData partyData, CrawlerUnit unit, CrawlerSpell spell)
        {
            long tier = (long)_roleService.GetScalingTier(partyData, unit, spell.RoleScalingTypeId);

            return (long)(spell.PowerCost + (tier * spell.PowerPerLevel));
        }

        public async Task CastSpell(PartyData party, UnitAction action, long overrideLevel = 0, int depth = 0)
        {
            try
            {
                float informationalDisplayDelay = 0;

                action.IsComplete = true;
                if (action.Spell == null)
                {
                    return;
                }

                if (_combatService.IsDisabled(action.Caster))
                {
                    if (!action.Caster.StatusEffects.HasBit(StatusEffects.Dead))
                    {
                        await ShowText(party, $"{action.Caster.Name} is disabled!", informationalDisplayDelay);
                    }
                    return;
                }

                if (_combatService.IsActionBlocked(party, action.Caster, action.Spell.CombatActionId))
                {
                    await ShowText(party, $"{action.Caster.Name} was blocked from performing that action!", informationalDisplayDelay);
                    return;
                }


                FullSpell fullSpell = GetFullSpell(party, action.Caster, action.Spell, overrideLevel);

                bool foundOkTarget = false;
                if (!IsNonCombatTarget(fullSpell.Spell.TargetTypeId))
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
                else
                {
                    foundOkTarget = true;
                }

                if (!foundOkTarget)
                {
                    return;
                }

                if (!fullSpell.Spell.HasFlag(CrawlerSpellFlags.SuppressCastText) && fullSpell.LuckyHitQuantity < 1)
                {
                    await ShowText(party, $"{action.Caster.Name} casts {fullSpell.Spell.Name}", informationalDisplayDelay);
                    if (fullSpell.LuckyHitQuantity == 1)
                    {
                        await ShowText(party, _textService.HighlightText("1 Lucky Hit!", TextColors.ColorGold), informationalDisplayDelay);
                    }
                    else if (fullSpell.LuckyHitQuantity > 1)
                    {
                        await ShowText(party, _textService.HighlightText($"{fullSpell.LuckyHitQuantity} Lucky Hits!", TextColors.ColorGold), informationalDisplayDelay);
                    }
                }

                if (action.Caster is PartyMember pmember)
                {
                    RemoveSpellPowerCost(party, pmember, action.Spell);
                }

                if (party.Combat != null)
                {
                    if (!string.IsNullOrEmpty(action.Caster.PortraitName))
                    {
                        _dispatcher.Dispatch(new SetWorldPicture(action.Caster.PortraitName, false));
                    }

                    if (action.FinalTargets.Count == 0 || action.FinalTargets[0].DefendRank < EDefendRanks.Guardian)
                    {
                        List<CombatGroup> groups = new List<CombatGroup>();

                        if (action.Spell.TargetTypeId == TargetTypes.AllEnemies)
                        {
                            groups = action.Caster.FactionTypeId == FactionTypes.Player ? party.Combat.Enemies : party.Combat.Allies;

                        }
                        else if (action.Spell.TargetTypeId == TargetTypes.AllEnemiesInAGroup)
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


                if (action.FinalTargets.Count > 0)
                {
                    long originalHitsLeft = fullSpell.HitsLeft;
                    string combatGroupId = action.FinalTargets[0].CombatGroupId;
                    foreach (CrawlerUnit target in action.FinalTargets)
                    {
                        if (fullSpell.Spell.TargetTypeId == TargetTypes.EnemyInEachGroup &&
                            target.CombatGroupId != combatGroupId)
                        {
                            fullSpell.HitsLeft = originalHitsLeft;
                            combatGroupId = target.CombatGroupId;
                        }

                        await CastSpellOnUnit(party, action.Caster, fullSpell, target, party.CombatTextScrollDelay);
                    }
                }
            }
            catch (Exception e)
            {
                _logService.Exception(e, "CastSpell");
            }
        }

        private async Task ShowText(PartyData party, string text, float delay = 0.0f, bool updateGroups = false)
        {
            _dispatcher.Dispatch(new AddActionPanelText(text));

            DateTime startTime = DateTime.UtcNow;

            if (delay > 0 && !_crawlerService.TriggerSpeedupNow())
            {
                while ((DateTime.UtcNow - startTime).TotalSeconds < delay)
                {
                    await Task.Delay(1);

                    if (_crawlerService.TriggerSpeedupNow())
                    {
                        break;
                    }
                }
            }
        }

        private void AddToActionDict(Dictionary<string, ActionListItem> dict, string actionName, long quantity, long extraMessageBits, bool regularHit, ECombatTextTypes textType, long elementTypeId)
        {
            if (string.IsNullOrEmpty(actionName))
            {
                return;
            }

            if (!dict.ContainsKey(actionName))
            {
                dict[actionName] = new ActionListItem();
            }

            if (dict[actionName].ElementTypeId == 0)
            {
                dict[actionName].ElementTypeId = elementTypeId;
            }
            dict[actionName].TotalQuantity += quantity;
            dict[actionName].TotalHits++;
            dict[actionName].ExtraMessageBits |= extraMessageBits;
            dict[actionName].IsRegularHit = regularHit;
            dict[actionName].TextType = textType;
            
        }

        internal class ActionListItem
        {
            public long ElementTypeId { get; set; }
            public long TotalQuantity { get; set; }
            public long TotalHits { get; set; }
            public long ExtraMessageBits { get; set; }
            public bool IsRegularHit { get; set; }
            public ECombatTextTypes TextType { get; set; }           
        }

        public async Task CastSpellOnUnit(PartyData party, CrawlerUnit caster, FullSpell spell, CrawlerUnit target, float delay)
        {
            if (spell.Spell.TargetTypeId == TargetTypes.AllEnemiesInAGroup ||
                spell.Spell.TargetTypeId == TargetTypes.AllAllies ||
                spell.Spell.TargetTypeId == TargetTypes.AllEnemies)
            {
                spell.HitsLeft = Math.Max(spell.HitQuantity, 1);
            }
            if (caster.StatusEffects.HasBit(StatusEffects.Cursed))
            {
                spell.HitsLeft = Math.Max(1, spell.HitsLeft / 2);
            }

            bool isEnemyTarget = IsEnemyTarget(spell.Spell.TargetTypeId);

            if (isEnemyTarget && target.StatusEffects.HasBit(StatusEffects.Dead))
            {
                return;
            }

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(null);
            RoleSettings roleSettings = _gameData.Get<RoleSettings>(null);

            long currHealth = target.Stats.Curr(StatTypes.Health);
            long maxHealth = target.Stats.Max(StatTypes.Health);

            long maxDamage = currHealth;
            long maxHealing = maxHealth - currHealth;

            bool haveMultiHitEffect = spell.Effects.Any(x =>
            x.Effect.EntityTypeId == EntityTypes.Damage ||
            x.Effect.EntityTypeId == EntityTypes.Healing ||
            x.Effect.EntityTypeId == EntityTypes.Attack ||
            x.Effect.EntityTypeId == EntityTypes.Shoot);

            if (!haveMultiHitEffect)
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

            Dictionary<string, ActionListItem> actionList = new Dictionary<string, ActionListItem>();

            long extraMessageBits = 0;
            
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
                    extraMessageBits = 0;
                    CrawlerSpellEffect effect = fullEffect.Effect;
                    OneEffect hit = fullEffect.Hit;

                    long finalMinQuantity = hit.MinQuantity;
                    long finalMaxQuantity = hit.MaxQuantity;

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

                    if (_rand.NextDouble() * 100 > fullEffect.PercentChance)
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

                        double damageScale = 1.0f;
                        long elementBits = (long)(1 << (int)effect.ElementTypeId);

                        double finalCritChance = hit.CritChance;
                        if (FlagUtils.IsSet(target.ResistBits, elementBits))
                        {
                            if (!FlagUtils.IsSet(target.VulnBits, elementBits))
                            {
                                damageScale /= combatSettings.VulnerabilityDamageMult;
                                finalCritChance += combatSettings.ResistAddCritChance;                            
                                extraMessageBits |= ExtraMessageBits.Resists;
                            }
                        }
                        else if (FlagUtils.IsSet(target.VulnBits, elementBits))
                        {
                            damageScale *= combatSettings.VulnerabilityDamageMult;
                            extraMessageBits |= ExtraMessageBits.Vulnerable;
                            finalCritChance += combatSettings.VulnAddCritChance;
                        }


                        if (!target.IsPlayer() && target.DefendRank == 0 && finalCritChance > 0 &&
                            _rand.NextDouble() * 100 < finalCritChance && !casterIsWeakened)
                        {
                            newQuantity = target.Stats.Curr(StatTypes.Health);
                            AddToActionDict(actionList, "CRITS!", newQuantity, extraMessageBits, false, ECombatTextTypes.Damage, spell.Effects[0].ElementType.IdKey);
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

                            if (target.DefendRank == EDefendRanks.Defend)
                            {
                                damageScale = combatSettings.DefendDamageScale;
                            }
                            else if (target.DefendRank == EDefendRanks.Guardian)
                            {
                                damageScale = combatSettings.GuardianDamageScale;
                            }
                            else if (target.DefendRank == EDefendRanks.Taunt)
                            {
                                damageScale *= combatSettings.TauntDamageScale;
                            }

                            newQuantity = (long)Math.Max(1, newQuantity * damageScale);

                            long defenseStat = target.Stats.Max(defenseStatId);

                            float defenseStatRatio = 1.0f * casterHit / Math.Max(1, defenseStat);

                            double hitChance = defenseStatRatio / combatSettings.GuaranteedHitDefenseRatio;

                            bool didMiss = false;
                            if (_rand.NextDouble() > hitChance)
                            {
                                AddToActionDict(actionList, "Misses", 0, ExtraMessageBits.Misses, false, ECombatTextTypes.None, 0);
                                didMiss = true;
                                newQuantity = 0;
                            }
                            
                            if (casterHit < defenseStat && !didMiss)
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
                            AddToActionDict(actionList, actionWord, newQuantity, extraMessageBits, true, ECombatTextTypes.Damage, spell.Effects[0].ElementType.IdKey);
                        }
                        totalDamage += newQuantity;
                        _crawlerStatService.Add(party, target, StatTypes.Health, StatCategories.Curr, -totalDamage, effect.ElementTypeId);
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

                            if (caster is PartyMember member)
                            {
                                quantity = GetSummonQuantity(party, member, unitType);
                            }

                            InitialCombatGroup icg = new InitialCombatGroup()
                            {
                                Quantity = quantity,
                                UnitTypeId = unitTypeId,
                                FactionTypeId = caster.FactionTypeId,
                                Level = caster.Level,
                                Range = CrawlerCombatConstants.MinRange,
                            };

                            _combatService.AddCombatUnits(party, icg);
                        }
                        else if (partyMember != null)
                        {
                            long currRoleId = -1;
                            foreach (Role role in roleSettings.GetData())
                            {
                                if (role.BinaryBonuses.Any(x=>x.EntityTypeId == EntityTypes.CrawlerSpell && x.EntityId == spell.Spell.IdKey))
                                {
                                    partyMember.Summons = partyMember.Summons.Where(x => x.RoleId != role.IdKey).ToList();
                                    currRoleId = role.IdKey;    
                                }
                            }

                            partyMember.Summons.Add(new PartySummon()
                            {
                                Id = partyMember.Id + "." + _rand.Next(),
                                Name = unitType.Name,
                                UnitTypeId = unitType.IdKey,
                                RoleId = currRoleId,
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
                        _crawlerStatService.Add(party, target, StatTypes.Health, StatCategories.Curr, totalHealing);
                        AddToActionDict(actionList, "Heals", newQuantity, 0, false, ECombatTextTypes.Healing, 0);
                    }
                    else if (effect.EntityTypeId == EntityTypes.PartyBuff)
                    {
                        double tier = _roleService.GetScalingTier(party, caster, RoleScalingTypes.Utility);

                        party.Buffs.Set(effect.EntityId, (float)Math.Sqrt(tier));
                        _dispatcher.Dispatch(new CrawlerUIUpdate());
                        _dispatcher.Dispatch(new ShowPartyMinimap() { Party = party });
                    }
                    else if (currHitTimes == 0)
                    {
                        if (casterIsWeakened && _rand.NextDouble() < 0.5f)
                        {
                            continue;
                        }

                        if (effect.EntityTypeId == EntityTypes.StatusEffect)
                        {
                            IReadOnlyList<StatusEffect> allEffects = _gameData.Get<StatusEffectSettings>(null).GetData();

                            if (effect.MaxQuantity < 0)
                            {
                                long quantityRemoved = Math.Abs(effect.MaxQuantity);

                                for (int i = 0; i < allEffects.Count && quantityRemoved > 0; i++)
                                {

                                    if (allEffects[i].IdKey < 1)
                                    {
                                        continue;
                                    }
                                    quantityRemoved--;
                                    if (target.StatusEffects.HasBit(allEffects[i].IdKey))
                                    {
                                        target.RemoveStatusBit(effect.EntityId);
                                        fullAction = $"{caster.Name} Cleanses {target.Name} of {allEffects[i].Name}";
                                    }
                                }

                            }
                            else
                            {
                                StatusEffect statusEffect = allEffects.FirstOrDefault(x => x.IdKey == effect.EntityId);
                                if (effect != null)
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
                        }
                        else if (currHitTimes == 0 && effect.EntityTypeId == EntityTypes.Stat && effect.MaxQuantity > 0)
                        {
                            if (party.Combat != null)
                            {
                                StatVal statVal = party.Combat.StatBuffs.FirstOrDefault(x => x.StatTypeId == effect.EntityId);
                                if (statVal == null)
                                {
                                    statVal = new StatVal()
                                    {
                                        StatTypeId = (short)effect.EntityId,
                                    };
                                    party.Combat.StatBuffs.Add(statVal);
                                }

                                if (statVal.Val < caster.Level)
                                {
                                    statVal.Val = (int)caster.Level;
                                    _crawlerStatService.CalcPartyStats(party, false);
                                }
                            }
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
                    bool didShowMisses = false;
                    foreach (string actionName in actionList.Keys)
                    {
                        ActionListItem actionListItem = actionList[actionName];

                        string extraWords = "";

                        if (FlagUtils.IsSet(actionListItem.ExtraMessageBits, ExtraMessageBits.Misses))
                        {
                            continue;
                        }

                        if (FlagUtils.IsSet(actionListItem.ExtraMessageBits, ExtraMessageBits.Resists))
                        {
                            extraWords = "(Resist)";
                        }
                        else if (FlagUtils.IsSet(actionListItem.ExtraMessageBits, ExtraMessageBits.Vulnerable))
                        {
                            extraWords = "(Vulnerable)";
                        }

                        string hitText = actionListItem.TotalHits + "x";

                        if (actionListItem.IsRegularHit && !didShowMisses)
                        {
                            long missCount = 0;
                            foreach (ActionListItem item in actionList.Values)
                            {
                                if (FlagUtils.IsSet(item.ExtraMessageBits, ExtraMessageBits.Misses))
                                {
                                    missCount += item.TotalHits;
                                }
                            }

                            if (missCount > 0)
                            {
                                hitText += " (" + missCount + " miss)";
                            }
                            didShowMisses = true;
                        }

                        await ShowText(party, $"{caster.Name} {actionName} {healTarget.Name} {hitText}"
                            + (actionListItem.TotalQuantity > 0 ? $" for {actionListItem.TotalQuantity} " : "")
                            + " " + $"{extraWords}", delay);
                        if (actionListItem.TextType != ECombatTextTypes.None && actionListItem.TotalQuantity != 0)
                        {
                            ShowFloatingCombatText(healTarget, 
                                (actionListItem.TextType == ECombatTextTypes.Damage ? "-" : "") + actionListItem.TotalQuantity,
                                actionListItem.TextType, actionListItem.ElementTypeId);
                        }
                    }

                    if (isDead)
                    {
                        target.StatusEffects.SetBit(StatusEffects.Dead);
                        _dispatcher.Dispatch(new UpdateCombatGroups());
                        await ShowText(party, $"{target.Name} is DEAD!\n", 0, true);
                        ShowFloatingCombatText(target, "DEAD!", ECombatTextTypes.Info, 0);
                    }
                    break;
                }
            }

            await Task.CompletedTask;
        }


        private void ShowFloatingCombatText(CrawlerUnit unit, string text, ECombatTextTypes textType, long elementTypeId)
        {
            _dispatcher.Dispatch(new ShowCombatText() { GroupId = unit.CombatGroupId, UnitId = unit.Id, Text = text, TextType = textType, ElementTypeId = elementTypeId });
        }


        public void SetupCombatData(PartyData partyData, PartyMember member)
        {
        }

        public bool IsEnemyTarget(long targetTypeId)
        {
            return targetTypeId == TargetTypes.Enemy ||
                targetTypeId == TargetTypes.AllEnemiesInAGroup ||
                targetTypeId == TargetTypes.AllEnemies ||
                targetTypeId == TargetTypes.EnemyInEachGroup;
        }

        public bool IsNonCombatTarget(long targetTypeId)
        {
            return targetTypeId == TargetTypes.Item ||
                targetTypeId == TargetTypes.Special ||
                targetTypeId == TargetTypes.World;
        }

        public long GetSummonQuantity(PartyData party, PartyMember member, UnitType unitType)
        {
            double roleSum = _roleService.GetScalingBonusPerLevel(party, member, RoleScalingTypes.Summon);
            long abilityLevel = member.GetAbilityLevel();
            double quantity = (1.0 + roleSum * (abilityLevel-1));

            CrawlerSpell summonSpell = _gameData.Get<CrawlerSpellSettings>(_gs.ch).GetData().FirstOrDefault(x => x.Effects.Any(e => e.EntityTypeId == EntityTypes.Unit && e.EntityId == unitType.IdKey));

            if (summonSpell != null)
            {
                quantity -= summonSpell.Level - 1;
            }

            quantity *= _gameData.Get<CrawlerCombatSettings>(_gs.ch).SummonQuantityScale;

            // 1.5 here for rounding and not random scaling value combat to combat

            if (_rand.NextDouble() < (quantity - (int)quantity))
            {
                quantity = Math.Ceiling(quantity);
            }

            long luckBonus = _crawlerStatService.GetStatBonus(party, member, StatTypes.Luck);

            long luckySummonCount = 0;
            for (int q = 0; q < quantity; q++)
            {
                if (_rand.NextDouble() * 100 < luckBonus)
                {
                    luckySummonCount++;
                }
            }
            quantity += luckySummonCount;

            return (int)Math.Max(1, Math.Sqrt(quantity));
        }
    }
}
