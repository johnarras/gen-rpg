using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Utils.Data
{
    [MessagePackObject]
    public class StringPair
    {
        [Key(0)] public string First { get; set; }
        [Key(1)] public string Second { get; set; }
    }
}
