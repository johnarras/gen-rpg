using Genrpg.Shared.MapServer.Constants;
using Genrpg.Shared.MapServer.Entities;

public class MapConstants : SharedMapConstants
{
    public const int TerrainBytesPerUnit = 12;


    public const string MapFilename = "MapMap.jpg";

    public const int LocCenterEdgeSize = TerrainPatchSize / 2;
    public const float TerrainBlockVisibilityRadius = 2.7f; //4.0f;

    public const int MountainZoneId = 1;
    public const int OceanZoneId = 2;

    public const float MaxMouseRaycastDistance = TerrainPatchSize;
    public const float MaxInteractDistance = 8.0f;

    public const int AlphaMapsPerTerrainCell = 1;

    public const int InitialRoadDistance = MaxRoadCheckDistance * 4 / 3;
    public const int InitialMountainDistance = 1000;

    public const int MaxRoadCheckDistance = 80;
    public const int RoadBaseHillScaleDistance = MaxRoadCheckDistance;

    public const int BaseTerrainIndex = 0;
    public const int DirtTerrainIndex = 1;
    public const int RoadTerrainIndex = 2;
    public const int SteepTerrainIndex = 3;
    public const int MaxTerrainIndex = 4;

    public const int MonsterSpawnSkipSize = 30;

    public const int MaxBridgeCheckDistance = 40;

    public const int MinResourceSeparation = 50;


    public const int ExtraSplatChannels = 0;

    public const int RoadSmoothMinRadius = 3;
    public const int RoadSmoothMaxRadius = 5;

    public const int MaxTerrainGridSize = 84;
    public const int ZoneCenterSkipSize = (TerrainPatchSize - 1) * 2;
    public const float MountainZeroHeightChance = 0.07f;
    public const float MountainRandomHeightChance = 0.85f;
    public const int MapEdgeSize = TerrainPatchSize / 4;

    public const float MountainHeightMult = 1.0f;


    public const byte AlphaSaveMult = 255;
    public const ushort HeightSaveMult = 1 << 15;
    public const float MaxOverrideTreeTypeChance = 0.10f;

    public const float RoadDipHeight = 0.00175f;
    public const float MaxRoadDipPercent = 0.80f;

    public const int BridgeMaxAngle = 180;
    public const int BridgeAngleDiv = 3;
    public const int BridgeAngleMod = BridgeMaxAngle / BridgeAngleDiv;

    // Shift for encoding bridge height into map data. 
    // Set this up so that 1 << BridgeHEightBitShift is > BridgeAngleMod...  
    // so the lowest BitShift bits are for the angle and the rest are for the
    // height.
    public const int BridgeHeightBitShift = 6;


    public const float MapHeightPerGrid = 0.015f;

    public const int MapObjectOffsetMult = 2000;
    public const int TreeObjectOffset = 0;
    public const int RockObjectOffset = TreeObjectOffset + MapObjectOffsetMult;
    public const int FenceObjectOffset = RockObjectOffset + MapObjectOffsetMult;
    public const int BridgeObjectOffset = FenceObjectOffset + MapObjectOffsetMult;
    public const int UnitObjectOffset = BridgeObjectOffset + MapObjectOffsetMult;
    public const int DungeonObjectOffset = UnitObjectOffset + MapObjectOffsetMult;
    public const int ClutterObjectOffset = DungeonObjectOffset + MapObjectOffsetMult;
    public const int GroundObjectOffset = ClutterObjectOffset + MapObjectOffsetMult;
    public const int NPCObjectOffset = GroundObjectOffset + MapObjectOffsetMult;
    public const int WaterObjectOffset = NPCObjectOffset + MapObjectOffsetMult;
    public const int GrassMinCellValue = 60000;
    public const int GrassMaxCellValue = GrassMinCellValue + 5000;

    public const float TerrainLayerTileSize = 4.0f;
    public const float TerrainLayerOffset = 0.0f;

    public const float OffsetHeightMult = 2.0f;

    public const float DefaultCreviceDepth = 12.0f;

    public const int DefaultNoiseSize = 513;

    public const int GrassResolutionDiv = 1;
    public const int MaxGrassValue = 7;

    public const int MaxGrass = 4;
    public const int OverrideMaxGrass = 2;

    public const float PrefabPlantDensityScale = 0.20f;

    // Location gen data

    public const int LocationSmallSize = 7;
    public const int LocationMidSize = 12;
    public const int LocationLargeSize = 20;


    public const int OverrideZoneScaleMax = 100;

    /// <summary>
    ///  This MUST be a power of 2 + 1... so 65, 129, 257, 513, 1025
    /// </summary>

    public const int MapHeight = 2000;
    public const int MinSteepnessForTexture = 20;
    public const float StartHeightPercent = 0.2f;
    public const float MinLandHeight = MapHeight * StartHeightPercent;
    public const float OceanHeight = MinLandHeight - 50;
    public const float MinDungeonHeight = OceanHeight + 125;
    public const float RoadBorderBaseDirtPercent = 0.40f;

    public const int DetailResolutionPerPatch = 16;
    public const int DetailResolution = (TerrainPatchSize - 1) / GrassResolutionDiv;

    public const float MinEdgeMountainChance = 0.9f;

    public const float MaxTreeBumpHeight = 2.5f;

    public const int MinMountainWidth = TerrainPatchSize / 2;
    public const int MaxMountainWidth = TerrainPatchSize;
    public const float MountainWidthDivisor = 9.0f;

    public const string TerrainCacheName = "TerrainCache";
    public const string TerrainTextureRoot = "TerrainTextureRoot";


    public const string WaterName = "MapWater";
    public const string MinimapWaterName = "MinimapWater";
    public const string FullMinimapWaterName = "MinimapWaterFull";
    public const string KillColliderName = "KillCollider";
}