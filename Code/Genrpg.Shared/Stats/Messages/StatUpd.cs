using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Stats.Messages
{
    [MessagePackObject]
    public sealed class StatUpd : BaseMapApiMessage
    {
        [Key(0)] public string UnitId { get; set; }

        [Key(1)] public List<SmallStat> Dat { get; set; }

        public StatUpd()
        {
            Dat = new List<SmallStat>();
        }

        public void AddStat(long statId, long currVal, long maxVal)
        {
            SmallStat smallStat = new SmallStat();
            smallStat.SetStatId(statId);
            smallStat.SetCurrVal(currVal);
            smallStat.SetMaxVal(maxVal);
            Dat.Add(smallStat);
        }
    }

    [MessagePackObject]
    public class SmallStat
    {
        const int StatId = 0;
        const int CurrVal = 1;
        const int MaxVal = 2;
        const int Size = 3;

        [Key(0)] public int[] Dat { get; set; } = new int[Size];

        public long GetStatId() { return Dat[StatId]; }
        public void SetStatId(long id) { Dat[StatId] = (int)id;}

        public long GetCurrVal() { return Dat[CurrVal]; }
        public void SetCurrVal(long curr) { Dat[CurrVal] = (int)curr;}

        public long GetMaxVal() { return Dat[MaxVal]; }
        public void SetMaxVal(long maxVal) { Dat[MaxVal] = (int)maxVal; }

        public SmallStat()
        {
        }

    }
}
