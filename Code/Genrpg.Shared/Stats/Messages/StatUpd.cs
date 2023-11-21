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

        [Key(1)] public List<FullStat> Dat { get; set; }

        public StatUpd()
        {
            Dat = new List<FullStat>();
        }

        public void AddFullStat(FullStat fullStat)
        {
            if (fullStat != null)
            {
                Dat.Add(fullStat);
            }
        }
    }

    [MessagePackObject]
    public class FullStat
    {
        const int StatTypeId = 0;
        const int Curr = 1;
        const int Base = 2;
        const int Pct = 3;
        const int Size = 4;

        [Key(0)] public int[] Dat { get; set; } = new int[Size];


        public void SetData(long statTypeId, long currVal, long baseVal, long pctVal)
        {
            Dat[StatTypeId] = (int)statTypeId;
            Dat[Curr] = (int)currVal;
            Dat[Base] = (int)baseVal;
            Dat[Pct] = (int)pctVal;
        }

        public long GetStatId() { return Dat[StatTypeId]; }

        public long GetCurr() { return Dat[Curr]; }

        public long GetBase() { return Dat[Base]; }

        public long GetPct() { return Dat[Pct]; }

        public long GetMax()
        {
            return GetBase() * (100 + GetPct()) / 100;
        }

    }
}
