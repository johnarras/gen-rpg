using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Messages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;

namespace Genrpg.Shared.Stats.Entities
{
    [MessagePackObject]
    public class StatGroup
    {

        private Dictionary<short,Stat>[] _stats = null;

        public List<Stat> GetAllBaseStats()
        {
            return _stats[StatCategories.Base].Values.ToList();
        }

        public StatGroup()
        {
            ResetCurrent();
        }

        public void ResetCurrent()
        {
            Dictionary<short, Stat>[] statCopy = new Dictionary<short,Stat>[StatCategories.Size];
            for (int i = 0; i < 3; i++)
            {
                statCopy[i] = new Dictionary<short, Stat>();
            }
            _stats = statCopy;
        }

        public int Get(long statTypeId, int statCategory)
        {
            if (_stats[statCategory].TryGetValue((short)statTypeId, out Stat stat))
            {
                return stat.Val;
            }

            if (statCategory == StatCategories.Curr)
            {
                return Max(statTypeId);
            }

            return 0;
        }

        public void Set(long statTypeId, long statCategory, long val)
        {

            if (!_stats[statCategory].TryGetValue((short)statTypeId, out Stat stat))
            {
                lock (this)
                {
                    if (!_stats[statCategory].TryGetValue((short)statTypeId, out Stat stat2))
                    {
                        stat = new Stat() { Id = (short)statTypeId };
                        Dictionary<short, Stat> newList = new Dictionary<short, Stat>(_stats[statCategory]);
                        newList[(short)statTypeId] = stat;
                        _stats[statCategory] = newList;
                    }
                    else
                    {
                        stat = stat2;
                    }
                }
            }
            stat.Val = (int)val;
        }

        public int Curr(long statTypeId) { return Get(statTypeId, StatCategories.Curr); }
        public void SetCurr(long statTypeId, long val) { Set(statTypeId, StatCategories.Curr, val); }

        public int Pct(long statTypeId) { return Get(statTypeId, StatCategories.Pct); }
        public void SetPct(long statTypeId, long val) { Set(statTypeId, StatCategories.Pct, val); }

        public int Base(long statTypeId) { return Get(statTypeId, StatCategories.Base); }
        public void SetBase(long statTypeId, long val) { Set(statTypeId, StatCategories.Base, val); }

        public int Max(long statTypeId)
        {
            int baseVal = Base(statTypeId);
            if (baseVal > 0)
            {
                int pctVal = Pct(statTypeId);

                return baseVal * (100 + pctVal) / 100;
            }
            return 0;
        }

        public float ScaleDown(long statTypeId)
        {
            return 1;
        }

        public List<FullStat> GetSnapshot()
        {
            List<short> allStatTypes = _stats[StatCategories.Base].Keys.ToList();

            List<FullStat> retval = new List<FullStat>();

            foreach (short statTypeId in allStatTypes)
            {

                FullStat fullStat = GetFullStat(statTypeId);

                if (fullStat != null)
                {
                    retval.Add(fullStat);
                }
            }
            return retval;
        }

        public void UpdateFromSnapshot(List<FullStat> fullStats)
        {
            if (fullStats == null)
            {
                return;
            }

            foreach (FullStat fullStat in fullStats)
            {
                SetBase(fullStat.GetStatId(), fullStat.GetBase());
                SetPct(fullStat.GetStatId(), fullStat.GetPct());
                SetCurr(fullStat.GetStatId(), fullStat.GetCurr());
            }
        }

        public FullStat GetFullStat(long statTypeId)
        {

            int baseVal = Base(statTypeId);

            if (baseVal > 0)
            {
                int currVal = Curr(statTypeId);
                int pctVal = Pct(statTypeId);

                FullStat smallStat = new FullStat();
                smallStat.SetData(statTypeId, currVal, baseVal, pctVal);
                return smallStat;
            }
            return null;
        }

    }
}
