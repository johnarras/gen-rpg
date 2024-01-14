using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using System.Collections.Generic;
using UnityEngine; // Needed
namespace Assets.Scripts.MapTerrain
{
    public class TerrainPatchData : IStringId
    {
        public string Id { get; set; }
        public string MapId { get; set; }

        public int MapVersion { get; set; }
        // X grid in map
        public int X { get; set; }
        // Y grid in map
        public int Y { get; set; }

        public Terrain terrain { get; set; }

        public TerrainData terrainData { get; set; }

        public object parentObject { get; set; }

        public List<long> FullZoneIdList { get; set; } = new List<long>();

        public List<long> MainZoneIdList { get; set; } = new List<long>();

        public byte[] DataBytes;

        public float[,] heights { get; set; }

        public float[,,] baseAlphas { get; set; } 

        public uint[,] mapObjects { get; set; }

        public ushort[,,] grassAmounts { get; set; }

        public byte[,] subZoneIds { get; set; } = new byte[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];

        public byte[,] mainZoneIds { get; set; } = new byte[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];

        public byte[,] overrideZoneScales { get; set; } = new byte[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];

        public bool HaveSetAlphamaps = false;

        public List<long> TerrainTextureIndexes { get; set; } = new List<long>();
        
        public string GetFilePath(GameState gs, bool duringSaveAndUpload)
        {
            string path = MapUtils.GetMapFolder(gs, MapId, MapVersion) + "TerrainX" + X.ToString("000") + "Y" + Y.ToString("000");
            if (duringSaveAndUpload)
            {
                path += "Temp";
            }
            return path;
        }

    }
}
