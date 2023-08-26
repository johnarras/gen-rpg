using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapObjects.Entities
{
    [MessagePackObject]
    public class MapObjectUtils
    {
        public static float DistanceBetween(MapObject obj1, MapObject obj2)
        {
            float dx = obj1.X - obj2.X;
            float dz = obj1.Z - obj2.Z;

            return (float)Math.Sqrt(dx * dx + dz * dz);
        }
    }
}
