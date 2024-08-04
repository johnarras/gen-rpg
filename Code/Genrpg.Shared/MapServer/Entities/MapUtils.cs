using MessagePack;
using Genrpg.Shared.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapServer.Constants;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;

namespace Genrpg.Shared.MapServer.Entities
{
    [MessagePackObject]
    public class MapUtils
    {
        public static string GetMapFolder(string MapId, long MapVersion)
        {
            return "Map" + MapId + "/V" + MapVersion.ToString("0000") + "/";
        }
        public static string GetMapObjectFilename(string filename, string mapId, int mapVersion)
        {
            return GetMapFolder(mapId, mapVersion) + filename;
        }

        public static string DirFromAngle(long angle)
        {
            while (angle < 0)
            {
                angle += 360;
            }
            while (angle >= 360)
            {
                angle -= 360;
            }

            if (angle == 0)
            {
                return "E";
            }
            else if (angle == 90)
            {
                return "S";
            }
            else if (angle == 180)
            {
                return "W";
            }
            else if (angle == 270)
            {
                return "N";
            }
            return "?";
        }

        public static int GetGridIndexFromCoord(double mapPos, int gridSize, bool useCeiling)
        {
            if (!useCeiling)
            {
                return MathUtils.Clamp(0, (int)(mapPos / SharedMapConstants.MapObjectGridSize), gridSize - 1);
            }
            else
            {
                return MathUtils.Clamp(0, (int)Math.Ceiling(mapPos / SharedMapConstants.MapObjectGridSize), gridSize - 1);
            }
        }

        public static PointXZ GetGridCoordinates(double x, double z, int gridSize)
        {
            return new PointXZ(GetGridIndexFromCoord(x, gridSize, false), GetGridIndexFromCoord(z, gridSize, false));
        }

        public static int GetMapObjectGridSize(Map map)
        {
            return (int)Math.Ceiling(1.0 * map.GetMapSize() / SharedMapConstants.MapObjectGridSize);
        }
    }  
}
