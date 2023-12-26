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

namespace Genrpg.Shared.Units.Entities
{
    // MessagePackIgnore
    public class Unit : MapObject
    {
        public long BaseStatAmount { get; set; }
        public long StatPct { get; set; }
        public long QualityTypeId { get; set; }

        public float CombatStartX { get; set; }
        public float CombatStartZ { get; set; }
        public float CombatStartRot { get; set; }

        virtual public string GetGroupId() { return null; }

        private List<AttackerInfo> _attackers = new List<AttackerInfo>();

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
        }

        public void ClearAttackers()
        {
            _attackers.Clear();
        }

        public List<AttackerInfo> GetAttackers()
        {
            return _attackers.ToList();
        }

        public AttackerInfo GetFirstAttacker()
        {
            return _attackers.FirstOrDefault(x => !string.IsNullOrEmpty(x.GroupId));
        }

        public float BaseSpeed { get; set; }

        public override bool IsUnit() { return true; }

        
        public Regen RegenMessage;

        
        public StatGroup Stats { get; set; } = new StatGroup();

        public float GetScale() { return 1.0f; }

        
        public List<SpellProc> Procs;


        
        public List<CurrentProc> CurrentProcs;


        
        public DateTime GlobalCooldownEnds = DateTime.UtcNow;

        
        public List<ActiveSpellEffect> SpellEffects;

        public float GetGlobalCooldown(GameState gs)
        {
            return SpellConstants.GlobalCooldownMS * (1 - Stats.Pct(StatTypes.Cooldown)) / 1000.0f;
        }

        public Unit()
        {
            Stats = new StatGroup();
            _attackers = new List<AttackerInfo>();
        }

        public CurrentProc GetCurrentProc(long spellTypeId)
        {
            if (CurrentProcs == null)
            {
                CurrentProcs = new List<CurrentProc>();
                SetDirty(true);
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

        
        public List<SpawnResult> Loot;
        public List<SpawnResult> SkillLoot;

        private int _flags = 0;

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


        public void AddProc(GameState gs, ISpellProc proc)
        {
            if (proc == null)
            {
                return;
            }

            SpellProc currProc = Procs.FirstOrDefault(x => x.SpellId == proc.SpellId);
            if (currProc != null)
            {
                return;
            }

            Procs.Add(SpellProc.CreateFrom(proc));
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
                _dataDict[t] = (T)Activator.CreateInstance(typeof(T));
                return (T)_dataDict[t];
            }

            return default;
        }


        public virtual void Set<T>(T obj) where T : IUnitData
        {
            _dataDict[obj.GetType()] = obj;
        }

        public virtual void Delete<T>(IRepositorySystem repoSystem) where T : class, IUnitData, new() { }

        public virtual Dictionary<Type, IUnitData> GetAllData() { return new Dictionary<Type, IUnitData>(); }

        public virtual void SaveAll(IRepositorySystem repoSystem, bool saveClean)
        {
        }

        public bool IsFullImmune(GameState gs)
        {
            return false;
        }
    }
}
