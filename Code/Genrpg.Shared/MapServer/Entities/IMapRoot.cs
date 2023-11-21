using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapServer.Entities
{
    public interface IMapRoot : IStringId
    {
        string Name { get; set; }
        string Desc { get; set; }
        string Icon { get; set; }
        string Art { get; set; }
        int MinLevel { get; set; }
        int MaxLevel { get; set; }
        int BlockCount { get; set; }
        float ZoneSize { get; set; }
        long Seed { get; set; }
        int MapVersion { get; set; }
        int SpawnX { get; set; }
        int SpawnY { get; set; }
        long OverrideZoneId { get; set; }
        float OverrideZonePercent { get; set; }
    }
}
