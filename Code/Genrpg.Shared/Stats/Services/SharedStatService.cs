
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Levels.Entities;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Genrpg.Shared.Stats.Services
{

    public class SharedStatService : IStatService
    {
        /// <summary>
        /// Recalculate stats for a monster/player based on level, equipment and spell effects
        /// </summary>
        /// <param name="gs">GameState</param>
        /// <param name="unit">The monster to set up</param>
        /// <param name="resetMutableStats">Do we reset health and mana to max or not?</param>
        public virtual void CalcStats(GameState gs, Unit unit, bool resetMutableStats)
        {
            if (unit == null || gs.data == null)
            {
                return;
            }

            UnitType utype = gs.data.GetGameData<UnitSettings>(unit).GetUnitType(unit.EntityId);

            if (utype == null)
            {
                return;
            }
            unit.AddFlag(UnitFlags.SuppressStatUpdates);

            List<SpellProc> oldProcs = unit.Procs;
            unit.Procs = new List<SpellProc>();

            Dictionary<long, long> oldStats = new Dictionary<long, long>();
            Dictionary<long, long> oldMaxStats = new Dictionary<long, long>();

            List<StatType> mutableStats = GetMutableStatTypes(gs, unit);

            foreach (StatType stat in mutableStats)
            {
                oldStats[stat.IdKey] = unit.Stats.Curr(stat.IdKey);
                oldMaxStats[stat.IdKey] = unit.Stats.Max(stat.IdKey);
            }

            LevelInfo levelData = gs.data.GetGameData<LevelSettings>(unit).GetLevel(unit.Level);

            unit.Stats.ResetCurrent();

            // Basic stat amount with basic monster stat amount.
            long coreStatAmount = StatConstants.MinBaseStat;
            long monsterScalePercent = 0;

            List<StatType> coreStats = gs.data.GetGameData<StatSettings>(unit).GetData().Where(x => x.IdKey >= StatConstants.PrimaryStatStart && x.IdKey <= StatConstants.PrimaryStatEnd).ToList();

            if (levelData != null)
            {
                coreStatAmount = levelData.StatAmount;
                monsterScalePercent = levelData.MonsterStatScale;
            }

            unit.BaseStatAmount = coreStatAmount;
            foreach (StatType stat in coreStats)
            {
                Set(gs, unit, stat.IdKey, StatCategories.Base, coreStatAmount);

                if (monsterScalePercent != 0)
                {
                    Add(gs, unit, stat.IdKey, StatCategories.Pct, monsterScalePercent);
                }
            }

            // Mutable stats with a max pool get reset before doing other bonuses.
            foreach (StatType ms in mutableStats)
            {
                if (ms.MaxPool > 0)
                {
                    Set(gs, unit, ms.IdKey, StatCategories.Base, ms.MaxPool);
                }
            }

            // Now do additional monster scaling like +/- health or dam.
            if (utype != null && utype.Effects != null)
            {
                foreach (BaseEffect eff in utype.Effects)
                {
                    AddEffectStat(gs, unit, eff, levelData, 1);
                }
            }

            if (unit.SpellEffects != null)
            {
                foreach (ActiveSpellEffect speff in unit.SpellEffects)
                {
                    AddEffectStat(gs, unit, speff, levelData, 1);
                }
            }

            Character ch = unit as Character;

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
                            AddEffectStat(gs, unit, list[l], levelData, 1);

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

                                SetType setType = gs.data.GetGameData<SetTypeSettings>(unit).GetSetType(setId);
                                if (setType != null && setType.Pieces != null)
                                {
                                    SetPiece piece = setType.Pieces.FirstOrDefault(x => x.ItemTypeId == eq.ItemTypeId);
                                    if (piece != null && piece.Procs != null)
                                    {
                                        foreach (SpellProc proc in piece.Procs)
                                        {
                                            unit.AddProc(gs, proc);
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
                    SetType setType = gs.data.GetGameData<SetTypeSettings>(unit).GetSetType(setId);
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
                                Add(gs, unit, sb.StatTypeId, StatCategories.Base, levelData.StatAmount * sb.Percent / 100);
                            }
                        }
                    }

                    if (setType.Procs != null)
                    {
                        foreach (SetSpellProc proc in setType.Procs)
                        {
                            if (proc.ItemCount <= quantity)
                            {
                                unit.AddProc(gs, proc);
                            }
                        }
                    }
                }

                AbilityData abilityData = ch.Get<AbilityData>();
                // Now add effects from skills and elements.
                List<AbilityRank> abilities = abilityData.GetData();

                List<AbilityEffect> abEffects = null;
                foreach (AbilityRank ab in abilities)
                {
                    if (ab.Rank <= AbilityData.DefaultRank)
                    {
                        continue;
                    }
                }
            }

            if (unit.StatPct != 0 && unit.StatPct != 100)
            {
                foreach (Stat stat in unit.Stats.GetAllStats())
                {
                    Set(gs, unit, stat.Id, StatCategories.Base, stat.Get(StatCategories.Pct) * unit.StatPct / 100);
                }
            }

            AddDerivedStats(gs, unit, levelData);

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
                Set(gs, unit, stat.IdKey, StatCategories.Curr, newValue);

                if (resetMutableStats ||
                    oldMaxStats.ContainsKey(stat.IdKey) &&
                    oldMaxStats[stat.IdKey] != newValue)
                {
                    statIdsToSend.Add(stat.IdKey);
                }
            }
            unit.RemoveFlag(UnitFlags.SuppressStatUpdates);
            SendUpdatedStats(gs, unit, statIdsToSend);
        }


        public void Add(GameState gs, Unit unit, long statTypeId, int statCategory, long value)
        {
            Set(gs, unit, statTypeId, statCategory, unit.Stats.Get(statTypeId, statCategory) + value);
        }

        public void Set(GameState gs, Unit unit, long statTypeId, int statCategory, long newVal)
        {
            long oldVal = unit.Stats.Get(statTypeId, statCategory);

            unit.Stats.Set(statTypeId, statCategory, newVal);

            if (oldVal != newVal)
            {
                SendUpdatedStats(gs, unit, new List<long>() { statTypeId });
            }
        }

        protected virtual void SendUpdatedStats(GameState gs, Unit unit, List<long> statIdsToSend)
        {
        }

        public virtual void RegenerateTick(GameState gs, Unit unit, float regenTickTime = StatConstants.RegenTickSeconds)
        {

        }
        /// <summary>
        /// Convert base stats light might and intellect to physical attack and magic attack
        /// </summary>
        /// <param name="gs">GameState</param>
        /// <param name="unit">The monster modified</param>
        /// <param name="levData">The data used for level-scaled stats</param>
        protected virtual void AddDerivedStats(GameState gs, Unit unit, LevelInfo levData)
        {
            if (unit == null)
            {
                return;
            }

            List<DerivedStat> list = gs.data.GetGameData<DerivedStatSettings>(unit).GetData();

            for (int d = 0; d < list.Count; d++)
            {
                DerivedStat ds = list[d];
                int addval = (int)(ds.Percent * unit.Stats.Max(ds.FromStatTypeId) / 100);
                Add(gs, unit, ds.ToStatTypeId, StatCategories.Base, addval);
            }
        }

        public List<StatType> GetMutableStatTypes(GameState gs, Unit unit)
        {
            return gs.data.GetGameData<StatSettings>(unit).GetData().Where(x => x.IdKey > 0 && x.IdKey <= StatConstants.MaxMutableStatTypeId).ToList();
        }

        public List<StatType> GetPrimaryStatTypes(GameState gs, Unit unit)
        {
            return gs.data.GetGameData<StatSettings>(unit).GetData().Where(x => x.IdKey >= StatConstants.PrimaryStatStart && x.IdKey <= StatConstants.PrimaryStatEnd).ToList();
        }

        public List<StatType> GetAttackStatTypes(GameState gs, Unit unit)
        {
            return GetPrimaryStatTypes(gs, unit).Where(x => x.IdKey != StatTypes.Stamina).ToList();
        }


        public List<StatType> GetFixedStatTypes(GameState gs, Unit unit)
        {
            return gs.data.GetGameData<StatSettings>(unit).GetData().Where(x => x.IdKey > StatConstants.MaxMutableStatTypeId).ToList();
        }

        public List<StatType> GetSecondaryStatTypes(GameState gs, Unit unit)
        {
            return gs.data.GetGameData<StatSettings>(unit).GetData().Where(x => x.IdKey > StatConstants.PrimaryStatEnd).ToList();
        }

        /// <summary>
        /// Add a stat value to a monster/player based on the effect and level of the monster.
        /// </summary>
        /// <param name="gs">GameState</param>
        /// <param name="unit">Monster/Player to modify</param>
        /// <param name="e">The effect doing the modifiying</param>
        /// <param name="levData">Level data used for the StatLevelPercent effects</param>
        protected void AddEffectStat(GameState gs, Unit unit, IEffect e, LevelInfo levData, int multiplier)
        {
            if (unit == null || e == null)
            {
                return;
            }

            if (e.EntityTypeId == EntityTypes.Stat)
            {
                Add(gs, unit, e.EntityId, StatCategories.Base, e.Quantity * multiplier);
            }
            else if (e.EntityTypeId == EntityTypes.StatPct)
            {
                Add(gs, unit, e.EntityId, StatCategories.Pct, e.Quantity * multiplier);
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
