using Genrpg.Shared.Pathfinding.Constants;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Pathfinding.Entities
{
    [MessagePackObject]
    public class WaypointList : IDisposable
    {
        [Key(0)] public string RetvalType { get; set; }
        [Key(1)] public List<Waypoint> Waypoints { get; set; } = new List<Waypoint>();

        public void AddGridCell(int gridx, int gridz)
        {
            Waypoints.Add(new Waypoint() { X=gridx*PathfindingConstants.BlockSize, Z = gridz*PathfindingConstants.BlockSize });
        }

        public void AddWorldLoc(float worldX, float worldZ)
        {
            Waypoints.Add(new Waypoint() { X = (int)worldX, Z = (int)worldZ });
        }

        public void Dispose()
        {
            Waypoints.Clear();
            Waypoints = null;
        }
    }
}
