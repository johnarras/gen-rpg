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
        const int Max = 2;
        const int Size = 3;    

        [Key(0)] public int[] Dat { get; set; } = new int[Size];

        public void SetData(long statTypeId, long currVal, long maxVal)
        {
            Dat[StatTypeId] = (int)statTypeId;
            Dat[Curr] = (int)currVal;
            Dat[Max] = (int)maxVal;
        }

        public long GetStatId() { return Dat[StatTypeId]; }

        public long GetCurr() { return Dat[Curr]; }

        public long GetMax() { return Dat[Max]; }
    }
}
