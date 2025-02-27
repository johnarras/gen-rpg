using MessagePack;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{
    [MessagePackObject]
    public class CurrentMapStatus
    {
        [Key(0)] public SmallIndexBitList Visited { get; set; } = new SmallIndexBitList();
        [Key(1)] public SmallIndexBitList Cleansed { get; set; } = new SmallIndexBitList();


        public void Clear()
        {
            Visited.Clear();
            Cleansed.Clear();
        }
    }
}
