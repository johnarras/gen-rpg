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
        public static string GetMapFolder(GameState gs, string MapId, long MapVersion)
        {
            return "Map" + MapId + "/V" + MapVersion.ToString("0000") + "/";
        }
        public static string GetMapObjectFilename(GameState gs, string filename, string mapId, int mapVersion)
        {
            return GetMapFolder(gs, mapId, mapVersion) + filename;
        }

    }
}
