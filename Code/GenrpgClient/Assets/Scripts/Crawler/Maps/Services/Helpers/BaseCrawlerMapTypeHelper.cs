using Assets.Scripts.Buildings;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.UI.Services;
using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.ProcGen.Settings.Trees;
using Genrpg.Shared.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Client.Assets.Services;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.Crawler.MapGen.Services;
using System.Runtime.InteropServices;
using Genrpg.Shared.ProcGen.Settings.Textures;
using Assets.Scripts.Assets.Textures;
using Assets.Scripts.Dungeons;
using System.Runtime;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{
    public abstract class BaseCrawlerMapTypeHelper : ICrawlerMapTypeHelper
    {
        protected IAssetService _assetService;
        protected IUIService _uIInitializable;
        protected ILogService _logService;
        protected IGameData _gameData;
        protected IClientGameState _gs;
        protected IClientEntityService _clientEntityService;
        protected ICrawlerWorldService _worldService;
        protected ICrawlerMapService _mapService;
        protected ICrawlerMapGenService _mapGenService;

        public abstract long GetKey();

        protected virtual bool IsIndoors() { return false; }

        public virtual async Awaitable<CrawlerMapRoot> EnterMap(PartyData partyData, EnterCrawlerMapData mapData, CancellationToken token)
        {
            if (partyData.CurrentMap.MapId != mapData.MapId)
            {
                partyData.CurrentMap.Visited.Clear();
                partyData.MapId = mapData.MapId;
            }

            if (partyData.MapX < 0 || partyData.MapZ < 0)
            {
                mapData.MapX = mapData.Map.Width / 2;
                mapData.MapZ = mapData.Map.Height / 2;
            }

            partyData.MapId = mapData.MapId;
            partyData.MapX = mapData.MapX;
            partyData.MapZ = mapData.MapZ;
            partyData.MapRot = mapData.MapRot;
            CrawlerMap map = mapData.Map;

            GameObject go = new GameObject() { name = GetKey().ToString() };
            CrawlerMapRoot mapRoot = _clientEntityService.GetOrAddComponent<CrawlerMapRoot>(go);

            mapRoot.SetupFromMap(map);
            mapRoot.DrawX = partyData.MapX * CrawlerMapConstants.BlockSize;
            mapRoot.DrawZ = partyData.MapZ * CrawlerMapConstants.BlockSize;
            mapRoot.DrawY = CrawlerMapConstants.BlockSize / 2;
            mapRoot.DrawRot = partyData.MapRot;

            await Task.CompletedTask;
            return mapRoot;
        }

        protected void AddWallComponent(CrawlerMapRoot mapRoot, ClientMapCell cell, int assetPositionIndex, int dungeonAssetIndex, GameObject parent, Vector3 offset, Vector3 euler, int realCellX, int realCellZ)
        {
            List<WeightedDungeonAsset> assetList = mapRoot.DungeonAssets.GetAssetList(dungeonAssetIndex);

            bool isDoor = dungeonAssetIndex == DungeonAssetIndex.Doors;

            if (isDoor)
            {
                dungeonAssetIndex = DungeonAssetIndex.Walls;
            }

            DungeonAsset asset = assetList[0].Asset;

            long assetWeightSum = assetList.Sum(x=>x.Weight);

            if (assetWeightSum > 0)
            {
                if (realCellZ < 0 && realCellZ < 0)
                {
                    asset = assetList[(int)(mapRoot.Map.ArtSeed % assetList.Count)].Asset;
                }

                long weightHash = realCellX * 7079 + realCellZ * 2383 + (int)offset.x * 3361 + (int)offset.y * 709 + (int)offset.z * 4327;

                long chosenWeight = weightHash % assetWeightSum;

                foreach (WeightedDungeonAsset wgo in assetList)
                {
                    chosenWeight -= wgo.Weight;

                    if (chosenWeight <= 0)
                    {
                        asset = wgo.Asset;
                        break;
                    }
                }
            }

            DungeonAsset dungeonAsset = _clientEntityService.FullInstantiate(asset);
            cell.AssetPositions[assetPositionIndex] = dungeonAsset;
            _clientEntityService.AddToParent(dungeonAsset, parent);
            dungeonAsset.transform.localPosition = offset;
            dungeonAsset.transform.eulerAngles = euler;


            List<WeightedMaterial> materialList = mapRoot.DungeonMaterials.GetMaterials(dungeonAssetIndex);

            Material finalMat = materialList.Count > 0 ? materialList[0].Mat : null;

            long matWeightSum = materialList.Sum(x=>x.Weight);

            if (matWeightSum > 0)
            {
                long weightHash = realCellX * 1951 + realCellZ * 443 + (int)offset.x * 197 + (int)offset.y * 2843 + (int)offset.z * 653;

                long chosenWeight = weightHash % matWeightSum;

                foreach (WeightedMaterial weightedMat in materialList)
                {
                    chosenWeight -= weightedMat.Weight;

                    if (chosenWeight <= 0)
                    {
                        finalMat = weightedMat.Mat;
                        break;
                    }
                }
            }

            if (finalMat != null)
            {
                foreach (Renderer rend in dungeonAsset.Renderers)
                {
                    rend.material = finalMat;
                }

                if (isDoor)
                {
                    foreach (Renderer rend in dungeonAsset.DoorRenderers)
                    {
                        rend.material = mapRoot.DoorMat;
                    }
                }
            }
            else
            {
                _clientEntityService.SetActive(dungeonAsset, false);
            }
        }

        protected void ShowBuilding(CrawlerBuilding buildingIn, BuildingMats mats, object parent, CrawlerObjectLoadData loadData)
        {

            if (loadData == null || loadData.MapCell == null || loadData.BuildingType == null || loadData.MapRoot == null)
            {
                return;
            }
            CrawlerBuilding crawlerBuilding = _clientEntityService.FullInstantiate(buildingIn);
            _clientEntityService.AddToParent(crawlerBuilding, parent);

            crawlerBuilding.InitData(loadData.BuildingType, loadData.Seed, mats);
            crawlerBuilding.transform.eulerAngles = new Vector3(0, loadData.Angle, 0);
            crawlerBuilding.transform.localScale = Vector3.one;
        }

        protected virtual void LoadTerrainTexture(GameObject parent, long terrainTextureId, CancellationToken token)
        {
            TextureType ttype = _gameData.Get<TextureTypeSettings>(null).Get(terrainTextureId);

            if (ttype != null && !string.IsNullOrEmpty(ttype.Art))
            {
                try
                {
                    _assetService.LoadAssetInto(parent, AssetCategoryNames.TerrainTex, ttype.Art, OnDownloadTerrainTexture, parent, token);
                }
                catch (Exception ee)
                {
                    _logService.Info("Inner Load Error: " + ee.Message);
                }
            }
        }

        private void OnDownloadTerrainTexture(object obj, object data, CancellationToken token)
        {

            GameObject parent = data as GameObject;

            if (parent == null)
            {
                return;
            }

            GameObject go = obj as GameObject;

            if (go == null)
            {
                return;
            }

            TextureList tlist = _clientEntityService.GetComponent<TextureList>(go);

            if (tlist == null || tlist.Textures == null || tlist.Textures.Count < 1 || tlist.Textures[0] == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            GImage image = _clientEntityService.GetComponent<GImage>(parent);

            if (image == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            Sprite spr = Sprite.Create(tlist.Textures[0], new Rect(0, 0, tlist.Textures[0].width, tlist.Textures[0].height), Vector2.zero);

            image.sprite = spr;
        }


        /// <summary>
        /// Find blocking bits for a given coordinate.
        /// </summary>
        /// <param name="mapRoot"></param>
        /// <param name="sx">Can be out of range 0-map.Width-1</param>
        /// <param name="sz">Can be out of range 0-map.Height-1</param>
        /// <param name="ex">Can be out of range 0-map.Width-1</param>
        /// <param name="ez">Can be out of range 0-map.Height-1</param>
        /// <returns></returns>
        public virtual int GetBlockingBits(CrawlerMapRoot mapRoot, int sx, int sz, int ex, int ez, bool allowBuildingEntry)
        {
            int blockBits = 0;
            if (ex > sx) // East
            {
                blockBits = mapRoot.Map.EastWall(sx, sz);
            }
            else if (ex < sx) // West
            {
                blockBits = mapRoot.Map.EastWall((sx + mapRoot.Map.Width - 1) % mapRoot.Map.Width, sz);
            }
            else if (ez > sz) // Up
            {
                blockBits = mapRoot.Map.NorthWall(sx, sz);
            }
            else if (ez < sz) // Down
            {
                blockBits = mapRoot.Map.NorthWall(sx, (sz + mapRoot.Map.Height - 1) % mapRoot.Map.Height);
            }

            int safeEx = (ex + mapRoot.Map.Width) % mapRoot.Map.Width;
            int safeEz = (ez + mapRoot.Map.Height) % mapRoot.Map.Height;

            if (mapRoot.Map.Get(safeEx, safeEz, CellIndex.Terrain) == 0)
            {
                return WallTypes.Wall;
            }
            byte buildingId = mapRoot.Map.Get(safeEx, safeEz, CellIndex.Building);

            if (buildingId > 0)
            {
                if (!allowBuildingEntry)
                {
                    return WallTypes.Wall;
                }

                if (mapRoot.Map.CrawlerMapTypeId == CrawlerMapTypes.City)
                {
                    int angle = mapRoot.Map.Get(safeEx, safeEz, CellIndex.Dir) * CrawlerMapConstants.DirToAngleMult;

                    int dx = ex - sx;
                    int dz = ez - sz;

                    int moveAngle = ((dx == 0 ? (dz > 0 ? 90 : 270) : (dx > 0 ? 180 : 0)) + 90) % 360;

                    if (moveAngle == angle)
                    {
                        blockBits |= WallTypes.Building;
                    }
                    else
                    {
                        blockBits |= (buildingId != 0 ? WallTypes.Wall : WallTypes.None);
                    }
                }
            }
            return blockBits;
        }

        public virtual async Awaitable DrawCell(CrawlerWorld world, PartyData party, CrawlerMapRoot mapRoot, ClientMapCell cell, int nx, int nz, int realCellX, int realCellZ, CancellationToken token)
        {
            if (mapRoot.DungeonAssets == null || cell.Content != null)
            {
                return;
            }

            int bz = CrawlerMapConstants.BlockSize;

            GameObject go = new GameObject() { name = "Cell" + cell.X + "." + cell.Z };
            cell.Content = go;           
            _clientEntityService.AddToParent(go, mapRoot.gameObject);
            go.transform.position = new Vector3(nx * bz, 0, nz * bz);

            bool isRoom = (mapRoot.Map.Get(cell.X, cell.Z, CellIndex.Walls) & (1 << MapWallBits.IsRoomBitOffset)) != 0;

            int dnx = (cell.X + 1) % mapRoot.Map.Width;
            int dnz = (cell.Z + 1) % mapRoot.Map.Height;

            bool eIsRoom = (mapRoot.Map.Get(dnx, cell.Z, CellIndex.Walls) & (1 << MapWallBits.IsRoomBitOffset)) != 0;
            bool nIsRoom = (mapRoot.Map.Get(cell.X, dnz, CellIndex.Walls) & (1 << MapWallBits.IsRoomBitOffset)) != 0;


            if (!IsIndoors())
            {
                GameObject imageChild = new GameObject() { name = "Image" };
                _clientEntityService.AddToParent(imageChild, go);
                imageChild.AddComponent<GImage>();
                Canvas canvas = imageChild.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                RectTransform rectTransform = imageChild.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector3(bz, bz);
                imageChild.transform.eulerAngles = new Vector3(90, 0, 0);
            }
            else if (mapRoot.Map.Get(cell.X, cell.Z, CellIndex.Terrain) != 0)
            {
                AddWallComponent(mapRoot, cell, DungeonAssetPosition.Ceiling, DungeonAssetIndex.Ceilings, go, new Vector3(0, bz * (isRoom?2:1), 0), new Vector3(90, 0, 0), realCellX, realCellZ);
                AddWallComponent(mapRoot, cell, DungeonAssetPosition.Floor, DungeonAssetIndex.Floors, go, new Vector3(0, 0, 0), new Vector3(90, 0, 0), realCellX, realCellZ);
            }

            Vector3 nOffset = new Vector3(0, bz / 2, bz / 2);
            Vector3 nRot = new Vector3(0, 0, 0);

            int northBits = mapRoot.Map.NorthWall(cell.X, cell.Z);

            bool havePillar = false;
            bool IsTallBorder = false;

            if (northBits == WallTypes.Wall || northBits == WallTypes.Secret)
            {
                AddWallComponent(mapRoot, cell, DungeonAssetPosition.NorthWall, DungeonAssetIndex.Walls, go, nOffset, nRot, realCellX, realCellZ);
                havePillar = true;
            }
            else if (northBits == WallTypes.Door)
            {
                AddWallComponent(mapRoot, cell, DungeonAssetPosition.NorthWall, DungeonAssetIndex.Doors, go, nOffset, nRot, realCellX, realCellZ);
                havePillar = true;
            }
            else if (northBits == WallTypes.Barricade)
            {
                AddWallComponent(mapRoot, cell, DungeonAssetPosition.NorthWall, DungeonAssetIndex.Fences, go, nOffset, nRot, realCellX, realCellZ);
            }
            if (isRoom != nIsRoom && IsIndoors())
            {
                AddWallComponent(mapRoot, cell, DungeonAssetPosition.NorthUpper, DungeonAssetIndex.Walls, go, nOffset + new Vector3(0, bz, 0), nRot, realCellX, realCellZ);
                IsTallBorder = true;
            }

            Vector3 eOffset = new Vector3(bz / 2, bz / 2, 0);
            Vector3 eRot = new Vector3(0, 90, 0);

            int eastBits = mapRoot.Map.EastWall(cell.X, cell.Z);

            if (eastBits == WallTypes.Wall || eastBits == WallTypes.Secret)
            {
                AddWallComponent(mapRoot, cell, DungeonAssetPosition.EastWall, DungeonAssetIndex.Walls, go, eOffset, eRot, realCellX, realCellZ);
                havePillar = true;
            }
            else if (eastBits == WallTypes.Door)
            {
                AddWallComponent(mapRoot, cell, DungeonAssetPosition.EastWall, DungeonAssetIndex.Doors, go, eOffset, eRot, realCellX, realCellZ);
                havePillar = true;
            }
            else if (eastBits == WallTypes.Barricade)
            {
                AddWallComponent(mapRoot, cell, DungeonAssetPosition.EastWall, DungeonAssetIndex.Fences, go, eOffset, eRot, realCellX, realCellZ);
            }

            if (isRoom != eIsRoom && IsIndoors())
            {
                AddWallComponent(mapRoot, cell, DungeonAssetPosition.EastUpper, DungeonAssetIndex.Walls, go, eOffset + new Vector3(0, bz, 0), eRot, realCellX, realCellZ);
                IsTallBorder = true;
            }


            // Check next wall up or over.
            if (!havePillar)
            {
                if (realCellX == 0 || realCellZ == 0 ||
                    realCellX == mapRoot.Map.Width - 1 ||
                    realCellZ == mapRoot.Map.Height -1)
                {
                    if (!mapRoot.Map.Looping)
                    {
                        havePillar = true;
                    }
                }

                int upx = realCellX;
                int upz = (realCellZ + 1) % mapRoot.Map.Height;

                int eastWall = mapRoot.Map.EastWall(realCellX, (realCellZ + 1) % mapRoot.Map.Height);
                if (eastWall == WallTypes.Wall || eastWall == WallTypes.Door || eastWall == WallTypes.Secret)
                {
                    havePillar = true;
                }
                else
                {
                    int northWall = mapRoot.Map.NorthWall((realCellX + 1) % mapRoot.Map.Width, realCellZ);
                    if (northWall == WallTypes.Wall || northWall == WallTypes.Door || northWall == WallTypes.Secret)
                    {
                        havePillar = true;
                    }
                }
            }

            if (havePillar && mapRoot.Map.CrawlerMapTypeId != CrawlerMapTypes.Outdoors)
            {
                AddWallComponent(mapRoot, cell, DungeonAssetPosition.Pillar, DungeonAssetIndex.Pillars, go, new Vector3(bz/2,0,bz/2), Vector3.zero, -1, -1);
                if (IsTallBorder)
                {
                    AddWallComponent(mapRoot, cell, DungeonAssetPosition.Pillar, DungeonAssetIndex.Pillars, go, new Vector3(bz / 2, bz, bz / 2), Vector3.zero, -1, -1);
                }
            }

            byte biomeTypeId = mapRoot.Map.Get(cell.X, cell.Z, CellIndex.Terrain);

            try
            {
                if (biomeTypeId > 0)
                {
                    ZoneType biomeType = _gameData.Get<ZoneTypeSettings>(null).Get(biomeTypeId);

                    if (biomeType != null)
                    {
                        LoadTerrainTexture(go, biomeType.Textures.Where(x => x.TextureChannelId == MapConstants.BaseTerrainIndex).First().TextureTypeId, token);
                    }
                }
            }
            catch (Exception e)
            {
                _logService.Info("Draw Cell Error: " + e.Message);
            }

            List<MapCellDetail> exitDetails = mapRoot.Map.Details.Where(d => d.X == cell.X && d.Z == cell.Z && d.EntityTypeId == EntityTypes.Map).ToList();

            byte buildingId = mapRoot.Map.Get(cell.X, cell.Z, CellIndex.Building);

            if (buildingId > 0)
            {
                BuildingType btype = _gameData.Get<BuildingSettings>(null).Get(buildingId);

                if (btype != null)
                {

                    string suffix = "";

                    if (btype.VariationCount > 1)
                    {
                        int indexVal = (cell.X * 13 + cell.Z * 41) % btype.VariationCount + 1;
                        suffix = indexVal.ToString();
                    }

                    int angle = mapRoot.Map.Get(cell.X, cell.Z, CellIndex.Dir) * CrawlerMapConstants.DirToAngleMult;


                    CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
                    {
                        MapCell = cell,
                        BuildingType = btype,
                        Angle = angle,
                        MapRoot = mapRoot,
                        Seed = cell.X*31+cell.Z*97+world.Seed/11+mapRoot.Map.ArtSeed/3+mapRoot.Map.IdKey/7,
                    };


                    string buildingArtName = btype.Art + suffix;


                    if (mapRoot.CityAssets != null)
                    {
                        int weightSum = mapRoot.CityAssets.Buildings.Sum(x=>x.Weight);
                        int weightChosen = (int)loadData.Seed * 3 % weightSum;
                        foreach (WeightedCrawlerBuilding wcb in mapRoot.CityAssets.Buildings)
                        {
                            weightChosen -= wcb.Weight;

                            if (weightChosen <= 0)
                            {
                                ShowBuilding(wcb.Building, wcb.Mats, cell.Content, loadData);
                            }
                        }
                    }
                }
            }

            byte treeTypeId = mapRoot.Map.Get(cell.X, cell.Z, CellIndex.Tree);

            if (treeTypeId > 0)
            {
                TreeType treeType = _gameData.Get<TreeTypeSettings>(null).Get(treeTypeId);

                if (treeType != null)
                {
                    int variation = 1;
                    if (treeType.VariationCount > 1)
                    {
                        variation = 1 + (cell.X * 31 + cell.Z * 47) % treeType.VariationCount;                        
                    }
                    _assetService.LoadAssetInto(cell.Content, AssetCategoryNames.Trees, treeType.Art + variation, OnDownloadTree, cell, token);

                }
            }
            await Task.CompletedTask;
        }

        protected void OnDownloadTree(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;

            if (go == null)
            {
                return;    
            }

            ClientMapCell cell = data as ClientMapCell;

            if (cell != null)
            {
                int dir = ((cell.X * 137 + cell.Z * 97) % 4)*90;

                go.transform.eulerAngles = new Vector3(0, dir, 0);
            }
        }
    }
}
