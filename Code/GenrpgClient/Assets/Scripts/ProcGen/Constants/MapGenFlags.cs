using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;

public class MapGenFlags
{
    public const int CheckedAdjacencies = 1 << 0;
    public const int IsEdgeWall = 1 << 1;
    public const int IsWallCenter = 1 << 2;
    public const int IsRaisedOrLowered = 1 << 4;
    public const int IsEscarpment = 1 << 5;
    public const int IsSecondaryWall = 1 << 6;
    public const int NearResourceNode = 1 << 7;
    public const int OverrideWallNoiseScale = 1 << 8;
    public const int NearWater = 1 << 9;
    public const int BelowWater = 1 << 10;
    public const int IsLocation = 1 << 11;
    public const int VeryCurvedRoad = 1 << 12;
    public const int MinorRoad = 1 << 13;
    public const int IsLocationPatch = 1 << 14;
}

