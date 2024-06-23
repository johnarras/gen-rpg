using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.ProcGen.RandomNumbers;
using Genrpg.Shared.Biomes.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{
    public class OutdoorCrawlerMapTypeHelper : BaseCrawlerMapTypeHelper
    {

        ISamplingService _samplingService;
        ICrawlerMapService _mapService;
        ILineGenService _lineGenService;

        public override ECrawlerMapTypes GetKey() { return ECrawlerMapTypes.Outdoors; }


        public override Awaitable<CrawlerMapRoot> Enter(PartyData partyData, EnterCrawlerMapData mapData, CancellationToken token)
        {

            return null;
        }
        public override async Awaitable DrawCell(CrawlerMapRoot mapRoot, UnityMapCell cell, int xpos, int zpos, CancellationToken token)
        {

            await Task.CompletedTask;
        }


        public override int GetBlockingBits(CrawlerMapRoot mapRoot, int sx, int sz, int ex, int ez)
        {
            return 0;
        }


        private List<Color> _biomeColors = null;
        private List<Color> GetBiomeColors()
        {
            if (_biomeColors == null)
            {
                _biomeColors = new List<Color>()
                {
                    Color.black, // City/POI
                    new Color(0,0.8f,0),  // Field
                    new Color(0,0.5f,0), // Forest
                    new Color(0.7f,0.7f,0), // Savannah
                    new Color(0.4f,0.7f,0), // Hills
                    new Color(0.5f,0.3f,0), // Swamp
                    new Color(1,1,1), // Snow
                    new Color(0.5f,0.5f,0.5f), // Mountain
                    new Color(1,1,0), // Desert
                    new Color(0.7f,0.2f,0), // Lava
                    new Color(0.3f,0.3f,1), // Water
                    new Color(0.75f,0.75f,0.75f), // Road
                };
            }
            return _biomeColors;
        }

        public override CrawlerMap Generate(CrawlerMapGenData genData)
        {

            CrawlerMap map = genData.World.CreateMap(ECrawlerMapTypes.Outdoors, false, 96, 64);

            IRandom rand = new MyRandom(genData.World.IdKey * 13 + map.IdKey*131);

            byte[,] overrides = new byte[map.Width, map.Height];
            long[,] terrain = new long[map.Width, map.Height];
            SpawnResult[,] objects = new SpawnResult[map.Width, map.Height];

            List<BiomeRegion> regions = new List<BiomeRegion>();

            List<BiomeType> biomes = _gameData.Get<BiomeSettings>(null).GetData().Where(x=>x.IdKey > 0).OrderBy(x=>x.MinLevel).ToList();

            List<long> okBiomeIds = biomes.Where(x => x.IdKey > 0 && x.MinLevel < 1000).Select(x=>x.IdKey).ToList();

            List<BiomeType> origBiomes = new List<BiomeType>(biomes);

            List<Color> biomeColors = GetBiomeColors();

            int startMapEdgeSize = 4;

            int cityDistanceFromEdge = startMapEdgeSize * 2;


            int fullRegionBiomes = biomes.Where(x => x.MinLevel <= 100).Count();

            SamplingData samplingData = new SamplingData()
            {
                Count = fullRegionBiomes,
                MaxAttemptsPerItem = 20,
                XMin = cityDistanceFromEdge,
                XMax = map.Width - cityDistanceFromEdge,
                YMin = cityDistanceFromEdge,
                YMax = map.Height - cityDistanceFromEdge,
                MinSeparation = 15,
                Seed = rand.Next(),
            };

            
            List<MyPoint2> points = _samplingService.PlanePoissonSample(samplingData);

            int sortx = (rand.NextDouble() < 0.5 ? -1 : 1);
            int sorty = (rand.NextDouble() < 0.5 ? -1 : 1);

            points = points.OrderBy(p=>p.X*sortx).ThenBy(p=>p.Y*sorty).ToList();

            List<MyPoint2> origPoints = new List<MyPoint2>(points);

            MyPoint2 firstPoint = points[0];

            points = points.OrderBy(p =>
                Math.Sqrt(
                    (p.X - firstPoint.X) * (p.X - firstPoint.X) +
                    (p.Y - firstPoint.Y) * (p.Y - firstPoint.Y)
                    )).ToList();


            int level = 1;
            int levelDelta = 7;
            float spreadDelta = 0.2f;
            float dirDelta = 0.3f;

            long cityBiomeId = 0;
            long waterBiomeId = origBiomes.FirstOrDefault(x => x.Name == "Water").IdKey;
            long roadBiomeId = origBiomes.FirstOrDefault(x => x.Name == "Road").IdKey;
            long poiBiomeId = origBiomes.FirstOrDefault(x => x.Name == "PointOfInterest").IdKey;
            long mountainBiomeId = origBiomes.FirstOrDefault(x => x.Name == "Mountains").IdKey;

            while (points.Count > 0 && biomes.Count > 0)
            {
                List<BiomeType> okBiomes = biomes.Where(x => x.MinLevel <= level).ToList();

                if (okBiomes.Count < 1)
                {
                    break;
                }

                MyPoint2 centerPoint = points[0];

                points.Remove(centerPoint);

                BiomeType biomeType = okBiomes[rand.Next() % okBiomes.Count];

                biomes.Remove(biomeType);

                BiomeRegion region = new BiomeRegion()
                {
                    CenterX = (int)centerPoint.X,
                    CenterY = (int)centerPoint.Y,
                    SpreadX = MathUtils.FloatRange(1 - spreadDelta, 1 + spreadDelta, rand),
                    SpreadY = MathUtils.FloatRange(1 - spreadDelta, 1 + spreadDelta, rand),
                    BiomeTypeId = biomeType.IdKey,
                    DirX = MathUtils.FloatRange(-dirDelta, dirDelta, rand),
                    DirY = MathUtils.FloatRange(-dirDelta, dirDelta, rand),
                };

                level += levelDelta;


                regions.Add(region);

            }

            if (regions.Count < 1)
            {
                return map;
            }

            float radiusDelta = 0.2f;

            int radius = 0;
            while (true)
            {
                bool foundUnsetCell = false;
                for (int x = 0; x < map.Width; x++)
                {
                    for (int y = 0; y < map.Height; y++)
                    {
                        if (terrain[x,y] == 0)
                        {
                            foundUnsetCell = true;
                            break;
                        }
                    }
                    if (foundUnsetCell)
                    {
                        break;
                    }
                }

                if (!foundUnsetCell)
                {
                    break;
                }

                radius++;

                foreach (BiomeRegion region in regions)
                {
                    float currRadius = MathUtils.FloatRange(radius * (1 - radiusDelta), radius * (1 + radiusDelta), rand);

                    float xrad = currRadius * region.SpreadX;
                    float yrad = currRadius * region.SpreadY;
                    float xcenter = region.CenterX + region.DirX * currRadius;
                    float ycenter = region.CenterY * region.DirY * currRadius;

                    xcenter = region.CenterX;
                    ycenter = region.CenterY;

                    int xmin = MathUtils.Clamp(0, (int)(xcenter - xrad - 1), map.Width - 1);
                    int xmax = MathUtils.Clamp(0, (int)(xcenter + xrad + 1), map.Width - 1);

                    int ymin = MathUtils.Clamp(0, (int)(ycenter - yrad - 1), map.Height - 1);
                    int ymax = MathUtils.Clamp(0, (int)(ycenter + yrad + 1), map.Height - 1);

                    for (int x = xmin; x <= xmax; x++)
                    {
                        for (int y = ymin; y <= ymax; y++)
                        {

                            if (terrain[x,y] != 0)
                            {
                                continue;
                            }

                            float xpct = (x - xcenter)/xrad;
                            float ypct = (y - ycenter)/yrad;

                            float distScale = Mathf.Sqrt(xpct*xpct + ypct*ypct);

                            if (distScale <= 1)
                            {
                                terrain[x, y] = region.BiomeTypeId;
                            }
                        }
                    }
                }
            }



            List<float> cornerRadii = new List<float>();

            float minCornerRadius = 12;
            float maxCornerRadius = 20;

            for (int c = 0; c < 4; c++)
            {
                cornerRadii.Add(MathUtils.FloatRange(minCornerRadius, maxCornerRadius, rand));
            }


            int maxCheckRadius = (int)(maxCornerRadius + startMapEdgeSize);

            int xcorner = 0;
            int ycorner = 0;
            for (int x = 0; x < map.Width; x++)
            {
                
                for (int y = 0; y < map.Height; y++)
                {
                    int cornerIndex = -1;

                    if (x <= maxCheckRadius)
                    {
                        xcorner = 0;
                        if (y <= maxCheckRadius)
                        {
                            ycorner = 0;
                            cornerIndex = 0;
                        }
                        else if (y >= map.Height-maxCheckRadius-1)
                        { 
                            cornerIndex = 1;
                            ycorner = map.Height - 1;
                        }
                    }
                    else if (x >= map.Width-maxCheckRadius-1)
                    {
                        xcorner = map.Width - 1;
                        if (y <= maxCheckRadius)
                        {
                            cornerIndex = 2;
                            ycorner = 0;
                        }
                        else if (y >= map.Height-maxCheckRadius-1)
                        {
                            cornerIndex = 3;
                            ycorner = map.Height - 1;
                        }
                    }

                    int mapEdgeSize = startMapEdgeSize + MathUtils.IntRange(-1, 1, rand);
                    if ((x < mapEdgeSize || x >= map.Width - mapEdgeSize) ||
                        (y < mapEdgeSize || y >= map.Height - mapEdgeSize))
                    {
                        terrain[x, y] = waterBiomeId;
                    }


                    if (cornerIndex >= 0 && cornerIndex < cornerRadii.Count)
                    {
                        int currRadius = (int)cornerRadii[cornerIndex]+startMapEdgeSize;


                        int cx = xcorner;
                        int cy = ycorner;

                        if (cx > 0)
                        {
                            cx -= currRadius;
                        }
                        else
                        {
                            cx += currRadius;
                        }

                        if (cy > 0)
                        {
                            cy -= currRadius;
                            
                        }
                        else
                        {
                            cy += currRadius;
                        }

                        if (cx < map.Width/2 && x > cx)
                        {
                            continue;
                        }

                        if (cx > map.Width/2 && x < cx)
                        {
                            continue;
                        }

                        if (cy < map.Height/2 && y > cy)
                        {
                            continue;
                        }

                        if (cy > map.Height/2 && y < cy)
                        {
                            continue;
                        }

                        float currDist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));

                        currDist += MathUtils.FloatRange(-1, 1, rand);

                        if (currDist >= currRadius && terrain[x, y] != waterBiomeId)                
                        {
                            terrain[x, y] = waterBiomeId;
                        }
                    }
                }
            }

            origPoints = origPoints.OrderBy(x => x.X).ToList();



            // Roads between cities


            List<MyPoint2> remainingPoints = new List<MyPoint2>(origPoints);


            MyPoint2 prevPoint = null;
            MyPoint2 currPoint = remainingPoints[0];
            remainingPoints.RemoveAt(0);

            float skipMidChance = 0.20f;

            while (remainingPoints.Count > 0)
            {
                MyPoint2 nextPoint = remainingPoints[0];
                remainingPoints.RemoveAt(0);

                int cx = (int)(currPoint.X);
                int cy = (int)(currPoint.Y);

                int nx = (int)(nextPoint.X);
                int ny = (int)(nextPoint.Y);


                int px = cx;

                if (prevPoint != null)
                {
                    px = (int)(prevPoint.X);    
                }

                bool foundRoad = false;
                // First checkbackwards to see if there's a matching road.
                for (int x = nx; x >= px; x--)
                {
                    if (terrain[x,ny] == roadBiomeId)
                    {
                        foundRoad = true;
                        for (int xx = nx; xx > x; xx--)
                        {
                            if (terrain[xx, ny] == roadBiomeId)
                            {
                                break;
                            }
                            terrain[xx,ny] = roadBiomeId;

                        }
                    }
                }

                if (foundRoad)
                {

                    prevPoint = currPoint;
                    currPoint = nextPoint;
                    continue;
                }

                float dx = nx - cx;
                float dy = ny - cy;


                int midx = (int)(cx + (int)MathUtils.FloatRange(0.33f, 0.67f, rand) * dx);
                int midy = (int)(cy + (int)MathUtils.FloatRange(0.33f, 0.67f, rand)  * dy);

                List<int[]> segments = new List<int[]>();

                segments.Add(new int[] { cx, cy, midx, midy });
                segments.Add(new int[] { midx, midy, nx, ny });


                foreach (int[] segment in segments)
                {
                    int sx = segment[0];
                    int sy = segment[1];
                    int ex = segment[2];
                    int ey = segment[3];

                    if (rand.NextDouble() < 0.5f)
                    {
                        for (int xx = Math.Min(sx,ex); xx <= Math.Max(sx,ex); xx++)
                        {
                            terrain[xx, sy] = roadBiomeId;
                        }
                        for (int yy = Math.Min(sy,ey); yy <= Math.Max(sy,ey); yy++)
                        {
                            terrain[ex, yy] = roadBiomeId;
                        }
                    }
                    else
                    {
                        for (int xx = Math.Min(sx, ex); xx <= Math.Max(sx, ex); xx++)
                        {
                            terrain[xx, ey] = roadBiomeId;
                        }
                        for (int yy = Math.Min(sy, ey); yy <= Math.Max(sy, ey); yy++)
                        {
                            terrain[sx, yy] = roadBiomeId;
                        }
                    }
                }

                prevPoint = currPoint;
                currPoint = nextPoint;
            }



            // Rivers
            // Mountains at zone borders. (okBiomeIds if  two diff make a small blob...only replacing things in ok biomeIds

            int crad = 1;
            int rrad = 2;
            int trad = Math.Max(crad, rrad);
            for (int x = trad; x < map.Width-trad; x++)
            {
                for (int y = trad; y < map.Height-trad; y++)
                {
                    List<long> currOkBiomeIds = new List<long>();
                    bool foundBadBiomeId = false;


                    // Check for roads.
                    for (int xx = x - rrad; xx <= x + rrad; xx++)
                    {
                        for (int yy = y - rrad; yy <= y + rrad; yy++)
                        {
                            if (terrain[xx, yy] == roadBiomeId)
                            {
                                foundBadBiomeId = true;
                                break;
                            }
                        }
                    }

                    if (foundBadBiomeId)
                    {
                        continue;
                    }

                    // Now check smaller radius for diff biomes.
                    for (int xx = x-crad; xx <= x+crad; xx++)
                    {
                        for (int yy = y-crad; yy <= y+crad; yy++)
                        {
                            long tid = terrain[xx, yy];
                            if (okBiomeIds.Contains(tid))
                            {
                                if (!currOkBiomeIds.Contains(tid))
                                {
                                    currOkBiomeIds.Add(tid);
                                }
                            }
                            else if (tid != mountainBiomeId)
                            {
                                foundBadBiomeId = true;
                                break;
                            }
                        }
                    }

                    int nrad = rand.NextDouble() < 0.2f ? 1 : 0;

                    if (!foundBadBiomeId && currOkBiomeIds.Count > 1)
                    {
                        for (int xx = x - nrad; xx <= x + nrad; xx++)
                        {
                            for (int yy = y - nrad; yy <= y + nrad; yy++)
                            {
                                terrain[xx, yy] = mountainBiomeId;
                            }
                        }
                    }
                }
            }



            int cityLevel = 1;
            for (int c = 0; c < origPoints.Count; c++)
            {
                MyPoint2 pt = origPoints[c];

                terrain[(int)pt.X, (int)pt.Y] = cityBiomeId;
                CrawlerMapGenData cityGenData = new CrawlerMapGenData()
                {
                    World = genData.World,
                    MapType = ECrawlerMapTypes.City,
                    MinLevel = cityLevel,
                    MaxLevel = cityLevel + levelDelta - 1,                   
                };

                CrawlerMap cityMap = _mapService.Generate(cityGenData);

                cityLevel += levelDelta;

            }

            try
            {

                Texture2D tex = new Texture2D(map.Width, map.Height, TextureFormat.RGB24, true, true);

                for (int x = 0; x < map.Width; x++)
                {
                    for (int y = 0; y < map.Height; y++)
                    {
                        tex.SetPixel(x, y, biomeColors[(int)terrain[x, y]]);
                    }
                }

                BinaryFileRepository repo = new BinaryFileRepository(_logService);
                repo.SaveBytes("CrawlerOutdoors.png", tex.EncodeToPNG());

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            return map;
        }
    }
}
