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
        private Stat[][] _stats = null;

        public Stat[] GetAllBaseStats()
        {
            return _stats[StatCategories.Base];
        }
        public StatGroup()
        {
            ResetAll();
        }

        public void ResetAll()
        {
            Stat[][] statCopy = new Stat[StatCategories.Size][];
            for (int i = 0; i < StatCategories.Size; i++)
            {
                statCopy[i] = new Stat[StatTypes.Max];
            }
            _stats = statCopy;
        }

        public int Get(long statTypeId, int statCategory)
        {
            return _stats[statCategory][statTypeId].Val;
        }

        public void Set(long statTypeId, long statCategory, long val)
        {
            _stats[statCategory][statTypeId].Val = (int)val;

        }

        public int Curr(long statTypeId) { return Get(statTypeId, StatCategories.Curr); }
        public void SetCurr(long statTypeId, long val) { Set(statTypeId, StatCategories.Curr, val); }

        public int Pct(long statTypeId) { return Get(statTypeId, StatCategories.Pct); }
        public void SetPct(long statTypeId, long val) { Set(statTypeId, StatCategories.Pct, val); }

        public int Base(long statTypeId) { return Get(statTypeId, StatCategories.Base); }
        public void SetBase(long statTypeId, long val) { Set(statTypeId, StatCategories.Base, val); }

        public int Bonus(long statTypeId) { return Get(statTypeId, StatCategories.Bonus); }
        public void SetBonus(long statTypeId, long val) { Set(statTypeId, StatCategories.Bonus, val); }

        public int Max(long statTypeId)
        {
            int baseVal = Base(statTypeId) + Bonus(statTypeId);
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
            List<FullStat> retval = new List<FullStat>();

            for (int statTypeId = 1; statTypeId < StatTypes.Max; statTypeId++)
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
                SetBase(fullStat.GetStatId(), fullStat.GetMax());
                SetCurr(fullStat.GetStatId(), fullStat.GetCurr());
            }
        }

        public FullStat GetFullStat(long statTypeId)
        {

            int maxVal = Max(statTypeId);

            if (maxVal > 0)
            {
                FullStat smallStat = new FullStat();
                smallStat.SetData(statTypeId, Curr(statTypeId), Max(statTypeId));
                return smallStat;
            }
            return null;
        }

    }
}
