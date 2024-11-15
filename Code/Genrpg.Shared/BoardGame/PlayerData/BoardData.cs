using MessagePack;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.BoardGame.Constants;
namespace Genrpg.Shared.BoardGame.PlayerData
{
    public class DataArray
    {
        public short[] Data = new short[0];

        public long this[int i]
        {
            get { return Data[i]; }
            set { Data[i] = (short)value; }
        }

        public int Length
        {
            get { return Data.Length; }
        }

        public void Create(int length)
        {
            Data = new short[length];
        }
    }


    [MessagePackObject]
    public class BoardData : NoChildPlayerData, IUserData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string OwnerId { get; set; }
        [Key(2)] public long BoardModeId { get; set; }
        [Key(3)] public long ZoneTypeId { get; set; }
        [Key(4)] public long Seed { get; set; }
        [Key(5)] public int TileIndex { get; set; }

        [Key(6)] public int ModeStartIndex { get; set; }
        [Key(7)] public int ModeRollsLeft { get; set; }
        [Key(8)] public int ModeLapsLeft { get; set; }
               
        public DataArray Tiles = new DataArray();
        public DataArray PassRewards = new DataArray();
        public DataArray LandRewards = new DataArray();

        [Key(9)] public List<TileCharge> Charges { get; set; } = new List<TileCharge>();

        public int Length { get { return Tiles.Length; } }


        private long[] _allPathIndexes = null;
        public long[] GetAllPathIndexes()
        {
            if (_allPathIndexes != null)
            {
                return _allPathIndexes;
            }
            long[] retval = new long[Length];
            long pathIndex = BoardGameConstants.StartPathIndex;
            retval[0] = pathIndex;
            for (int t = 1; t < Length; t++)
            {
                if (Tiles[t] == TileTypes.Home)
                {
                    pathIndex++;
                }
                retval[t] = pathIndex;
            }
            _allPathIndexes = retval;
            return retval;
        }

        public long GetPathIndex(int tileIndex)
        {
            if (tileIndex < 0 || tileIndex >= Tiles.Length)
            {
                return BoardGameConstants.StartPathIndex;
            }
            long pathIndex = BoardGameConstants.StartPathIndex;
            for (int t = 1; t < Tiles.Length && t < tileIndex; t++)
            {
                if (Tiles[t] == TileTypes.Home)
                {
                    pathIndex++;
                }
            }
            return pathIndex;
        }

        public void CreateData(List<long> tileTypeIds)
        {
            Tiles.Create(tileTypeIds.Count);
            PassRewards.Create(tileTypeIds.Count);
            LandRewards.Create(tileTypeIds.Count);

            for (int t = 0; t < tileTypeIds.Count; t++)
            {
                Tiles[t] = tileTypeIds[t];
            }
        }

    }
    [MessagePackObject]
    public class BoardDataLoader : UnitDataLoader<BoardData> { }

    [MessagePackObject]
    public class BoardDataMapper : UnitDataMapper<BoardData> { }
}
