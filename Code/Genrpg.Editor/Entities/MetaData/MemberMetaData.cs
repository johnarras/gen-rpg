using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Editor.Entities.MetaData
{
    [MessagePackObject]
    public class MemberMetaData
    {
        [Key(0)] public string MemberName { get; set; }
        [Key(1)] public string Description { get; set; }
        [Key(2)] public string MinValue { get; set; }
        [Key(3)] public string MaxValue { get; set; }
        [Key(4)] public string AcceptableValues { get; set; }
    }
}
