﻿using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Dungeons;
using System.Collections.Generic;
using System.Security.Policy;
using GEntity = UnityEngine.GameObject;

namespace Assets.Scripts.Crawler.Maps.GameObjects
{
    public class CrawlerMapRoot : BaseBehaviour
    {

        public string MapId { get; set; }

        public Dictionary<long, UnityMapCell> Cells { get; set; } = new Dictionary<long, UnityMapCell>();

        public DungeonAssets Assets { get; set; }

        public UnityMapCell GetCell(int x, int y)
        {
            int index = Map.GetIndex(x, y);

            if (Cells.TryGetValue(index, out UnityMapCell cell))
            {
                return cell;
            }

            cell = new UnityMapCell() { X = x, Z = y };

            Cells[index] = cell;
            return cell;
        }

        public CrawlerMap Map { get; set; }

        public GEntity ContentRoot { get; set; }

        public float DrawX { get; set; }
        public float DrawZ { get; set; }
        public float DrawY { get; set; }
        public float DrawRot { get; set; }

        public void SetupFromMap(CrawlerMap map)
        {
            Map = map;

            int dataSize = map.XSize * map.ZSize;

            foreach (MapCellDetail detail in map.Details)
            {
                GetCell(detail.X, detail.Z).Details.Add(detail);
            }
        }


        public byte EastWall(int x, int y)
        {
            
            return (byte)((Map.CoreData[Map.GetIndex(x,y)] >> DungeonCoreMapCellBits.EWallStart) % (1 << DungeonCoreMapCellBits.WallBitSize));
        }

        public byte NorthWall(int x, int y)
        {
            return (byte)((Map.CoreData[Map.GetIndex(x, y)] >> DungeonCoreMapCellBits.NWallStart) % (1 << DungeonCoreMapCellBits.WallBitSize));
        }

    }
}