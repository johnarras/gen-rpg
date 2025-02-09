using Assets.Scripts.Dungeons;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.GameObjects
{

    public class DungeonAssetPosition
    {
        public const int None = 0;
        public const int NorthWall = 1;
        public const int EastWall = 2;
        public const int Ceiling = 4;
        public const int Floor = 5;
        public const int Pillar = 6;
        public const int NorthUpper = 7;
        public const int EastUpper = 8;
        public const int Max = 9;
    }


    public class ClientMapCell
    {
        public int X { get; set; }
        public int Z { get; set; }
        public List<MapCellDetail> Details { get; set; } = new List<MapCellDetail>();
        public object Content { get; set; }

        public DungeonAsset[] AssetPositions { get; set; } = new DungeonAsset[DungeonAssetPosition.Max];

    }

}
