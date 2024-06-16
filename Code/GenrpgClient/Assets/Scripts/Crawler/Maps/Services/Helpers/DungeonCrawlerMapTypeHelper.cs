using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Dungeons;

using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Dungeons.Settings;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{
    public class DungeonCrawlerMapTypeHelper : BaseCrawlerMapTypeHelper
    {
        public override ECrawlerMapTypes GetKey() { return ECrawlerMapTypes.Dungeon; }


        public async override Awaitable<CrawlerMapRoot> Enter(PartyData partyData, EnterCrawlerMapData mapData, CancellationToken token)
        {
            partyData.MapId = mapData.MapId;
            partyData.MapX = mapData.MapX;
            partyData.MapZ = mapData.MapZ;
            partyData.MapRot = mapData.MapRot;

            CrawlerMap cmap = GenerateMap(mapData.MapId);

            GameObject go = new GameObject() { name = "Dungeon" };
            CrawlerMapRoot mapRoot = _gameObjectService.GetOrAddComponent<CrawlerMapRoot>(go);

            mapRoot.SetupFromMap(cmap);
            mapRoot.DrawX = partyData.MapX * CrawlerMapConstants.BlockSize;
            mapRoot.DrawZ = partyData.MapZ * CrawlerMapConstants.BlockSize;
            mapRoot.DrawY = CrawlerMapConstants.BlockSize / 2;
            mapRoot.DrawRot = partyData.MapRot;


            await Task.CompletedTask;
            return mapRoot;
        }

        CrawlerMap GenerateMap(long dungeonId)
        {
            CrawlerMap cmap = new CrawlerMap();
            cmap.Looping = true;
            cmap.MapType = ECrawlerMapTypes.Dungeon;
            MyRandom rand = new MyRandom(StrUtils.GetIdHash(dungeonId + "Dungeon"));

            cmap.XSize = MathUtils.IntRange(15, 25, rand);
            cmap.ZSize = MathUtils.IntRange(15, 25, rand);

            cmap.SetupDataBlocks();

            float chance = 0.6f;
            for (int x = 0; x < cmap.XSize; x++)
            {
                for (int z = 0; z < cmap.ZSize; z++)
                {
                    int index = cmap.GetIndex(x, z);
                    if (rand.NextDouble() < chance)
                    {
                        if (rand.NextDouble() < chance)
                        {
                            int value = MathUtils.IntRange(0, WallTypes.Max - 1, rand);
                            cmap.CoreData[index] |= (byte)(value << DungeonCoreMapCellBits.EWallStart);
                        }
                        if (rand.NextDouble() < chance)
                        {
                            int value = MathUtils.IntRange(0, WallTypes.Max - 1, rand);
                            cmap.CoreData[index] |= (byte)(value << DungeonCoreMapCellBits.NWallStart);
                        }
                    }
                }
            }

            cmap.DungeonArt = _gameData.Get<DungeonArtSettings>(null).Get(cmap.DungeonArtId);

            return cmap;
        }

        public override int GetBlockingBits(CrawlerMapRoot mapRoot, int sx, int sz, int ex, int ez)
        {
            int blockBits = 0;
            if (ex > sx) // East
            {
                blockBits = mapRoot.EastWall(sx, sz);
            }
            else if (ex < sx) // West
            {
                blockBits = mapRoot.EastWall((sx + mapRoot.Map.XSize - 1) % mapRoot.Map.XSize, sz);
            }
            else if (ez > sz) // Up
            {
                blockBits = mapRoot.NorthWall(sx, sz);
            }
            else if (ez < sz) // Down
            {
                blockBits = mapRoot.NorthWall(sx, (sz + mapRoot.Map.ZSize - 1) % mapRoot.Map.ZSize);
            }
            return blockBits;
        }

        public override async Awaitable DrawCell(CrawlerMapRoot mapRoot, UnityMapCell cell, int nx, int nz, CancellationToken token)
        {
            if (mapRoot.Assets == null)
            {
                return;
            }
            
            int bz = CrawlerMapConstants.BlockSize;

            if (cell.Content == null)
            {
                cell.Content = new GameObject() { name = "Cell" + cell.X + "." + cell.Z };
                GEntityUtils.AddToParent(cell.Content, mapRoot.gameObject);
                cell.Content.transform.position = new Vector3(nx * bz, 0, nz * bz);
            }

            AddWallComponent(mapRoot.Assets.Ceiling, cell.Content, new Vector3(0, bz, 0), new Vector3(90, 0, 0));
            AddWallComponent(mapRoot.Assets.Floor, cell.Content, new Vector3(0, 0, 0), new Vector3(90, 0, 0));

            Vector3 nOffset = new Vector3(0, bz / 2, bz / 2);
            Vector3 nRot = new Vector3(0, 0, 0);


            int northBits = mapRoot.NorthWall(cell.X, cell.Z);

            if (northBits == WallTypes.Wall || northBits == WallTypes.Secret)
            {
                AddWallComponent(mapRoot.Assets.Wall, cell.Content, nOffset, nRot);
            }
            else if (northBits == WallTypes.Door)
            {
                AddWallComponent(mapRoot.Assets.Door, cell.Content, nOffset, nRot);
            }

            Vector3 eOffset = new Vector3(bz / 2, bz / 2, 0);
            Vector3 eRot = new Vector3(0, 90, 0);

            int eastBits = mapRoot.EastWall(cell.X, cell.Z);

            if (eastBits == WallTypes.Wall || eastBits == WallTypes.Secret)
            {
                AddWallComponent(mapRoot.Assets.Wall, cell.Content, eOffset, eRot);
            }
            else if (eastBits == WallTypes.Door)
            {
                AddWallComponent(mapRoot.Assets.Door, cell.Content, eOffset, eRot);
            }
            await Task.CompletedTask;
        }
    }
}
