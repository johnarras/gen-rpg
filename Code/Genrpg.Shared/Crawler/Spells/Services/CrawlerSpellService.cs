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

namespace Genrpg.Shared.Crawler.Spells.Services
{
    public interface ICrawlerSpellService : IInjectable
    {
        List<CrawlerSpell> GetSpellsForMember(PartyData party, PartyMember member);
        List<CrawlerSpell> GetNonSpellCombatActionsForMember(PartyData party, PartyMember member);
        FullSpell GetFullSpell(PartyData party, CrawlerUnit unit, CrawlerSpell spell, long overrideLevel = 0);
        Task CastSpell(PartyData party, UnitAction action, long overrideLevel = 0, int depth = 0);
        ISpecialMagicHelper GetSpecialEffectHelper(long effectEntityId);
        void RemoveSpellPowerCost(PartyData party, PartyMember member, CrawlerSpell spell);
        void SetupCombatData(PartyData party, PartyMember member);
        long GetPowerCost(PartyData party, PartyMember member, CrawlerSpell spell);
        bool IsEnemyTarget(long targetTypeId);
        bool IsNonCombatTarget(long targetTypeId);
        long GetSummonQuantity(PartyData party, PartyMember member);
    }



    public class CrawlerSpellService : ICrawlerSpellService
    {

        class ExtraMessageBits
        {
            public const long Resists = (1 << 0);
            public const long Vulnerable = (1 << 1);
            public const long Misses = (1 << 2);
        }


        private IStatService _statService = null;
        private ILogService _logService = null;
        private ICrawlerCombatService _combatService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected IClientRandom _rand = null;
        protected ICrawlerStatService _crawlerStatService = null;
        private ITextService _textService = null;
        private IRoleService _roleService;
        private IDispatcher _dispatcher;

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

                if (spell.Level > member.Level)
                {
                    continue;
                }

                if (spell.HasFlag(CrawlerSpellFlags.MonsterOnly))
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
                okSpells = okSpells.OrderBy(x => x.CombatActionId).ToList();
            }
            return okSpells;
        }

        // Figure out what this unit's combat hit will look like.
        public FullSpell GetFullSpell(PartyData party, CrawlerUnit caster, CrawlerSpell spell, long overrideLevel = 0)
        {
            FullSpell fullSpell = new FullSpell() { Spell = spell };

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(null);

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

                bool finalQuantityIsNegativeAttackCount = false;

                if (effect.EntityTypeId == EntityTypes.Attack)
                {
                    equipSlotToCheck = EquipSlots.MainHand;
                    statUsedForScaling = StatTypes.Strength;
                    oneEffect.HitType = EHitTypes.Melee;
                    attacksPerLevel += _roleService.GetScalingBonusPerLevel(party, caster, RoleScalingTypes.Melee);
                }
                else if (effect.EntityTypeId == EntityTypes.Shoot)
                {
                    oneEffect.HitType = EHitTypes.Ranged;
                    equipSlotToCheck = EquipSlots.Ranged;
                    statUsedForScaling = StatTypes.Agility;
                    attacksPerLevel += _roleService.GetScalingBonusPerLevel(party, caster, RoleScalingTypes.Ranged);
                }
                else
                {
                    quantityIsBaseDamage = true;
                    oneEffect.HitType = EHitTypes.Spell;
                    equipSlotToCheck = EquipSlots.MainHand;
                    if (effect.EntityTypeId == EntityTypes.Damage)
                    {
                        statUsedForScaling = StatTypes.Intellect;
                        attacksPerLevel += _roleService.GetScalingBonusPerLevel(party, caster, RoleScalingTypes.SpellDam);
                    }
                    else if (effect.EntityTypeId == EntityTypes.Healing)
                    {
                        statUsedForScaling = StatTypes.Devotion;
                        attacksPerLevel += _roleService.GetScalingBonusPerLevel(party, caster, RoleScalingTypes.Healing);
                    }
                    else if (effect.EntityTypeId == EntityTypes.StatusEffect)
                    {
                        if (effect.MaxQuantity < 0)
                        {
                            attacksPerLevel += _roleService.GetScalingBonusPerLevel(party, caster, RoleScalingTypes.Healing);
                            finalQuantityIsNegativeAttackCount = true;
                        }
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

                oneEffect.MinQuantity += _crawlerStatService.GetStatBonus(party, caster, statUsedForScaling);
                oneEffect.MaxQuantity += _crawlerStatService.GetStatBonus(party, caster, statUsedForScaling);

                long baseDamageBonus = _crawlerStatService.GetStatBonus(party, caster, StatTypes.DamagePower);

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
                        double currAttackQuantity = (caster.IsPlayer() ? CrawlerCombatConstants.BasePlayerAttackQuantity : CrawlerCombatConstants.BaseAttackQuantity) +
                            attacksPerLevel * (abilityLevel - 1); // -1 here since so level 1 doesn't double dip.

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

        public long GetPowerCost(PartyData party, PartyMember member, CrawlerSpell spell)
        {
            return 0;
        }

        public void RemoveSpellPowerCost(PartyData party, PartyMember member, CrawlerSpell spell)
        {

            long powerCost = spell.GetPowerCost(member.Level);

            if (powerCost > 0)
            {
                long currMana = member.Stats.Curr(StatTypes.Mana);
                _statService.Add(member, StatTypes.Mana, StatCategories.Curr, -Math.Min(powerCost, currMana));
                party.StatusPanel.RefreshUnit(member);
            }
        }

        public async Task CastSpell(PartyData party, UnitAction action, long overrideLevel = 0, int depth = 0)
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
                    if (!action.Caster.StatusEffects.HasBit(StatusEffects.Dead))
                    {
                        await ShowText(party, $"{action.Caster.Name} is disabled!", displayDelay);
                    }
                    return;
                }

                if (_combatService.IsActionBlocked(party, action.Caster, action.Spell.CombatActionId))
                {
                    await ShowText(party, $"{action.Caster.Name} was blocked from performing that action!", displayDelay);
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

                if (!foundOkTarget)
                {
                    return;
                }

                if (!fullSpell.Spell.HasFlag(CrawlerSpellFlags.SuppressCastText) && fullSpell.LuckyHitQuantity < 1)
                {
                    await ShowText(party, $"{action.Caster.Name} casts {fullSpell.Spell.Name}", displayDelay);
                    if (fullSpell.LuckyHitQuantity == 1)
                    {
                        await ShowText(party, _textService.HighlightText("1 Lucky Hit!", TextColors.ColorGold), displayDelay);
                    }
                    else if (fullSpell.LuckyHitQuantity > 1)
                    {
                        await ShowText(party, _textService.HighlightText($"{fullSpell.LuckyHitQuantity} Lucky Hits!", TextColors.ColorGold), displayDelay);
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
                        party.WorldPanel.SetPicture(action.Caster.PortraitName, false);
                    }

                    if (action.FinalTargets.Count == 0 || action.FinalTargets[0].DefendRank < EDefendRanks.Guardian)
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


                if (action.FinalTargets.Count > 0)
                {
                    long originalHitsLeft = fullSpell.HitsLeft;
                    string combatGroupId = action.FinalTargets[0].CombatGroupId;
                    foreach (CrawlerUnit target in action.FinalTargets)
                    {
                        if (fullSpell.Spell.TargetTypeId == TargetTypes.AllEnemyGroups &&
                            target.CombatGroupId != combatGroupId)
                        {
                            fullSpell.HitsLeft = originalHitsLeft;
                            combatGroupId = target.CombatGroupId;
                        }

                        await CastSpellOnUnit(party, action.Caster, fullSpell, target, displayDelay);
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
            party.ActionPanel.AddText(text);

            DateTime startTime = DateTime.UtcNow;

            while ((DateTime.UtcNow - startTime).TotalSeconds < delay)
            {
                await Task.Delay(100);

                if (party.SpeedupListener.TriggerSpeedupNow())
                {
                    break;
                }
            }
        }

        private void AddToActionDict(Dictionary<string, ActionListItem> dict, string actionName, long quantity, long extraMessageBits, bool regularHit, ECombatTextTypes textType)
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
            dict[actionName].ExtraMessageBits |= extraMessageBits;
            dict[actionName].IsRegularHit = regularHit;
            dict[actionName].TextType = textType;
            
        }

        internal class ActionListItem
        {
            public long TotalQuantity { get; set; }
            public long TotalHits { get; set; }
            public long ExtraMessageBits { get; set; }
            public bool IsRegularHit { get; set; }
            public ECombatTextTypes TextType { get; set; }           
        }

        public async Task CastSpellOnUnit(PartyData party, CrawlerUnit caster, FullSpell spell, CrawlerUnit target, float delay = 0.5f)
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


                        if (target.DefendRank == 0 && hit.CritChance > 0 &&
                            _rand.NextDouble() * 100 < hit.CritChance && !casterIsWeakened)
                        {
                            newQuantity = target.Stats.Curr(StatTypes.Health);
                            AddToActionDict(actionList, "CRITS!", newQuantity, extraMessageBits, false, ECombatTextTypes.Damage);
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

                            if (_rand.NextDouble() > hitChance)
                            {
                                AddToActionDict(actionList, "Misses", 0, ExtraMessageBits.Misses, false, ECombatTextTypes.None);
                            }

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
                            AddToActionDict(actionList, actionWord, newQuantity, extraMessageBits, true, ECombatTextTypes.Damage);
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

                            if (caster is PartyMember member)
                            {
                                quantity = GetSummonQuantity(party, member);
                            }
                            _combatService.AddCombatUnits(party, unitType, Math.Max(1, (long)(quantity * unitType.SpawnQuantityScale)), caster.FactionTypeId);
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
                        _statService.Add(healTarget, StatTypes.Health, StatCategories.Curr, newQuantity);
                        AddToActionDict(actionList, "Heals", newQuantity, 0, false, ECombatTextTypes.Healing);
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
                    party.StatusPanel.RefreshUnit(target);

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
                                actionListItem.TextType);
                        }
                    }

                    if (isDead)
                    {
                        target.StatusEffects.SetBit(StatusEffects.Dead);
                        party.WorldPanel.UpdateCombatGroups();
                        await ShowText(party, $"{target.Name} is DEAD!\n", delay, true);
                        ShowFloatingCombatText(target, "DEAD!", ECombatTextTypes.Info);
                    }
                    break;
                }
            }

            await Task.CompletedTask;
        }


        private void ShowFloatingCombatText(CrawlerUnit unit, string text, ECombatTextTypes textType)
        {
            _dispatcher.Dispatch(new ShowCombatText() { GroupId = unit.CombatGroupId, UnitId = unit.Id, Text = text, TextType = textType });
        }


        public void SetupCombatData(PartyData partyData, PartyMember member)
        {
        }

        public bool IsEnemyTarget(long targetTypeId)
        {
            return targetTypeId == TargetTypes.Enemy ||
                targetTypeId == TargetTypes.EnemyGroup ||
                targetTypeId == TargetTypes.AllEnemies ||
                targetTypeId == TargetTypes.AllEnemyGroups;
        }

        public bool IsNonCombatTarget(long targetTypeId)
        {
            return targetTypeId == TargetTypes.Item ||
                targetTypeId == TargetTypes.Special ||
                targetTypeId == TargetTypes.Location;
        }

        public long GetSummonQuantity(PartyData party, PartyMember member)
        {
            double roleSum = _roleService.GetScalingBonusPerLevel(party, member, RoleScalingTypes.Summon);
            long abilityLevel = member.GetAbilityLevel();
            double quantity = (1.0 + roleSum * (abilityLevel-1));
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

            return (int)quantity;
        }
    }
}
