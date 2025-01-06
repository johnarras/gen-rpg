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

        public virtual async Awaitable<CrawlerMapRoot> Enter(PartyData partyData, EnterCrawlerMapData mapData, CancellationToken token)
        {
            if (partyData.CurrentMap.MapId != mapData.MapId)
            {
                partyData.CurrentMap.Visited.Clear();
                partyData.MapId = mapData.MapId;
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

        protected void AddWallComponent(GameObject asset, GameObject parent, Vector3 offset, Vector3 euler)
        {
            GameObject obj = _clientEntityService.FullInstantiate(asset);
            _clientEntityService.AddToParent(obj, parent);
            obj.transform.localPosition = offset;
            obj.transform.eulerAngles = euler;
        }
        protected void OnDownloadBuilding(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;

            if (go == null)
            {
                return;
            }

            CrawlerObjectLoadData loadData = data as CrawlerObjectLoadData;

            if (loadData == null || loadData.MapCell == null || loadData.BuildingType == null || loadData.MapRoot == null ||
                go.transform.parent == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            MapBuilding mapBuilding = _clientEntityService.GetComponent<MapBuilding>(go);

            if (mapBuilding != null)
            {
                mapBuilding.Init(loadData.BuildingType, new OnSpawn());
            }
            go.transform.eulerAngles = new Vector3(0, loadData.Angle, 0);
            go.transform.localScale = Vector3.one;
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

        public virtual async Awaitable DrawCell(CrawlerWorld world, PartyData party, CrawlerMapRoot mapRoot, ClientMapCell cell, int nx, int nz, CancellationToken token)
        {
            if (mapRoot.Assets == null || cell.Content != null)
            {
                return;
            }

            int bz = CrawlerMapConstants.BlockSize;

            GameObject go = new GameObject() { name = "Cell" + cell.X + "." + cell.Z };
            cell.Content = go;           
            _clientEntityService.AddToParent(go, mapRoot.gameObject);
            go.transform.position = new Vector3(nx * bz, 0, nz * bz);

            bool isRoom = (mapRoot.Map.Get(cell.X, cell.Z, CellIndex.Walls) & (1 << MapWallBits.IsRoomBitOffset)) != 0;

            bool nIsRoom = false;
            bool eIsRoom = false;

            int dnx = (cell.X + 1) % mapRoot.Map.Width;
            int dnz = (cell.Z + 1) % mapRoot.Map.Height;

           // if (cell.X < mapRoot.Map.Width-1)
            {
                eIsRoom = (mapRoot.Map.Get(dnx, cell.Z, CellIndex.Walls) & (1 << MapWallBits.IsRoomBitOffset)) != 0;
            }
           // if (cell.Z < mapRoot.Map.Height-1)
            {
                nIsRoom = (mapRoot.Map.Get(cell.X, dnz, CellIndex.Walls) & (1 << MapWallBits.IsRoomBitOffset)) != 0;
            }


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
                AddWallComponent(mapRoot.Assets.Ceiling, go, new Vector3(0, bz * (isRoom?2:1), 0), new Vector3(90, 0, 0));
                AddWallComponent(mapRoot.Assets.Floor, go, new Vector3(0, 0, 0), new Vector3(90, 0, 0));
            }

            Vector3 nOffset = new Vector3(0, bz / 2, bz / 2);
            Vector3 nRot = new Vector3(0, 0, 0);

            int northBits = mapRoot.Map.NorthWall(cell.X, cell.Z);

            if (northBits == WallTypes.Wall || northBits == WallTypes.Secret)
            {
                AddWallComponent(mapRoot.Assets.Wall, go, nOffset, nRot);
            }
            else if (northBits == WallTypes.Door)
            {
                AddWallComponent(mapRoot.Assets.Door, go, nOffset, nRot);
            }
            else if (northBits == WallTypes.Barricade)
            {
                AddWallComponent(mapRoot.Assets.Barricade, go, nOffset, nRot);
            }
            if (isRoom != nIsRoom)
            {
                AddWallComponent(mapRoot.Assets.Wall, go, nOffset + new Vector3(0, bz, 0), nRot);
            }


            Vector3 eOffset = new Vector3(bz / 2, bz / 2, 0);
            Vector3 eRot = new Vector3(0, 90, 0);

            int eastBits = mapRoot.Map.EastWall(cell.X, cell.Z);

            if (eastBits == WallTypes.Wall || eastBits == WallTypes.Secret)
            {
                AddWallComponent(mapRoot.Assets.Wall, go, eOffset, eRot);              
            }
            else if (eastBits == WallTypes.Door)
            {
                AddWallComponent(mapRoot.Assets.Door, go, eOffset, eRot);
            }
            else if (eastBits == WallTypes.Barricade)
            {
                AddWallComponent(mapRoot.Assets.Barricade, go, eOffset, eRot);
            }

            if (isRoom != eIsRoom)
            {
                AddWallComponent(mapRoot.Assets.Wall, go, eOffset + new Vector3(0, bz, 0), eRot);
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
                    };


                    _assetService.LoadAssetInto(cell.Content, AssetCategoryNames.Buildings, "Default/" + btype.Art + suffix, OnDownloadBuilding, loadData, token);
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
