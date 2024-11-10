using MessagePack;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Dungeons.Settings;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Dungeons.Constants;
using Genrpg.Shared.Characters.PlayerData;

namespace Genrpg.Shared.Crawler.Maps.Entities
{
    public class CellIndex
    {
        public const int Building = 0;
        public const int Dir = 1;
        public const int Walls = 2;
        public const int Terrain = 3;
        public const int Region = 4;
        public const int Tree = 5;
        public const int Magic = 6;
        public const int Encounter = 7;
        public const int Disables = 8;
        public const int Unused4 = 9;
        public const int Unused5 = 10;
        public const int Unused6 = 11;
        public const int Unused7 = 12;
        public const int Unused8 = 13;
        public const int Unused9 = 14;
        public const int Unused10 = 15;
        public const int Unused11 = 16;
        public const int Max = 16;
    }

    [MessagePackObject]
    public class CrawlerMap : IStringId
    {
        [Key(0)] public string Id { get; set; }
        [Key(1)] public long IdKey { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public List<ZoneRegion> Regions { get; set; } = null;
        [Key(4)] public long CrawlerMapTypeId { get; set; } = CrawlerMapTypes.Dungeon;
        [Key(5)] public int Width { get; set; }
        [Key(6)] public int Height { get; set; }
        [Key(7)] public int Level { get; set; }
        [Key(8)] public bool Looping { get; set; }
        [Key(9)] public long MapFloor { get; set; }
        [Key(10)] public string FromPlaceName { get; set; }
        [Key(11)] public long MapQuestItemId { get; set; }
        [Key(12)] public string RiddleText { get; set; }
        [Key(13)] public string RiddleAnswer { get; set; }
        [Key(14)] public string RiddleError { get; set; }
        [Key(15)] public byte[] Data { get; set; }
        public List<MapCellDetail> Details = new List<MapCellDetail>();

        public void SetupDataBlocks()
        {
            Data = new byte[Width * Height * CellIndex.Max];
        }

        public byte Get(int x, int z, int offset)
        {
            return Data[GetDataIndex(x, z, offset)];
        }

        public void Set(int x, int z, int offset, long value)
        {
            Data[GetDataIndex(x, z, offset)] = (byte)value;
        }

        public void AddBits(int x, int z, int offset, long value)
        {
            Data[GetDataIndex(x, z, offset)] |= (byte)value;
        }

        protected int GetDataIndex(int x, int z, int offset)
        {
            return offset * Width * Height + z * Width + x;
        }

        public int GetIndex(int x, int z)
        {
            return z * Width + x;
        }


        private string _floorName = null;
        public string GetName(int x, int z)
        {
            if (MapFloor > 1)
            {

                if (string.IsNullOrEmpty(_floorName))
                {
                    _floorName = Name + " (Level " + MapFloor + ")";
                }
                return _floorName;
            }

            if (Regions == null || Regions.Count < 1)
            {
                return Name;
            }

            byte ztype = Get(x, z, CellIndex.Region);

            ZoneRegion region = Regions.FirstOrDefault(x => x.ZoneTypeId == ztype);
            if (region != null)
            {
                return region.Name;
            }
            return Name;
        }

        public byte EastWall(int x, int y)
        {
            return (byte)((Get(x, y, CellIndex.Walls) >> MapWallBits.EWallStart) % (1 << MapWallBits.WallBitSize));
        }

        public byte NorthWall(int x, int y)
        {
            return (byte)((Get(x, y, CellIndex.Walls) >> MapWallBits.NWallStart) % (1 << MapWallBits.WallBitSize));
        }

    }
}
