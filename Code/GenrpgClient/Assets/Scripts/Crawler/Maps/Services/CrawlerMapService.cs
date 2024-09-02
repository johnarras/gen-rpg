using Assets.Scripts.Controllers;
using Assets.Scripts.Crawler.GameEvents;
using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.Maps.Services.Helpers;
using Assets.Scripts.Crawler.StateHelpers.Combat;
using Assets.Scripts.Crawler.Tilemaps;
using Assets.Scripts.Dungeons;
using Assets.Scripts.UI.Crawler.States;
using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Crawler.MapGen.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GEntity = UnityEngine.GameObject;

namespace Assets.Scripts.Crawler.Services.CrawlerMaps
{

    public class CrawlerMapService : ICrawlerMapService
    {
        IAssetService _assetService;
        ICameraController _cameraController;
        ICrawlerService _crawlerService;
        private IDispatcher _dispatcher;
        private ILogService _logService;
        private IGameData _gameData;
        private ICrawlerWorldService _worldService;

        const int ViewRadius = 8;
        CrawlerMapRoot _crawlerMapRoot = null;
        private CancellationToken _token;

        private GameObject _cameraParent = null;
        private Camera _camera = null;

        private PartyData _party;
        private CrawlerWorld _world;

        private SetupDictionaryContainer<long, ICrawlerMapTypeHelper> _mapTypeHelpers = new SetupDictionaryContainer<long, ICrawlerMapTypeHelper>();
        public static long MapType { get; set; } = CrawlerMapTypes.None;

        private GameObject _playerLightObject = null;
        private Light _playerLight = null;
        public async Task Initialize(CancellationToken token)
        {

            _token = token;

            CreateWallImageGrid();
            await Task.CompletedTask;
        }

        public ICrawlerMapTypeHelper GetMapHelper(long mapType)
        {
            if (_mapTypeHelpers.TryGetValue(mapType, out ICrawlerMapTypeHelper helper))
            {
                return helper;
            }
            return null;
        }

        public async Awaitable EnterMap(PartyData partyData, EnterCrawlerMapData mapData, CancellationToken token)
        {
            CleanMap();
            _party = partyData;
            _world = await _worldService.GetWorld(_party.WorldId);

            if (_playerLight == null)
            {
                _cameraParent = _cameraController?.GetCameraParent();
                if (_playerLightObject == null)
                {
                    _playerLightObject = await _assetService.LoadAssetAsync(AssetCategoryNames.UI, "PlayerLight", _cameraParent, _token, "Units");
                }
                _playerLight = GEntityUtils.GetComponent<Light>(_playerLightObject);

                if (_playerLight != null)
                {
                    _playerLight.color = new Color(1.0f, 0.9f, 0.8f, 1.0f);
                }
                _playerLight.intensity = 0;

                PlayerLightController plc = _playerLightObject.GetComponent<PlayerLightController>();
                if (plc != null)
                {
                    plc.enabled = false;
                }
            }

            MapType = mapData.Map.CrawlerMapTypeId;
            ICrawlerMapTypeHelper helper = GetMapHelper(MapType);

            _crawlerMapRoot = await helper.Enter(partyData, mapData, token);

            await LoadDungeonAssets(_crawlerMapRoot, token);

            _queuedMoves.Clear();

            MovePartyTo(partyData, _party.MapX, _party.MapZ, _party.MapRot, token);

            await _crawlerService.SaveGame();
        }

        private async Awaitable LoadDungeonAssets(CrawlerMapRoot mapRoot, CancellationToken token)
        {

            _assetService.LoadAsset(AssetCategoryNames.Dungeons, mapRoot.Map.DungeonArt.Art, OnLoadDungeonAssets, null, null, token);

            while (mapRoot.Assets == null)
            {
                await Awaitable.NextFrameAsync(token);
            }
        }

        private void OnLoadDungeonAssets(object obj, object data, CancellationToken token)
        {
            GEntity assetGo = obj as GEntity;

            if (assetGo == null)
            {
                return;
            }

            _crawlerMapRoot.Assets = assetGo.GetComponent<DungeonAssets>();
        }

        public void CleanMap()
        {
            if (_crawlerMapRoot != null && _crawlerMapRoot.Assets != null)
            {
                GEntityUtils.Destroy(_crawlerMapRoot.Assets.gameObject);
                _crawlerMapRoot.Assets = null;
            }
            if (_crawlerMapRoot != null)
            {
                GEntityUtils.Destroy(_crawlerMapRoot.gameObject);
                _crawlerMapRoot = null;
            }
        }

        private void UpdateCameraPos(CancellationToken token)
        {
            if (_crawlerMapRoot == null)
            {
                return;
            }

            if (_playerLight != null)
            {

                if (MapType == CrawlerMapTypes.Dungeon)
                {
                    _playerLight.intensity = 100;
                    _playerLight.range = 1000;
                }
                else
                {
                    _playerLight.intensity = 0;
                }
            }

            int bz = CrawlerMapConstants.BlockSize;

            if (_camera == null)
            {

                _camera = _cameraController.GetMainCamera();
                _camera.transform.localPosition = new Vector3(0, 0, -bz * 0.3f);
                _camera.transform.eulerAngles = new Vector3(0, 0, 0);
                _camera.farClipPlane = CrawlerMapConstants.BlockSize * 8;
                _camera.rect = new Rect(0, 0, 9f / 16f, 1);
                _camera.fieldOfView = 60f;
            }

            _cameraParent.transform.position = new Vector3(_crawlerMapRoot.DrawX, _crawlerMapRoot.DrawY, _crawlerMapRoot.DrawZ);
            _cameraParent.transform.eulerAngles = new Vector3(0, _crawlerMapRoot.DrawRot + 90, 0);
            _party.WorldPanel.SetPicture(null);
            if (_playerLightObject != null && _camera != null)
            {
                _playerLightObject.transform.position = _camera.transform.position;
            }
        }


        public bool UpdatingMovement()
        {
            return _updatingMovement;
        }
        private bool _updatingMovement = false;

        public void ClearMovement()
        {
            _queuedMoves.Clear();
            _updatingMovement = false;
        }

        const int maxQueuedMoves = 4;
        Queue<KeyCode> _queuedMoves = new Queue<KeyCode>();

        public async Awaitable UpdateMovement(CancellationToken token)
        {
            if (_queuedMoves.Count < maxQueuedMoves)
            {
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    _queuedMoves.Enqueue(KeyCode.W);
                }
                else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    _queuedMoves.Enqueue(KeyCode.S);
                }
                else if (Input.GetKeyDown(KeyCode.Q))
                {
                    _queuedMoves.Enqueue(KeyCode.Q);
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    _queuedMoves.Enqueue(KeyCode.E);
                }
                else if (Input.GetKeyDown(KeyCode.A))
                {
                    _queuedMoves.Enqueue(KeyCode.A);
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    _queuedMoves.Enqueue(KeyCode.D);
                }
            }

            if (_updatingMovement)
            {
                return;
            }

            _updatingMovement = true;
            AwaitableUtils.ForgetAwaitable(UpdateMovementInternal(token));
            await Task.CompletedTask;
        }

        private async Awaitable UpdateMovementInternal(CancellationToken token)
        {
            _updatingMovement = true;
            bool isRotation = false;
            while (_queuedMoves.TryDequeue(out KeyCode currCommand))
            {
                bool movedPosition = false;
                if (currCommand == KeyCode.W)
                {
                    await Move(1, 0, token);
                    movedPosition = true;
                }
                else if (currCommand == KeyCode.S)
                {
                    await Move(-1, 0, token);
                    movedPosition = true;
                }
                else if (currCommand == KeyCode.Q)
                {
                    await Move(0, -1, token);
                    movedPosition = true;
                }
                else if (currCommand == KeyCode.E)
                {
                    await Move(0, 1, token);
                    movedPosition = true;
                }
                else if (currCommand == KeyCode.A)
                {
                    await Rot(-1, token);
                    isRotation = true;
                }
                else if (currCommand == KeyCode.D)
                {
                    await Rot(1, token);
                    isRotation = true;
                }
                await _crawlerService.OnFinishMove(movedPosition, token);
                MovePartyTo(_party, _party.MapX, _party.MapZ, _party.MapRot, token, isRotation);
            }
            _updatingMovement = false;
        }

        public int GetBlockingBits(int sx, int sz, int ex, int ez, bool allowBuildingEntry)
        {
            ICrawlerMapTypeHelper helper = GetMapHelper(_crawlerMapRoot.Map.CrawlerMapTypeId);

            return helper.GetBlockingBits(_crawlerMapRoot, sx, sz, ex, ez, allowBuildingEntry);
        }

        const int moveFrames = 6;
        private async Awaitable Move(int forward, int left, CancellationToken token)
        {
            float sin = (float)Math.Round(MathF.Sin(-_party.MapRot * Mathf.PI / 180f));
            float cos = (float)Math.Round(Mathf.Cos(-_party.MapRot * Mathf.PI / 180f));

            float nx = cos * forward + sin * left;
            float nz = sin * forward - cos * left;

            float bs = CrawlerMapConstants.BlockSize;

            int sx = _party.MapX;
            int sz = _party.MapZ;

            int ex = (int)(_party.MapX + nx);
            int ez = (int)(_party.MapZ + nz);

            if (!_crawlerMapRoot.Map.Looping)
            {
                if (ex < 0 || ex >= _crawlerMapRoot.Map.Width ||
                    ez < 0 || ez >= _crawlerMapRoot.Map.Height)
                {
                    // Bonk
                    _dispatcher.Dispatch(new ShowFloatingText("Bonk!", EFloatingTextArt.Error));
                    return;
                }
            }

            int blockBits = GetBlockingBits(sx, sz, ex, ez, true);

            if (blockBits == WallTypes.Wall || blockBits == WallTypes.Secret)
            {
                // Bonk
                _dispatcher.Dispatch(new ShowFloatingText("Bonk!", EFloatingTextArt.Error));
                return;
            }

            if (blockBits == WallTypes.Building)
            {
                if (TryEnterBuilding(_crawlerMapRoot, ex, ez, token))
                {
                    MarkCellVisited(_crawlerMapRoot.Map.IdKey, ex, ez);
                    return;
                }
            }

            float endDrawX = _crawlerMapRoot.DrawX + nx * bs;
            float endDrawZ = _crawlerMapRoot.DrawZ + nz * bs;

            float startDrawX = _crawlerMapRoot.DrawX;
            float startDrawZ = _crawlerMapRoot.DrawZ;

            int frames = moveFrames;

            if (left != 0)
            {
                frames = frames * 1;
            }

            float dz = endDrawZ - startDrawZ;
            float dx = endDrawX - startDrawX;

            for (int frame = 1; frame < frames; frame++)
            {

                _crawlerMapRoot.DrawX = startDrawX + frame * dx / frames;
                _crawlerMapRoot.DrawZ = startDrawZ + frame * dz / frames;

                UpdateCameraPos(token);

                if (frame < frames - 1)
                {
                    await Awaitable.NextFrameAsync(token);
                }
            }

            ex = MathUtils.ModClamp(ex, _crawlerMapRoot.Map.Width);
            ez = MathUtils.ModClamp(ez, _crawlerMapRoot.Map.Height);

            _party.MapX = ex;
            _party.MapZ = ez;
        }

        private async Awaitable Rot(int delta, CancellationToken token)
        {

            float startRot = _party.MapRot;
            float endRot = _party.MapRot + delta * 90;

            float deltaRot = endRot - startRot;

            int frames = moveFrames * 1;

            for (int frame = 1; frame <= frames; frame++)
            {
                _crawlerMapRoot.DrawRot = startRot + deltaRot * frame / frames;
                UpdateCameraPos(token);
                if (frame < frames)
                {
                    await Awaitable.NextFrameAsync(token);
                }
            }

            SetFullRot(endRot);

        }

        private void SetFullRot(float endRot)
        {
            _party.MapRot = MathUtils.ModClamp((int)endRot, 360);
            _crawlerMapRoot.DrawRot = _party.MapRot;
        }


        private async Awaitable DrawNearbyMap(CancellationToken token)
        {
            if (_crawlerMapRoot == null)
            {
                return;
            }

            int bz = CrawlerMapConstants.BlockSize;

            int cx = (int)(_party.MapX);
            int cz = (int)(_party.MapZ);

            int bigViewRadius = ViewRadius + 2;

            if (_crawlerMapRoot.Map.CrawlerMapTypeId == CrawlerMapTypes.Outdoors)
            {
                bigViewRadius += 2;
            }

            int viewBufferSize = bigViewRadius + 2;

            for (int x = cx - bigViewRadius; x <= cx + bigViewRadius; x++)
            {
                int offsetX = Math.Abs(x - cx);
                for (int z = cz - bigViewRadius; z <= cz + bigViewRadius; z++)
                {
                    int offsetZ = Math.Abs(z - cz);

                    int worldX = x;
                    int worldZ = z;

                    int cellX = x;
                    int cellZ = z;

                    while (cellX < 0)
                    {
                        cellX += _crawlerMapRoot.Map.Width;
                    }
                    while (cellZ < 0)
                    {
                        cellZ += _crawlerMapRoot.Map.Height;
                    }

                    cellX %= _crawlerMapRoot.Map.Width;
                    cellZ %= _crawlerMapRoot.Map.Height;

                    UnityMapCell cell = _crawlerMapRoot.GetCell(cellX, cellZ);

                    if (_crawlerMapRoot.Map.CrawlerMapTypeId == CrawlerMapTypes.Outdoors &&
                        (offsetX >= ViewRadius + viewBufferSize ||
                        offsetZ >= ViewRadius + viewBufferSize))
                    {
                        if (cell.Content != null)
                        {
                            GEntityUtils.Destroy(cell.Content);
                            cell.Content = null;
                        }
                        continue;
                    }
                    if (_crawlerMapRoot.Map.Looping ||
                        _crawlerMapRoot.Map.CrawlerMapTypeId == CrawlerMapTypes.Dungeon ||
                            worldX >= 0 && worldX < _crawlerMapRoot.Map.Width &&
                            worldZ >= 0 && worldZ < _crawlerMapRoot.Map.Height)
                    {
                        if (cell.Content != null)
                        {
                            cell.Content.transform.position = new Vector3(worldX * bz, 0, worldZ * bz);
                        }
                        else
                        {
                            ICrawlerMapTypeHelper helper = GetMapHelper(_crawlerMapRoot.Map.CrawlerMapTypeId);

                            await helper.DrawCell(_world, _party, _crawlerMapRoot, cell, worldX, worldZ, token);
                        }
                    }
                }
            }
        }

        private bool TryEnterBuilding(CrawlerMapRoot mapRoot, int ex, int ez, CancellationToken token)
        {

            byte buildingId = mapRoot.Map.Get(ex, ez, CellIndex.Building);

            BuildingType btype = _gameData.Get<BuildingSettings>(null).Get(buildingId);

            if (btype == null)
            {
                return false;
            }

            MapCellDetail detail = mapRoot.Map.Details.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Map &&
            x.EntityId > 0 && x.X == ex && x.Z == ez);

            if (detail != null)
            {
                EnterBuilding(ECrawlerStates.MapExit, token, detail);
                return true;
            }

            if (btype.Name == "Equipment")
            {
                EnterBuilding(ECrawlerStates.Vendor, token);
                return true;
            }
            else if (btype.Name == "Guild")
            {
                EnterBuilding(ECrawlerStates.TavernMain, token);
                return true;
            }
            else if (btype.Name == "Trainer")
            {
                EnterBuilding(ECrawlerStates.TrainingMain, token);
                return true;
            }
            else if (btype.Name == "House")
            {
                EnterBuilding(ECrawlerStates.EnterHouse, token);
                return true;
            }
            return true;
        }
        private void EnterBuilding(ECrawlerStates state, CancellationToken token, object extraData = null)
        {
            SetFullRot(_party.MapRot + 180);
            UpdateCameraPos(token);
            _crawlerService.ChangeState(state, token, extraData);

        }

        public string GetBuildingArtPrefix()
        {
            return "Default";
        }

        public void MarkCurrentCellVisited()
        {
            if (_party == null || _party.Combat != null &&
                _crawlerMapRoot == null || _crawlerMapRoot.Map == null ||
                _party.MapId != _crawlerMapRoot.Map.IdKey ||
                _party.MapX < 0 || _party.MapZ < 0 ||
                _party.MapX >= _crawlerMapRoot.Map.Width ||
                _party.MapZ >= _crawlerMapRoot.Map.Height)
            {
                return;
            }

            MarkCellVisited(_party.MapId, _party.MapX, _party.MapZ);
        }

        public void MarkCellVisited(long mapId, int x, int z)
        {           
            if (_party == null || _world == null)
            {
                return;
            }

            if (_party.CompletedMaps.HasBit(mapId))
            {
                return;
            }

            CrawlerMap map = _world.GetMap(mapId);
            if (map == null)
            {
                return;
            }

            if (map.CrawlerMapTypeId == CrawlerMapTypes.City)
            {
                _party.CompletedMaps.SetBit(mapId);
                return;
            }

            CrawlerMapStatus status = _party.Maps.FirstOrDefault(x => x.MapId == mapId);
            if (status == null)
            {
                status = new CrawlerMapStatus() { MapId = mapId };
                _party.Maps.Add(status);
            }

            if (status.TotalCells < 1)
            {
                for (int mx = 0; mx < map.Width; mx++)
                {
                    for (int mz = 0; mz < map.Height; mz++)
                    {
                        if (map.Get(mx,mz,CellIndex.Terrain) > 0)
                        {
                            status.TotalCells++;
                        }
                    }
                }
            }

            int index = map.GetIndex(x, z);

            if (!status.Visited.HasBit(index))
            {
                status.CellsVisited++;
            }

            status.Visited.SetBit(index);

            if (status.CellsVisited >= status.TotalCells)
            {
                _party.CompletedMaps.SetBit(mapId);
                _party.Maps.Remove(status);
                _crawlerService.ChangeState(ECrawlerStates.GiveLoot, _token, new GiveLootParams()
                {
                    Header = "You Explored " + map.Name + "!",
                    LootScale = (1 + status.TotalCells/35),
                    BonusLevels = status.TotalCells/50,
                    MonsterExpCount = status.TotalCells/40,
                });
                return;
            }

            _party.CurrentMap.Visited.SetBit(index);
        }

        public bool PartyHasVisited(long mapId, int x, int z, bool thisRunOnly = false)
        {
            if (_party == null || _world == null)
            {
                return false;
            }

            if (_party.CompletedMaps.HasBit(mapId))
            {
                return true;
            }

            CrawlerMap map = _world.GetMap(mapId);
            if (map == null)
            {
                return false;
            }

            if (thisRunOnly)
            {
                return _party.CurrentMap.Visited.HasBit(map.GetIndex(x, z));
            }

            CrawlerMapStatus status = _party.Maps.FirstOrDefault(x => x.MapId == mapId);
            if (status == null)
            {
                return false;
            }

            int index = map.GetIndex(x, z);

            return status.Visited.HasBit(index);
        }

        public void MovePartyTo(PartyData partyData, int x, int z, int rot, CancellationToken token, bool rotationOnly = false)
        {
            if (_crawlerMapRoot == null)
            {
                return;
            }

            x = MathUtils.Clamp(0, x, _crawlerMapRoot.Map.Width - 1);
            z = MathUtils.Clamp(0, z, _crawlerMapRoot.Map.Height - 1);

            _crawlerMapRoot.DrawX = x * CrawlerMapConstants.BlockSize;
            _crawlerMapRoot.DrawZ = z * CrawlerMapConstants.BlockSize;
            _party.MapX = x;
            _party.MapZ = z;
            _party.MapRot = rot;
            UpdateCameraPos(token);
            MarkCurrentCellVisited();
            AwaitableUtils.ForgetAwaitable(DrawNearbyMap(token));
            _dispatcher.Dispatch(new ShowPartyMinimap() { Party = _party, PartyArrowOnly = rotationOnly });

        }

        private int IgnoreSecret(int wallVal)
        {
            return wallVal == WallTypes.Secret ? WallTypes.Wall : wallVal;
        }
        public FullWallTileImage GetMinimapWallFilename(CrawlerMap map, int x, int z)
        {
            StringBuilder sb = new StringBuilder();

            int index = 0;

            index += IgnoreSecret(map.NorthWall(x, (z + map.Height - 1) % map.Height));
            index *= 3;
            index += IgnoreSecret(map.EastWall((x + map.Width - 1) % map.Width, z));
            index *= 3;
            index += IgnoreSecret(map.NorthWall(x, z));
            index *= 3;
            index += IgnoreSecret(map.EastWall(x, z));

            FullWallTileImage img = TileImages[index];

            return img;
        }

        private bool _didInitWallImages = false;
        string _wallLetterList = "OWDW";
        private void CreateWallImageGrid()
        {
            if (_didInitWallImages)
            {
                return;
            }
            _didInitWallImages = true;
            TileImages = new FullWallTileImage[TileImageConstants.ArraySize];

            for (int i = 0; i < TileImageConstants.ArraySize; i++)
            {
                int div = 1;

                int[] vals = new int[TileImageConstants.ArraySize];
                for (int w = 0; w < TileImageConstants.WallCount; w++)
                {
                    vals[w] = (i / div) % 3;
                    div *= 3;
                }

                bool didFindRefImage = false;
                for (int k = 0; k < _refImages.Count; k++)
                {
                    WallTileImage wti = _refImages[k];

                    for (int rot = 0; rot < TileImageConstants.WallCount; rot++)
                    {
                        bool anyWrong = false;

                        for (int w = 0; w < TileImageConstants.WallCount; w++)
                        {
                            if (wti.WallIds[(rot + w) % 4] != vals[w])
                            {
                                anyWrong = true;
                                break;
                            }
                        }

                        if (anyWrong)
                        {
                            continue;
                        }

                        TileImages[i] = new FullWallTileImage() { Index = i, WallIds = vals, RefImage = wti, RotAngle = ((4 - rot) % 4) * 90, };

                        didFindRefImage = true;
                        break;
                    }
                }

                if (!didFindRefImage)
                {
                    WallTileImage wti = new WallTileImage() { WallIds = vals };
                    _refImages.Add(wti);
                    StringBuilder sb = new StringBuilder();
                    for (int w = 0; w < TileImageConstants.WallCount; w++)
                    {
                        sb.Append(_wallLetterList[vals[w]]);
                    }

                    wti.Filename = sb.ToString() + SpriteNameSuffixes.Wall;
                    TileImages[i] = new FullWallTileImage() { Index = i, WallIds = vals, RefImage = wti };
                }
            }

            StringBuilder outputSb = new StringBuilder();

            for (int i = 0; i < TileImages.Length; i++)
            {
                StringBuilder sb = new StringBuilder();

                for (int w = 0; w < TileImageConstants.WallCount; w++)
                {
                    sb.Append(_wallLetterList[TileImages[i].WallIds[w]]);
                }
            }

            if (false)
            {
                int size = 64;

                int wallThickness = 2;
                int doorThickness = 3;

                int doorStart = 20;
                int doorEnd = 63 - doorStart - 1;

                string crawlerMinimapAtlasPath = AppUtils.DataPath + "/FullAssets/Crawler/Atlas/CrawlerMinimapAtlas";
                foreach (WallTileImage wti in _refImages)
                {

                    Texture2D tex2d = new Texture2D(size, size, TextureFormat.ARGB32, false);

                    for (int x = 0; x < tex2d.width; x++)
                    {
                        for (int y = 0; y < tex2d.height; y++)
                        {
                            tex2d.SetPixel(x, y, new Color32(0, 0, 0, 0));
                        }
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        int sx = (i != 0 ? 0 : size - wallThickness - 1);
                        int sy = (i != 1 ? 0 : size - wallThickness - 1);

                        int ex = (i != 2 ? size - 1 : wallThickness);
                        int ey = (i != 3 ? size - 1 : wallThickness);

                        int dsx = (i == 0 ? sx - doorThickness : i == 2 ? wallThickness : doorStart);
                        int dex = (i % 2 == 0 ? dsx + doorThickness : doorEnd);

                        int dsy = (i == 3 ? wallThickness : i == 1 ? size - wallThickness - 1 - doorThickness : doorStart);
                        int dey = (i % 2 == 1 ? dsy + doorThickness : doorEnd);

                        if (wti.WallIds[i] == WallTypes.None)
                        {
                            continue;
                        }
                        for (int x = sx; x <= ex; x++)
                        {
                            for (int y = sy; y <= ey; y++)
                            {
                                tex2d.SetPixel(x, y, Color.white);
                            }
                        }


                        if (wti.WallIds[i] == WallTypes.Door)
                        {
                            for (int x = dsx; x <= dex; x++)
                            {
                                for (int y = dsy; y <= dey; y++)
                                {
                                    tex2d.SetPixel(x, y, Color.white);
                                }
                            }
                        }
                    }

                    byte[] bytes = tex2d.EncodeToPNG();

                    File.WriteAllBytes(crawlerMinimapAtlasPath + "/" + wti.Filename + ".png", bytes);
                }
            }
        }

        private FullWallTileImage[] TileImages { get; set; }

        private List<WallTileImage> _refImages { get; set; } = new List<WallTileImage>();
    }


    public class TileImageConstants
    {
        public const int WallCount = 4;
        public const int Images = 3;

        public const int ArraySize = 81; // 3^4
    }

    public class FullWallTileImage
    {

        public int Index { get; set; }
        public int[] WallIds { get; set; } = new int[TileImageConstants.WallCount];
        public long RotAngle { get; set; } = 0;

        public string ValText { get; set; }

        public WallTileImage RefImage { get; set; }
    }


    public class WallTileImage
    {
        public int[] WallIds { get; set; } = new int[TileImageConstants.WallCount];
        public string Filename { get; set; } = "OOOO";

    }

}