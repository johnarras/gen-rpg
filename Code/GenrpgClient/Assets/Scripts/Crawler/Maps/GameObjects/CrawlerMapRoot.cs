using Genrpg.Shared.Crawler.Maps.Entities;
using Assets.Scripts.Dungeons;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Buildings;
using Assets.Scripts.Crawler.Maps.Services.Helpers;

namespace Assets.Scripts.Crawler.Maps.GameObjects
{
    public class CrawlerMapRoot : BaseBehaviour
    {

        public string MapId { get; set; }

        public Dictionary<string, ClientMapCell> Cells { get; set; } = new Dictionary<string, ClientMapCell>();

        public DungeonAssets DungeonAssets { get; set; }

        public DungeonMaterials DungeonMaterials { get; set; }

        public Material DoorMat { get; set; }

        public CityAssets CityAssets { get; set; }

        public ICrawlerMapTypeHelper MapTypeHelper { get; set; }

        public ClientMapCell GetCell(int x, int z)
        {

            string key = x + "." + z;

            if (Cells.TryGetValue(key, out ClientMapCell cell))
            {
                return cell;
            }

            x = (x + Map.Width) % Map.Width;
            z = (z + Map.Height) % Map.Height;

            cell = new ClientMapCell() { X = x, Z = z };

            Cells[key] = cell;
            return cell;
        }

        public CrawlerMap Map { get; set; }

        public float DrawX { get; set; }
        public float DrawZ { get; set; }
        public float DrawY { get; set; }
        public float DrawRot { get; set; }

        public void SetupFromMap(CrawlerMap map)
        {
            Map = map;

            int dataSize = map.Width * map.Height;

            foreach (MapCellDetail detail in map.Details)
            {
                GetCell(detail.X, detail.Z).Details.Add(detail);
            }
        }
    }
}
