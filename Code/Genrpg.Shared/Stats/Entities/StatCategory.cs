using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Stats.Entities
{
    [MessagePackObject]
    public class StatCategory
    {
        public const int Base = 0;
        public const int Pct = 1;
        public const int Curr = 2;
        public const int Size = 3;
    }
}
