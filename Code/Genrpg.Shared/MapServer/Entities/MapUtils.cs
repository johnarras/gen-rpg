using MessagePack;
using Genrpg.Shared.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

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

    }
}
