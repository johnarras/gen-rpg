using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.Entities;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Tiles.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.BoardGame.Services
{
    public class SharedBoardGenService : ISharedBoardGenService
    {
        protected IGameData _gameData;       
        public List<long> GenerateTiles(IFilteredObject filtered, BoardGenArgs genData)
        {
            List<long> retval = new List<long>();

            if (genData.Seed == 0)
            {
                genData.Seed = (long)(DateTime.UtcNow.Ticks);
            }

            MyRandom rand = new MyRandom(genData.Seed);

            IReadOnlyList<TileType> tileTypes = _gameData.Get<TileTypeSettings>(filtered).GetData();

            BoardGenSettings genSettings = _gameData.Get<BoardGenSettings>(filtered);


            List<TileType> okTileTypes = tileTypes.Where(x => x.IdKey > 0 && x.OnMainPath).ToList();

            List<TileType> dupeTiles = tileTypes.Where(x => x.IdKey > 0 && x.MaxQuantity == 0).ToList();

            if (dupeTiles.Count < 1)
            {
                return retval;
            }

            List<TileType> limitedTiles = tileTypes.Where(x => x.IdKey > 0 && x.MaxQuantity > 0).ToList();

            int specialTilesUsed = 0;

            while (limitedTiles.Count > 0)
            {
                List<TileType> okTiles = limitedTiles.Where(x=>x.MinPosition <= specialTilesUsed).ToList();

                if (okTiles.Count < 1)
                {
                    return retval;
                }

                double totalPriority = okTiles.Sum(x => x.SpawnPriority);

                double chosenPriority = rand.NextDouble() * totalPriority;

                bool didFindTile = false;
                for (int okTileIndex = 0; okTileIndex < okTiles.Count; okTileIndex++)
                {
                    chosenPriority -= okTiles[okTileIndex].SpawnPriority;

                    if (chosenPriority <= 0)
                    {
                        retval.Add(okTiles[okTileIndex].IdKey);
                        for (int goldtileTimes = 0; goldtileTimes < BoardGameConstants.GoldTilesBetweenEachSpecialTile; goldtileTimes++)
                        {
                            retval.Add(dupeTiles[0].IdKey);
                        }
                        limitedTiles.Remove(okTiles[okTileIndex]);
                        didFindTile = true;
                        specialTilesUsed++;
                        break;
                    }
                }

                if (!didFindTile)
                {
                    break;
                }
            }

            int sidePathQuantity = 0;
            
            if (genData.BoardModeId == BoardModes.Primary)
            {
                sidePathQuantity = 1;
            }

            for (int s = 0; s < sidePathQuantity; s++) 
            {
                long sidePathLength = MathUtils.LongRange(genSettings.SidePathMinLength, genSettings.SidePathMaxLength, rand);

                retval.Add(TileTypes.StartPath);
                for (int i = 0; i < sidePathLength; i++)
                {
                    retval.Add(TileTypes.SidePath);
                }
                retval.Add(TileTypes.EndPath);
            }

            return retval;
        }
    }
}
