using MessagePack;
using Genrpg.Shared.Interfaces;
using System;

namespace Genrpg.Shared.ProcGen.Settings.Names
{

    [MessagePackObject]
    public class NameValue : IIdName
    {
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
    }


    [MessagePackObject]
    public class KeyValue
    {
        [Key(0)] public string Key { get; set; }
        [Key(1)] public string Val { get; set; }
    }
}
