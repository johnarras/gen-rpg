﻿using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Charms.Constants;
using Genrpg.Shared.Charms.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Settings.ItemSets;
using Genrpg.Shared.Levels.Settings;
using Genrpg.Shared.Spells.Casting;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Spells.PlayerData;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Stats.Settings.DerivedStats;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace Genrpg.Shared.Stats.Services
{

    public class SharedStatService : IStatService
    {

        protected IGameData _gameData;

        /// <summary>
        /// Recalculate stats for a monster/player based on level, equipment and spell effects
        /// </summary>
        /// <param name="gs">GameState</param>
        /// <param name="unit">The monster to set up</param>
        /// <param name="resetMutableStats">Do we reset health and mana to max or not?</param>
        public virtual void CalcStats(Unit unit, bool resetMutableStats)
        {
            if (unit == null)
            {
                return;
            }

            UnitType utype = _gameData.Get<UnitSettings>(unit).Get(unit.EntityId);

            if (utype == null)
            {
                return;
            }
            unit.AddFlag(UnitFlags.SuppressStatUpdates);

            List<OldSpellProc> oldProcs = unit.Procs;
            unit.Procs = new List<OldSpellProc>();

            Dictionary<long, long> oldStats = new Dictionary<long, long>();
            Dictionary<long, long> oldMaxStats = new Dictionary<long, long>();

            List<StatType> mutableStats = GetMutableStatTypes(unit);

            foreach (StatType stat in mutableStats)
            {
                oldStats[stat.IdKey] = unit.Stats.Curr(stat.IdKey);
                oldMaxStats[stat.IdKey] = unit.Stats.Max(stat.IdKey);
            }

            LevelInfo levelData = _gameData.Get<LevelSettings>(unit).Get(unit.Level);

            unit.Stats.ResetAll();

            Character ch = unit as Character;

            PlayerCharmData charmData = unit.Get<PlayerCharmData>();

            PlayerCharm charmStatus = charmData.GetData().FirstOrDefault(x => x.CurrentCharmUseId == CharmUses.Character &&
            x.TargetId == unit.Id);

            // Basic stat amount with basic monster stat amount.
            long coreStatAmount = StatConstants.MinBaseStat;
            long monsterScalePercent = 0;

            List<StatType> coreStats = _gameData.Get<StatSettings>(unit).GetData().Where(x => x.IdKey >= StatConstants.PrimaryStatStart && x.IdKey <= StatConstants.PrimaryStatEnd).ToList();

            if (levelData != null)
            {
                coreStatAmount = levelData.StatAmount;

                monsterScalePercent = (ch == null ? levelData.MonsterStatScale : 0);
            }

            unit.BaseStatAmount = coreStatAmount;
            foreach (StatType stat in coreStats)
            {
                Set(unit, stat.IdKey, StatCategories.Base, coreStatAmount);

                if (monsterScalePercent != 0)
                {
                    Add(unit, stat.IdKey, StatCategories.Pct, monsterScalePercent);
                }
            }

            // Mutable stats with a max pool get reset before doing other bonuses.
            foreach (StatType ms in mutableStats)
            {
                if (ms.MaxPool > 0)
                {
                    Set(unit, ms.IdKey, StatCategories.Base, ms.MaxPool);
                }
            }

            // Now do additional monster scaling like +/- health or dam.
            if (utype != null && utype.Effects != null)
            {
                foreach (UnitEffect eff in utype.Effects)
                {
                    AddEffectStat(unit, eff, levelData, 1);
                }
            }

            if (unit.Effects != null)
            {
                foreach (IDisplayEffect speff in unit.Effects)
                {
                    AddEffectStat(unit, speff, levelData, 1);
                }
            }

            if (ch != null)
            {
                Dictionary<long, int> setQuantities = new Dictionary<long, int>();
                // Now scale based on equipment the user has.

                InventoryData inventory = ch.Get<InventoryData>();
                List<Item> equip = inventory.GetAllEquipment();
                for (int e = 0; e < equip.Count; e++)
                {
                    Item eq = equip[e];
                    List<ItemEffect> list = eq.Effects;
                    if (list != null)
                    {
                        for (int l = 0; l < list.Count; l++)
                        {
                            AddEffectStat(unit, list[l], levelData, 1);

                            if (list[l].EntityTypeId == EntityTypes.Set && list[l].EntityId > 0)
                            {
                                long setId = list[l].EntityId;
                                if (!setQuantities.ContainsKey(setId))
                                {
                                    setQuantities[setId] = 1;
                                }
                                else
                                {
                                    setQuantities[setId]++;
                                }

                                SetType setType = _gameData.Get<SetTypeSettings>(unit).Get(setId);
                                if (setType != null && setType.Pieces != null)
                                {
                                    SetPiece piece = setType.Pieces.FirstOrDefault(x => x.ItemTypeId == eq.ItemTypeId);
                                    if (piece != null && piece.OldProcs != null)
                                    {
                                        foreach (OldSpellProc proc in piece.OldProcs)
                                        {
                                            unit.AddProc(proc);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                foreach (long setId in setQuantities.Keys)
                {
                    int quantity = setQuantities[setId];
                    SetType setType = _gameData.Get<SetTypeSettings>(unit).Get(setId);
                    if (setType == null)
                    {
                        continue;
                    }

                    if (setType.Stats != null)
                    {
                        foreach (SetStat sb in setType.Stats)
                        {
                            if (sb.ItemCount <= quantity)
                            {
                                Add(unit, sb.StatTypeId, StatCategories.Base, levelData.StatAmount * sb.Percent / 100);
                            }
                        }
                    }

                    if (setType.Procs != null)
                    {
                        foreach (SetSpellProc proc in setType.Procs)
                        {
                            if (proc.ItemCount <= quantity)
                            {
                                unit.AddProc(proc);
                            }
                        }
                    }
                }

                CombatAbilityData abilityData = ch.Get<CombatAbilityData>();
                // Now add effects from skills and elements.
                IReadOnlyList<CombatAbilityRank> abilities = abilityData.GetData();

                foreach (CombatAbilityRank ab in abilities)
                {
                    if (ab.Rank <= AbilityConstants.DefaultRank)
                    {
                        continue;
                    }
                }
            }

            if (charmStatus != null)
            {
                PlayerCharmBonusList bonusList = charmStatus.Bonuses.FirstOrDefault(x => x.CharmUseId == CharmUses.Character);

                if (bonusList != null)
                {
                    foreach (PlayerCharmBonus charmBonus in bonusList.Bonuses)
                    {
                        if (charmBonus.EntityTypeId == EntityTypes.Stat)
                        {
                            Add(unit, charmBonus.EntityId, StatCategories.Pct, charmBonus.Quantity);
                        }
                    }
                }
            }

            if (unit.StatPct != 0 && unit.StatPct != 100)
            {
                for (int statTypeId = 1; statTypeId < StatTypes.Max; statTypeId++)
                { 
                    Set(unit, statTypeId, StatCategories.Base, unit.Stats.Pct(statTypeId) * unit.StatPct / 100);
                }
            }

            AddDerivedStats(unit, levelData);

            // Now for anything mutable, set the current value to the same or new lower value, or perhaps the 
            // old value or max value.

            List<long> statIdsToSend = new List<long>();

            foreach (StatType stat in mutableStats)
            {
                long newValue = unit.Stats.Max(stat.IdKey);
                if (resetMutableStats)
                {
                    if (stat.RegenSeconds < 0)
                    {
                        newValue = 0;
                    }
                }
                else
                {
                    if (oldStats.ContainsKey(stat.IdKey))
                    {
                        newValue = Math.Min(newValue, oldStats[stat.IdKey]);
                    }
                }
                Set(unit, stat.IdKey, StatCategories.Curr, newValue);

                if (resetMutableStats ||
                    oldMaxStats.ContainsKey(stat.IdKey) &&
                    oldMaxStats[stat.IdKey] != newValue)
                {
                    statIdsToSend.Add(stat.IdKey);
                }
            }
            unit.RemoveFlag(UnitFlags.SuppressStatUpdates);
            SendUpdatedStats(unit, statIdsToSend);
        }


        public void Add(Unit unit, long statTypeId, int statCategory, long value)
        {
            Set(unit, statTypeId, statCategory, unit.Stats.Get(statTypeId, statCategory) + value);
        }

        public void Set(Unit unit, long statTypeId, int statCategory, long newVal)
        {
            long oldVal = unit.Stats.Get(statTypeId, statCategory);

            unit.Stats.Set(statTypeId, statCategory, newVal);

            if (oldVal != newVal)
            {
                SendUpdatedStats(unit, new List<long>() { statTypeId });
            }
        }

        protected virtual void SendUpdatedStats(Unit unit, List<long> statIdsToSend)
        {
        }

        public virtual void RegenerateTick(IRandom rand, Unit unit, float regenTickTime = StatConstants.RegenTickSeconds)
        {

        }
        /// <summary>
        /// Convert base stats light might and intellect to physical attack and magic attack
        /// </summary>
        /// <param name="gs">GameState</param>
        /// <param name="unit">The monster modified</param>
        /// <param name="levData">The data used for level-scaled stats</param>
        protected virtual void AddDerivedStats(Unit unit, LevelInfo levData)
        {
            if (unit == null)
            {
                return;
            }

            IReadOnlyList<DerivedStat> list = _gameData.Get<DerivedStatSettings>(unit).GetData();

            for (int d = 0; d < list.Count; d++)
            {
                DerivedStat ds = list[d];
                int addval = (int)(ds.Percent * unit.Stats.Max(ds.FromStatTypeId) / 100);
                Add(unit, ds.ToStatTypeId, StatCategories.Base, addval);
            }
        }

        public List<StatType> GetMutableStatTypes(Unit unit)
        {
            return _gameData.Get<StatSettings>(unit).GetData().Where(x => x.IdKey > 0 && x.IdKey <= StatConstants.MaxMutableStatTypeId).ToList();
        }

        public List<StatType> GetPrimaryStatTypes(Unit unit)
        {
            return _gameData.Get<StatSettings>(unit).GetData().Where(x => x.IdKey >= StatConstants.PrimaryStatStart && x.IdKey <= StatConstants.PrimaryStatEnd).ToList();
        }

        public List<StatType> GetAttackStatTypes(Unit unit)
        {
            return GetPrimaryStatTypes(unit).Where(x => x.IdKey != StatTypes.Stamina).ToList();
        }


        public List<StatType> GetFixedStatTypes(Unit unit)
        {
            return _gameData.Get<StatSettings>(unit).GetData().Where(x => x.IdKey > StatConstants.MaxMutableStatTypeId).ToList();
        }

        public List<StatType> GetSecondaryStatTypes(Unit unit)
        {
            return _gameData.Get<StatSettings>(unit).GetData().Where(x => x.IdKey > StatConstants.PrimaryStatEnd).ToList();
        }

        /// <summary>
        /// Add a stat value to a monster/player based on the effect and level of the monster.
        /// </summary>
        /// <param name="gs">GameState</param>
        /// <param name="unit">Monster/Player to modify</param>
        /// <param name="e">The effect doing the modifiying</param>
        /// <param name="levData">Level data used for the StatLevelPercent effects</param>
        protected void AddEffectStat(Unit unit, IEffect e, LevelInfo levData, int multiplier)
        {
            if (unit == null || e == null)
            {
                return;
            }

            if (e.EntityTypeId == EntityTypes.Stat)
            {
                Add(unit, e.EntityId, StatCategories.Base, e.Quantity * multiplier);
            }
            else if (e.EntityTypeId == EntityTypes.StatPct)
            {
                Add(unit, e.EntityId, StatCategories.Pct, e.Quantity * multiplier);
            }
        }


        /// <summary>
        /// Stat converted to a percent at the owner's given level.
        /// </summary>
        /// <param name="statTypeId"></param>
        /// <returns></returns>
        public float Pct(Unit unit, long statTypeId)
        {
            long div = 10;
            if (unit.BaseStatAmount > 0)
            {
                div = unit.BaseStatAmount;
            }
            return 0.01f * unit.Stats.Curr(statTypeId) / div;
        }
    }


}
