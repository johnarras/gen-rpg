using MessagePack;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.ProcGen.Entities
{
    /// <summary>
    /// Use this to determine what items to conenct in a graph.
    /// </summary>
    [MessagePackObject]
    public class ConnectPointData
    {
        public long Id;
        public object Data;
        public double X;
        public double Z;
        public int MaxConnections = 3;
        public int ConnectSet = 0;

        public double MinDistToOther = 10000000;

        public List<ConnectPointData> Adjacencies;

        public ConnectPointData()
        {
            Adjacencies = new List<ConnectPointData>();
        }

        public double DistanceTo(ConnectPointData other)
        {
            if (other == null)
            {
                return 100000000L;
            }

            double dx = X - other.X;
            double dz = Z - other.Z;
            return Math.Sqrt(dx * dx + dz * dz);
        }

    }
}
