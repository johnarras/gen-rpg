﻿using Genrpg.Shared.Crawler.Maps.Entities;
using Assets.Scripts.UI.Services;
using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Constants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genrpg.Shared.Client.Assets;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Client.Assets.Services;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.MapGen.Services;
using Genrpg.Shared.Crawler.MapGen.Entities;
using Genrpg.Shared.Crawler.MapGen.Helpers;

namespace Assets.Scripts.Crawler.Maps.Services.GenerateMaps
{
    public abstract class BaseCrawlerMapGenHelper : ICrawlerMapGenHelper
    {

        protected IAssetService _assetService;
        protected IUIService _uiService;
        protected ILogService _logService;
        protected IGameData _gameData;
        protected IClientGameState _gs;
        protected IClientEntityService _clientEntityService;
        protected ICrawlerWorldService _worldService;
        protected ICrawlerMapService _crawlerMapService;
        protected ICrawlerMapGenService _mapGenService;
        protected IZoneGenService _zoneGenService;

        public abstract long GetKey();

        public abstract Task<NewCrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData crawlerMapGenData);

        /// <summary>
        /// Add a bunch of random lines within a given 
        /// </summary>
        /// <param name="map"></param>
        /// <param name="genData"></param>
        /// <param name="rand"></param>
        /// <param name="density"></param>
        /// <returns></returns>
        protected bool[,] AddCorridors(CrawlerMap map, CrawlerMapGenData genData, IRandom rand, float density = 1.0f)
        {
            bool[,] clearCells = new bool[map.Width, map.Height];
            clearCells[map.Width / 2, map.Height / 2] = true;

            List<MyPoint> endPoints = new List<MyPoint> { new MyPoint(map.Width / 2, map.Height / 2) };

            int streetCount = (int)(Math.Sqrt((map.Width * map.Height)) * density);

            int edgeSize = 2;
            for (int times = 0; times < streetCount; times++)
            {
                MyPoint startPoint = endPoints[rand.Next() % endPoints.Count];

                List<MyPoint> okOffsets = new List<MyPoint>();

                for (int x = -1; x <= 1; x++)
                {
                    int sx = startPoint.X + x;

                    if (sx < edgeSize || sx >= map.Width-edgeSize)
                    {
                        continue;
                    }

                    for (int y = -1; y <= 1; y++)
                    {
                        if ((x != 0) == (y != 0))
                        {
                            continue;
                        }

                        int sy = startPoint.Y + y;

                        if (sy < edgeSize || sy >= map.Height-edgeSize)
                        {
                            continue;
                        }

                        if (clearCells[sx, sy] == true)
                        {
                            continue;
                        }

                        okOffsets.Add(new MyPoint(x, y));
                    }
                }

                if (okOffsets.Count < 1)
                {
                    continue;
                }

                MyPoint offset = okOffsets[rand.Next() % okOffsets.Count];

                int length = MathUtils.IntRange(3, 12, rand);

                for (int l = 1; l < length; l++)
                {
                    int lx = startPoint.X + l * offset.X;
                    int ly = startPoint.Y + l * offset.Y;

                    if (lx < edgeSize || lx >= map.Width - edgeSize ||
                        ly < edgeSize || ly >= map.Height - edgeSize)
                    {
                        continue;
                    }
                    clearCells[lx, ly] = true;

                    if ((l == length / 2 && rand.NextDouble() < 0.2f) || l == length - 1 || rand.NextDouble() < 0.05f)
                    {
                        endPoints.Add(new MyPoint(lx, ly));
                    }
                }
            }

            return clearCells;
        }
    }
}