﻿using Assets.Scripts.Buildings;
using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Maps.Services.Helpers;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.UI.Services;
using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.ProcGen.Settings.Texturse;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Constants;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Threading;
using UnityEngine;
using UnityEngine.XR;
using GEntity = UnityEngine.GameObject;

namespace Assets.Scripts.Crawler.Maps.Services.GenerateMaps
{
    public abstract class BaseCrawlerMapGenHelper : ICrawlerMapGenHelper
    {

        public const float EncounterChance = 0.05f;
        public const float EncounterTreasureChance = 0.2f;

        protected IAssetService _assetService;
        protected IUIService _uIInitializable;
        protected ILogService _logService;
        protected IGameData _gameData;
        protected IUnityGameState _gs;
        protected IGameObjectService _gameObjectService;
        protected ICrawlerWorldService _worldService;
        protected ICrawlerMapService _mapService;
        protected ICrawlerMapGenService _mapGenService;
        protected IZoneGenService _zoneGenService;

        public abstract ECrawlerMapTypes GetKey();

        public abstract Awaitable<NewCrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData crawlerMapGenData);

        protected static readonly long[] _newMapZoneIds = new long[] { ZoneTypes.Cave, ZoneTypes.Dungeon, ZoneTypes.Tower };


        protected long GetBuildingIdFromZoneTypeId(long zoneTypeId)
        {
            return zoneTypeId == ZoneTypes.Cave ? BuildingTypes.Cave :
                zoneTypeId == ZoneTypes.Dungeon ? BuildingTypes.Dungeon :
                zoneTypeId == ZoneTypes.Tower ? BuildingTypes.Tower :
                0;
        }

        /// <summary>
        /// Add a bunch of random lines within a given 
        /// </summary>
        /// <param name="map"></param>
        /// <param name="genData"></param>
        /// <param name="rand"></param>
        /// <param name="density"></param>
        /// <returns></returns>
        protected bool[,] AddCorridors(CrawlerMap map, CrawlerMapGenData genData, MyRandom rand, float density = 1.0f)
        {
            bool[,] clearCells = new bool[map.Width, map.Height];
            clearCells[map.Width / 2, map.Height / 2] = true;

            List<MyPoint> endPoints = new List<MyPoint> { new MyPoint(map.Width / 2, map.Height / 2) };

            int streetCount = (int)(Math.Sqrt((map.Width * map.Height)) * density);

            for (int times = 0; times < streetCount; times++)
            {
                MyPoint startPoint = endPoints[rand.Next() % endPoints.Count];

                List<MyPoint> okOffsets = new List<MyPoint>();

                for (int x = -1; x <= 1; x++)
                {
                    int sx = startPoint.X + x;

                    if (sx < 1 || sx >= map.Width - 2)
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

                        if (sy < 1 || sy >= map.Height - 2)
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

                    if (lx < 1 || lx >= map.Width - 1 ||
                        ly < 1 || ly >= map.Height - 1)
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