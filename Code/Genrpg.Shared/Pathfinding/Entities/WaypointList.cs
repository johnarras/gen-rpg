using Genrpg.Shared.Pathfinding.Constants;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Pathfinding.Entities
{
    [MessagePackObject]
    public class WaypointList
    {
        [Key(0)] public string RetvalType { get; set; }
        [Key(1)] public List<Waypoint> Waypoints { get; set; } = new List<Waypoint>();

        public void RemoveWaypointAt(int index)
        {
            if (index < 0 || index >= Waypoints.Count)
            {
                return;
            }
            Waypoint wp = Waypoints[index];
            Waypoints.RemoveAt(index);
            _waypointCache.Push(wp);
        }

        private Stack<Waypoint> _waypointCache = new Stack<Waypoint>(); 

        public void AddGridCell(int gridx, int gridz)
        {
            if (_waypointCache.TryPop(out Waypoint wp))
            {
                wp.X = gridx * PathfindingConstants.BlockSize;
                wp.Z = gridz * PathfindingConstants.BlockSize ;
                Waypoints.Add(wp);
            }
            else
            {
                Waypoints.Add(new Waypoint() { X = gridx * PathfindingConstants.BlockSize, Z = gridz * PathfindingConstants.BlockSize });
            }
        }

        public void Clear()
        {
            foreach (Waypoint wp in Waypoints)
            {
                _waypointCache.Push(wp);
            }
            Waypoints.Clear();
        }

    }
}
