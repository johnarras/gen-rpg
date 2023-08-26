using MessagePack;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.NPCs.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Stats.Messages;

namespace Genrpg.Shared.Units.Entities
{
    // MessagePackKeyOffset 50
    [MessagePackObject]
    public class Unit : MapObject
    {
        // Use these things to generate the spells this can cast.
        [Key(50)] public long NPCTypeId { get; set; }

        [Key(51)] public long BaseStatAmount { get; set; }
        [Key(52)] public long StatPct { get; set; }
        [Key(53)] public long QualityTypeId { get; set; }

        [Key(54)] public float CombatStartX { get; set; }
        [Key(55)] public float CombatStartZ { get; set; }
        [Key(56)] public float CombatStartRot { get; set; }


        private string _firstAttacker = null;
        private List<string> _attackers = null;
        public void AddAttacker(string unitId)
        {
            if (!_attackers.Contains(unitId))
            {
                _attackers.Add(unitId);
            }
            if (string.IsNullOrEmpty(_firstAttacker))
            {
                _firstAttacker = _attackers.FirstOrDefault();
            }
        }

        public void RemoveAttacker(string unitId)
        {
            if (_attackers.Contains(unitId))
            {
                _attackers.Remove(unitId);
            }

            if (_firstAttacker == unitId)
            {
                _firstAttacker = _attackers.FirstOrDefault();
            }
        }

        public void ClearAttackers()
        {
            _firstAttacker = null;
            _attackers.Clear();
        }

        public List<string> GetAttackers()
        {
            return _attackers.ToArray().ToList();
        }

        public string GetFirstAttacker()
        {
            return _firstAttacker;
        }

        public void ClearFirstAttacker()
        {
            _firstAttacker = "";
        }

        [Key(57)] public float BaseSpeed { get; set; }

        public override bool IsUnit() { return true; }

        [JsonIgnore]
        [IgnoreMember] public Regen RegenMessage;

        [Key(58)] public StatGroup Stats { get; set; }

        public float GetScale() { return 1.0f; }

        [JsonIgnore]
        [IgnoreMember] public NPCType NPCType;
        [JsonIgnore]
        [IgnoreMember] public NPCStatus NPCStatus;

        // NOT Stored in the cloud. Calculated during StatsService.CalcStats()

        [JsonIgnore]
        [IgnoreMember] public List<SpellProc> Procs;


        [JsonIgnore]
        [IgnoreMember] public List<CurrentProc> CurrentProcs;


        [JsonIgnore]
        [IgnoreMember] public DateTime GlobalCooldownEnds = DateTime.UtcNow;

        [JsonIgnore]
        [IgnoreMember] public List<SpellEffect> SpellEffects;

        public float GetGlobalCooldown(GameState gs)
        {
            return SpellConstants.GlobalCooldownMS * (1 - Stats.Pct(StatType.Cooldown)) / 1000.0f;
        }

        public Unit()
        {
            Stats = new StatGroup();
            _attackers = new List<string>();
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

        [JsonIgnore]
        [IgnoreMember] public List<SpawnResult> Loot;

        [JsonIgnore]
        [IgnoreMember] public List<SpawnResult> SkillLoot;

        [JsonIgnore]
        [IgnoreMember] public int Flags = 0;

        public bool HasFlag(int flag)
        {
            return (Flags & flag) != 0;
        }

        public void AddFlag(int flag)
        {
            Flags |= flag;
        }

        public void RemoveFlag(int flag)
        {
            Flags &= ~flag;
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

        protected Dictionary<Type, IUnitDataContainer> _dataDict = new Dictionary<Type, IUnitDataContainer>();

        public virtual T Get<T>() where T : class, IUnitData, new()
        {
            Type t = typeof(T);

            if (_dataDict.ContainsKey(t))
            {
                return (T)_dataDict[t].GetData();
            }

            if (!IsPlayer())
            {
                _dataDict[t] = new UnitDataContainer<T>((T)Activator.CreateInstance(typeof(T)));
                return (T)_dataDict[t].GetData();
            }

            return default;
        }


        public virtual void Set<T>(T obj) where T : class, IUnitData, new()
        {
            _dataDict[typeof(T)] = new UnitDataContainer<T>(obj);
        }

        public virtual void Delete<T>(IRepositorySystem repoSystem) where T : class, IUnitData, new() { }

        public virtual Dictionary<Type, IUnitDataContainer> GetAllData() { return new Dictionary<Type, IUnitDataContainer>(); }

        public virtual void SaveAll(IRepositorySystem repoSystem, bool saveClean)
        {
        }

        public bool IsFullImmune(GameState gs)
        {
            return false;
        }
    }
}
