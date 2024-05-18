using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Spawns.Entities;
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

        public List<UnitClass> Classes { get; set; } = new List<UnitClass>();

        private List<AttackerInfo> _attackers = new List<AttackerInfo>();

        public override bool IsUnit() { return true; }

        public float BaseSpeed { get; set; }

        public Regen RegenMessage;

        public StatGroup Stats = new StatGroup();

        public List<OldSpellProc> Procs;

        public List<CurrentProc> CurrentProcs;

        public List<SpawnResult> Loot;
        public List<SpawnResult> SkillLoot;

        private int _flags = 0;

        public DateTime GlobalCooldownEnds = DateTime.UtcNow;


        public Unit(IRepositoryService repositoryService) : base(repositoryService) { }


        public override void Dispose()
        {
            base.Dispose();
            Classes.Clear();
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
            if (_attackers.Count > 5)
            {
                _logService.Message("AttackerCount: " + _attackers.Count);
            }
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


        public float GetGlobalCooldown(GameState gs)
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


        public void AddProc(GameState gs, IOldSpellProc proc)
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

        public virtual bool CanInteract(GameState gs, Unit otherUnit)
        {
            return FactionTypeId == otherUnit.FactionTypeId;

        }

        protected Dictionary<Type, IUnitData> _dataDict = new Dictionary<Type, IUnitData>();

        public virtual T Get<T>() where T : class, IUnitData, new()
        {
            Type t = typeof(T);

            if (_dataDict.ContainsKey(t))
            {
                return (T)_dataDict[t];
            }

            if (!IsPlayer())
            {
                Set((T)Activator.CreateInstance(typeof(T)));
                return (T)_dataDict[t];
            }

            return default;
        }


        public virtual void Set<T>(T obj) where T : IUnitData
        {
            _dataDict[obj.GetType()] = obj;
            obj.SetRepo(_repoService);
        }

        public virtual void Delete<T>(IRepositoryService repoSystem) where T : class, IUnitData, new() { }

        public virtual Dictionary<Type, IUnitData> GetAllData() { return new Dictionary<Type, IUnitData>(); }

        public virtual void SaveData(bool saveAll)
        {
        }

        public bool IsFullImmune(GameState gs)
        {
            return false;
        }
    }
}
