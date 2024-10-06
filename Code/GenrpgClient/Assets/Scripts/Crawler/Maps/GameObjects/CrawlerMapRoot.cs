using Genrpg.Shared.Crawler.Maps.Entities;
using Assets.Scripts.Dungeons;
using System.Collections.Generic;

namespace Assets.Scripts.Crawler.Maps.GameObjects
{
    public class CrawlerMapRoot : BaseBehaviour
    {

        public string MapId { get; set; }

        public Dictionary<long, ClientMapCell> Cells { get; set; } = new Dictionary<long, ClientMapCell>();

        public DungeonAssets Assets { get; set; }

        public ClientMapCell GetCell(int x, int y)
        {
            int index = Map.GetIndex(x, y);

            if (Cells.TryGetValue(index, out ClientMapCell cell))
            {
                return cell;
            }

            cell = new ClientMapCell() { X = x, Z = y };

            Cells[index] = cell;
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
