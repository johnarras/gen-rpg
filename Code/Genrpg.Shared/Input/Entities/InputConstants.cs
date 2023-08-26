using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Input.Entities
{
    [MessagePackObject]
    public class InputConstants
    {
        public const int MinActionIndex = 1;
        public const int MaxActionIndex = 10;
        public static bool OkActionIndex(long index)
        {
            return index >= MinActionIndex && index <= MaxActionIndex;
        }
    }
}
