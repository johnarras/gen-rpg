using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spawns.Entities
{
    [MessagePackObject]
    public class RollData
    {
        public long Level = 0;
        public long QualityTypeId = 0;
        public int Times = 1;
        public int Depth = 0;
        public double Scale = 1.0f;
        public long RewardSourceId = 0;
        public long EntityId = 0;
    }
}
