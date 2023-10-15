using Genrpg.Shared.Stats.Constants;
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

        [Key(1)] public long[] Vals = new long[StatCategories.Size];

        public long Get(int statCategory)
        {
            if (statCategory < 0 || statCategory >= StatCategories.Size)
            {
                return 0;
            }

            return Vals[statCategory];
        }

        public void Add(int statCategory, long value)
        {
            Set(statCategory, Get(statCategory) + value);
        }

        public void Set(int statCategory, long value)
        {
            if (statCategory < 0 || statCategory >= StatCategories.Size)
            {
                return;
            }

            if (statCategory == StatCategories.Curr && Id > StatConstants.MaxMutableStatTypeId)
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
            return Get(StatCategories.Base) * (100 + Get(StatCategories.Pct)) / 100;
        }

        public void Reset()
        {
            Vals = new long[StatCategories.Size];
        }
    }
}
