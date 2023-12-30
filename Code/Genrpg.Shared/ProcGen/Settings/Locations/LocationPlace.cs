using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Settings.Locations
{
    [MessagePackObject]
    public class LocationPlace
    {
        [Key(0)] public string Id { get; set; }
        [Key(1)] public string LocationId { get; set; }
        [Key(2)] public string NPCObjId { get; set; }
        [Key(3)] public string BuildingObjId { get; set; }
        [Key(4)] public int CenterX { get; set; }
        [Key(5)] public int CenterZ { get; set; }
        [Key(6)] public int EntranceX { get; set; }
        [Key(7)] public int EntranceZ { get; set; }
        [Key(8)] public int XSize { get; set; }
        [Key(9)] public int ZSize { get; set; }

    }
}
