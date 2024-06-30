using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Genrpg.Shared.Dungeons.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace Assets.Scripts.Crawler.Maps.Entities
{
    public class CrawlerMap
    {
        public long IdKey { get; set; }
        public string Name { get; set; }
        public ECrawlerMapTypes MapType { get; set; } = ECrawlerMapTypes.Dungeon;
        public long DungeonArtId { get; set; } = 1;
        public int Width { get; set; }
        public int Height { get; set; }
        public int Level { get; set; }
        public bool Looping { get; set; }
        public long ZoneTypeId { get; set; }
        public byte[] CoreData { get; set; } // Walls or terrain
        public byte[] ExtraData { get; set; } // Effects or objects on map
        public List<MapCellDetail> Details = new List<MapCellDetail>();
        public DungeonArt DungeonArt { get; set; }



        public void SetupDataBlocks()
        {
            CoreData = new byte[Width * Height];
            ExtraData = new byte[Width * Height];
        }

        public int GetIndex(int x, int z)
        {
            return z * Width + x;
        }
    }
}
