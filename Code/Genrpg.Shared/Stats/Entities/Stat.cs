using MessagePack;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading;

namespace Genrpg.Shared.Stats.Entities
{
    [MessagePackObject]
    public class Stat
    {
        [Key(0)] public long Id { get; set; }

        [Key(1)] public List<long> Vals { get; set; } = new List<long>();

        public long Get(int statCategory)
        {
            if (statCategory < 0 || statCategory >= StatCategory.Size)
            {
                return 0;
            }

            while (Vals.Count < StatCategory.Size)
            {
                Vals.Add(0);
            }
            return Vals[statCategory];
        }

        public void Add(int statCategory, long value)
        {
            Set(statCategory, Get(statCategory) + value);
        }

        public void Set(int statCategory, long value)
        {
            if (statCategory < 0 || statCategory >= StatCategory.Size)
            {
                return;
            }

            while (Vals.Count < StatCategory.Size)
            {
                Vals.Add(0);
            }

            if (statCategory == StatCategory.Curr && Id > StatConstants.MaxMutableStatTypeId)
            {
                Vals[statCategory] = GetMax();
            }
            else
            {
                Vals[statCategory] = value;
            }
        }

        public long GetMax()
        {
            return Get(StatCategory.Base) * (100 + Get(StatCategory.Pct)) / 100;
        }

        public void Reset()
        {
            Vals = new List<long>();
        }
    }
}