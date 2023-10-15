using Genrpg.Shared.Stats.Constants;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Stats.Entities
{
    [MessagePackObject]
    public class StatGroup
    {

        [Key(0)] public List<Stat> _stats { get; set; }

        public List<Stat> GetAllStats()
        {
            return _stats.ToList();
        }

        public StatGroup()
        {
            _stats = new List<Stat>();
        }

        public void ResetCurrent()
        {
            _stats.ForEach(x => x.Reset());
        }

        public long Curr(long statTypeId)
        {
            return GetStat(statTypeId).Get(StatCategories.Curr);
        }

        public long Max(long statTypeId)
        {
            return GetStat(statTypeId).GetMax();
        }

        public long Pct(long statTypeId)
        {
            return GetStat(statTypeId).Get(StatCategories.Pct);
        }

        public float ScaleDown(long statTypeId)
        {
            return 1;
        }

        protected Stat GetStat(long statTypeId)
        {
            Stat stat = _stats.FirstOrDefault(x => x.Id == statTypeId);
            if (stat == null)
            {
                lock (this)
                {
                    stat = _stats.FirstOrDefault(x => x.Id == statTypeId);
                    if (stat == null)
                    {
                        stat = new Stat() { Id = statTypeId };
                        List<Stat> newList = new List<Stat>(_stats);
                        newList.Add(stat);
                        _stats = newList;
                    }
                }
            }
            return stat;
        }

        public long Get(long statTypeId, int statCategory)
        {
            return GetStat(statTypeId).Get(statCategory);
        }

        public void Set(long statTypeId, int statCategory, long val)
        {
            GetStat(statTypeId).Set(statCategory, val);
        }

        public void Add(long statTypeId, int statCategory, long val)
        {
            Set(statTypeId, statCategory, Get(statTypeId, statCategory));
        }
    }
}
