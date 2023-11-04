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

        public float[,] heights { get; set; } = new float[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];

        public float[,,] baseAlphas { get; set; } = new float[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize, MapConstants.MaxTerrainIndex];

        public uint[,] mapObjects { get; set; } = new uint[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];

        public ushort[,,] grassAmounts { get; set; } = new ushort[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize, MapConstants.MaxGrass];

        public int[,] subZoneIds { get; set; } = new int[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];

        public int[,] mainZoneIds { get; set; } = new int[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];

        public float[,] overrideZoneScales { get; set; } = new float[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];

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
