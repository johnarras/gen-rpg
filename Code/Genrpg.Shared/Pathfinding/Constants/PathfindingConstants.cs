using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Pathfinding.Constants
{
    [MessagePackObject]
    public class PathfindingConstants
    {
        public const string Filename = "Pathfinding";
        public const float MaxSteepness = 60;

    }
}
