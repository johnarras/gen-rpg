using Assets.Scripts.Controllers;
using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Services.Helpers;
using Assets.Scripts.Dungeons;
using Assets.Scripts.UI.Crawler.States;
using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GEntity = UnityEngine.GameObject;

namespace Assets.Scripts.Crawler.Services.CrawlerMaps
{

    public class CrawlerMapGenData
    {
        public CrawlerWorld World;
        public ECrawlerMapTypes MapType;
        public int Level { get; set; } = 0;
        public long ZoneTypeId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Looping { get; set; }
        public long FromMapId { get; set; }
        public int FromMapX { get; set; }
        public int FromMapZ { get; set; }
    }


    public class CrawlerMapService : ICrawlerMapService
    {
        IAssetService _assetService;
        ICameraController _cameraController;
        ICrawlerService _crawlerService;
        private IDispatcher _dispatcher;
        private ILogService _logService;
        private IGameData _gameData;

        const int ViewRadius = 8;
        CrawlerMapRoot _crawlerMap = null;
        private CancellationToken _token;

        private GameObject _cameraParent = null;
        private Camera _camera = null;

        private PartyData _party;

        private SetupDictionaryContainer<ECrawlerMapTypes, ICrawlerMapTypeHelper> _mapTypeHelpers = new SetupDictionaryContainer<ECrawlerMapTypes, ICrawlerMapTypeHelper>();

        public static ECrawlerMapTypes MapType { get; set; } = ECrawlerMapTypes.None;

        private GameObject _playerLightObject = null;
        private Light _playerLight = null;
        public async Task Initialize(CancellationToken token)
        {
           
            _token = token;

            await Task.CompletedTask;
        }

        public ICrawlerMapTypeHelper GetHelper(ECrawlerMapTypes mapType)
        {
            if (_mapTypeHelpers.TryGetValue(mapType, out ICrawlerMapTypeHelper helper))
            {
                return helper;
            }
            return null;
        }
      

        

        public async Awaitable EnterMap (PartyData partyData, EnterCrawlerMapData mapData, CancellationToken token)
        {
            CleanMap();
            _party = partyData;

            if (mapData.Map == null)
            {
            }

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


            MapType = mapData.Map.MapType;
            ICrawlerMapTypeHelper helper = GetHelper(MapType);

            _crawlerMap = await helper.Enter(partyData, mapData, token);

            await LoadDungeonAssets(_crawlerMap, token);

            UpdateCameraPos(token);

            await DrawNearbyMap(token);

            _queuedMoves.Clear();

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

            _crawlerMap.Assets = assetGo.GetComponent<DungeonAssets>();
        }

        private void CleanMap()
        {
            if (_crawlerMap != null && _crawlerMap.Assets != null)
            {
                GEntityUtils.Destroy(_crawlerMap.Assets.gameObject);
                _crawlerMap.Assets = null;
            }
            if (_crawlerMap != null)
            {
                GEntityUtils.Destroy(_crawlerMap.gameObject);
                _crawlerMap = null;
            }
        }
        
        private void UpdateCameraPos(CancellationToken token)
        {
            if (_crawlerMap == null)
            {
                return;
            }

            if (_playerLight != null)
            {
               
                if (MapType == ECrawlerMapTypes.Dungeon)
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

            _cameraParent.transform.position = new Vector3(_crawlerMap.DrawX, _crawlerMap.DrawY, _crawlerMap.DrawZ);
            _cameraParent.transform.eulerAngles = new Vector3(0, _crawlerMap.DrawRot+90, 0);
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

            AwaitableUtils.ForgetAwaitable(UpdateMovementInternal(token));
            await Task.CompletedTask;
        }

        private async Awaitable UpdateMovementInternal(CancellationToken token)
        { 
            _updatingMovement = true;
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
                }
                else if (currCommand == KeyCode.D)
                {
                    await Rot(1, token);
                }
                await _crawlerService.OnFinishMove(movedPosition, token);
            }
            _updatingMovement = false;
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

            if (!_crawlerMap.Map.Looping)
            {
                if (ex < 0 || ex >= _crawlerMap.Map.Width ||
                    ez < 0 || ez >= _crawlerMap.Map.Height)
                {
                    // Bonk
                    _dispatcher.Dispatch(new ShowFloatingText("Edge!", EFloatingTextArt.Error));
                    return;
                }
            }

            ICrawlerMapTypeHelper helper = GetHelper(_crawlerMap.Map.MapType);


            int blockBits = helper.GetBlockingBits(_crawlerMap, sx, sz, ex, ez);

            if (blockBits == WallTypes.Wall || blockBits == WallTypes.Secret)
            {
                // Bonk
                _dispatcher.Dispatch(new ShowFloatingText("Bonk!", EFloatingTextArt.Error));
                return;
            }
            if (blockBits == WallTypes.Building)
            {
                if (TryEnterBuilding(_crawlerMap, ex, ez, token))
                {
                    return;
                }
            }

            float endDrawX = _crawlerMap.DrawX + nx * bs;
            float endDrawZ = _crawlerMap.DrawZ + nz * bs;

            float startDrawX = _crawlerMap.DrawX;
            float startDrawZ = _crawlerMap.DrawZ;

            int frames = moveFrames;

            if (left != 0)
            {
                frames = frames * 1;
            }

            float dz = endDrawZ - startDrawZ;
            float dx = endDrawX - startDrawX;

            for (int frame = 1; frame < frames; frame++)
            {

                _crawlerMap.DrawX = startDrawX + frame * dx / frames;
                _crawlerMap.DrawZ = startDrawZ + frame * dz / frames;

                UpdateCameraPos(token);

                if (frame < frames - 1)
                {
                    await Awaitable.NextFrameAsync(token);
                }
            }

            ex = MathUtils.ModClamp(ex, _crawlerMap.Map.Width);
            ez = MathUtils.ModClamp(ez, _crawlerMap.Map.Height);

            _crawlerMap.DrawX = ex * CrawlerMapConstants.BlockSize;
            _crawlerMap.DrawZ = ez * CrawlerMapConstants.BlockSize;
            _party.MapX = ex;
            _party.MapZ = ez;
            UpdateCameraPos(token);
            AwaitableUtils.ForgetAwaitable(DrawNearbyMap(token));

        }

        private async Awaitable Rot(int delta, CancellationToken token)
        {

            float startRot = _party.MapRot;
            float endRot = _party.MapRot + delta * 90;

            float deltaRot = endRot - startRot;

            int frames = moveFrames * 1;

            for (int frame = 1; frame <= frames; frame++)
            {              
                _crawlerMap.DrawRot = startRot + deltaRot * frame/frames;
                UpdateCameraPos(token);
                if (frame < frames)
                {
                    await Awaitable.NextFrameAsync(token);
                }
            }

            _party.MapRot = MathUtils.ModClamp((int)endRot, 360);
            _crawlerMap.DrawRot = _party.MapRot;

            
        }


        private async Awaitable DrawNearbyMap(CancellationToken token)
        {
            if (_crawlerMap == null)
            {
                return;
            }

            int bz = CrawlerMapConstants.BlockSize;

            int cx = (int)(_party.MapX);
            int cz = (int)(_party.MapZ);

            int BigViewRadius = ViewRadius * 2;

            int viewBufferSize = 2;

            for (int x = cx-BigViewRadius; x<= cx+BigViewRadius; x++)
            {
                int offsetX = Math.Abs(x - cx);
                for (int z = cz-BigViewRadius; z<= cz+BigViewRadius; z++)
                {
                    int offsetZ = Math.Abs(z - cz);

                    int worldX = x;
                    int worldZ = z;

                    int cellX = x;
                    int cellZ = z;

                    while (cellX < 0)
                    {
                        cellX += _crawlerMap.Map.Width;
                    }
                    while (cellZ < 0)
                    {
                        cellZ += _crawlerMap.Map.Height;
                    }

                    cellX %= _crawlerMap.Map.Width;
                    cellZ %= _crawlerMap.Map.Height;

                    UnityMapCell cell = _crawlerMap.GetCell(cellX, cellZ);

                    if (_crawlerMap.Map.MapType == ECrawlerMapTypes.Outdoors && 
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

                    if (_crawlerMap.Map.Looping || 
                        (worldX >= 0 && worldX < _crawlerMap.Map.Width &&
                        worldZ >= 0 && worldZ < _crawlerMap.Map.Height))
                    {
                        if (cell.Content != null)
                        {
                            cell.Content.transform.position = new Vector3(worldX * bz, 0, worldZ * bz);
                        }
                        else
                        {
                            ICrawlerMapTypeHelper helper = GetHelper(_crawlerMap.Map.MapType);

                            await helper.DrawCell(_crawlerMap, cell, worldX, worldZ, token);
                        }
                    }
                }
            }
        }

        private bool TryEnterBuilding(CrawlerMapRoot mapRoot, int ex, int ez, CancellationToken token)
        {

            byte info = mapRoot.Map.ExtraData[mapRoot.Map.GetIndex(ex, ez)];

            BuildingType btype = _gameData.Get<BuildingSettings>(null).Get(info);

            if (btype == null)
            {
                return false;
            }

            if (btype.Name == "Vendor")
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
        private void EnterBuilding(ECrawlerStates state, CancellationToken token)
        {
            _crawlerService.ChangeState(state, token);
        }

        public string GetBuildingArtPrefix()
        {
            return "Default";
        }

        public CrawlerMap Generate(CrawlerMapGenData genData)
        {

            ICrawlerMapTypeHelper helper = GetHelper(genData.MapType);

            return helper.Generate(genData);

        }
    }
}
