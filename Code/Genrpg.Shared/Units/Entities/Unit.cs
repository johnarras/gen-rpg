using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Stats.Messages;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Casting;
using Genrpg.Shared.Pathfinding.Entities;
using Genrpg.Shared.Spells.Interfaces;
using MessagePack;
using Genrpg.Shared.Utils.Data;
using Newtonsoft.Json;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Rewards.Entities;

namespace Genrpg.Shared.Units.Entities
{
    /// <summary>
    /// Core effect
    /// </summary>
    [MessagePackObject]
    public class UnitEffect : IEffect
    {
        [Key(0)] public long EntityTypeId { get; set; }

        [Key(1)] public long Quantity { get; set; }

        [Key(2)] public long EntityId { get; set; }
    }

    // MessagePackIgnore
    public class Unit : MapObject
    {
        public long BaseStatAmount { get; set; }
        public long StatPct { get; set; }
        public long QualityTypeId { get; set; }

        public float CombatStartX { get; set; }
        public float CombatStartZ { get; set; }
        public float CombatStartRot { get; set; }

        public long SexTypeId { get; set; }
        public bool DidFailAIUpdate { get; set; }
        public bool UnitWasNotOk { get; set; }

        [JsonIgnore] public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

        private List<AttackerInfo> _attackers = new List<AttackerInfo>();

        public override bool IsUnit() { return true; }

        public float BaseSpeed { get; set; }

        public Regen RegenMessage;

        public StatGroup Stats = new StatGroup();

        public List<OldSpellProc> Procs;

        public List<CurrentProc> CurrentProcs;

        public List<RewardList> Loot;
        public List<RewardList> SkillLoot;

        private int _flags = 0;

        [JsonIgnore] public DateTime GlobalCooldownEnds { get; set; } = DateTime.UtcNow;

        public Unit(IRepositoryService repositoryService) : base(repositoryService) { }

            public bool HasStatusBits(long statusBits)
        {
            return StatusEffects != null &&
                StatusEffects.MatchAnyBits((int)statusBits);
        }

        public override void Dispose()
        {
            base.Dispose();
            _attackers.Clear();
            _attackers.Clear();
            Procs?.Clear();
            CurrentProcs?.Clear();
            Loot?.Clear();
            SkillLoot?.Clear();
            _dataDict.Clear();
        }

        virtual public string GetGroupId() { return null; }

        public void AddAttacker(string attackerId, string groupId)
        {
            AttackerInfo currAttacker = _attackers.FirstOrDefault(x => x.AttackerId == attackerId);
            if (currAttacker != null)
            {
                currAttacker.GroupId = groupId;
                return;
            }

            currAttacker = new AttackerInfo()
            {
                AttackerId = attackerId,
                GroupId = groupId,
            };

            _attackers.Add(currAttacker);
        }

        public void RemoveAttacker(string attackerId)
        {
            _attackers = _attackers.Where(x => x.AttackerId != attackerId).ToList();
            if (string.IsNullOrEmpty(TargetId) && _attackers.Count < 1)
            {
                RemoveFlag(UnitFlags.DidStartCombat);
            }
        }

        public void ClearAttackers(ILogService _logService)
        {
            _attackers.Clear();
            RemoveFlag(UnitFlags.DidStartCombat);
        }

        public List<AttackerInfo> GetAttackers()
        {
            return _attackers.ToList();
        }

        public AttackerInfo GetFirstAttacker()
        {
            return _attackers.FirstOrDefault(x => !string.IsNullOrEmpty(x.GroupId));
        }


        public float GetGlobalCooldown()
        {
            return SpellConstants.GlobalCooldownMS * (1 - Stats.Pct(StatTypes.Cooldown)) / 1000.0f;
        }

        public bool HasFlag(int flag)
        {
            return (_flags & flag) != 0;
        }

        public void AddFlag(int flag)
        {
            _flags |= flag;
        }

        public void RemoveFlag(int flag)
        {
            _flags &= ~flag;
        }

        public int GetFlags()
        {
            return _flags;
        }

        public CurrentProc GetCurrentProc(long spellTypeId)
        {
            if (CurrentProcs == null)
            {
                CurrentProcs = new List<CurrentProc>();
            }

            CurrentProc proc = CurrentProcs.FirstOrDefault(x => x.SpellTypeId == spellTypeId);
            if (proc == null)
            {
                proc = new CurrentProc() { SpellTypeId = spellTypeId, CooldownEnds = DateTime.UtcNow };
                CurrentProcs.Add(proc);
                SetDirty(true);
            }
            return proc;
        }


        public void AddProc(IOldSpellProc proc)
        {
            if (proc == null)
            {
                return;
            }

            OldSpellProc currProc = Procs.FirstOrDefault(x => x.SpellId == proc.SpellId);
            if (currProc != null)
            {
                return;
            }

            Procs.Add(OldSpellProc.CreateFrom(proc));
        }

        public virtual bool CanInteract(Unit otherUnit)
        {
            return FactionTypeId == otherUnit.FactionTypeId;

        }


        public bool IsFullImmune()
        {
            return false;
        }
    }
}
