using Assets.Scripts.Controllers;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Services.Helpers;
using Assets.Scripts.Crawler.Tilemaps;
using Assets.Scripts.Dungeons;
using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.TimeOfDay.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.TimeOfDay.Services;
using Genrpg.Shared.Client.Assets.Services;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.StateHelpers.Exploring;
using UnityEngine;
using Genrpg.Shared.Tasks.Services;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.UI.Entities;
using Assets.Scripts.Buildings;
using Genrpg.Shared.Crawler.Loot.Settings;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.Crawler.Stats.Services;
using Assets.Scripts.Crawler.ClientEvents.WorldPanelEvents;
using Genrpg.Shared.Spells.Constants;

namespace Assets.Scripts.Crawler.Services.CrawlerMaps
{

    public class CrawlerMapService : ICrawlerMapService
    {
        class MovementKeyCode
        {
            public char Key { get; private set; } 
            public int RotationAmount { get; private set; }
            public int ForwardAmount { get; private set; }
            public int RightAmount { get; private set; }

            public MovementKeyCode(char key, int rotationAmount, int forwardAmount, int rightAmount)
            {
                Key = key;
                RotationAmount = rotationAmount;
                ForwardAmount = forwardAmount;  
                RightAmount = rightAmount;    
            }
        }

        private List<MovementKeyCode> _movementKeyCodes = new List<MovementKeyCode>();

        private IAssetService _assetService;
        private ICameraController _cameraController;
        private ICrawlerService _crawlerService;
        private IInputService _inputService;
        private IDispatcher _dispatcher;
        private ILogService _logService;
        private IGameData _gameData;
        private ICrawlerWorldService _worldService;
        private ITimeOfDayService _timeService;
        private IClientEntityService _clientEntityService;
        private ITaskService _taskService;
        private IClientGameState _gs;
        private IScreenService _screenService;
        private IClientRandom _rand;
        private IStatService _statService;
        private ICrawlerStatService _crawlerStatService;

        const int ViewRadius = 8;
        CrawlerMapRoot _crawlerMapRoot = null;
        private CancellationToken _token;

        private GameObject _cameraParent = null;
        private Camera _camera = null;

        private PartyData _party;
        private CrawlerWorld _world;

        private FullWallTileImage[] TileImages { get; set; }

        private List<WallTileImage> _refImages { get; set; } = new List<WallTileImage>();

        private SetupDictionaryContainer<long, ICrawlerMapTypeHelper> _mapTypeHelpers = new SetupDictionaryContainer<long, ICrawlerMapTypeHelper>();


        public string GetBGImageName()
        {
            if (_party == null)
            {
                _party = _crawlerService.GetParty();
            }

            if (_party.Combat != null)
            {
                return "Battlefield";
            }


            ZoneType zoneType = _worldService.GetCurrentZone(_party).Result;

            if (zoneType != null && !string.IsNullOrEmpty(zoneType.Icon))
            {
                return zoneType.Icon;
            }

            if (_crawlerMapRoot == null || _crawlerMapRoot.DungeonAssets == null || string.IsNullOrEmpty(_crawlerMapRoot.DungeonAssets.BGImageName))
            {
                return CrawlerClientConstants.DefaultWorldBG;
            }

            return _crawlerMapRoot.DungeonAssets.BGImageName;
        }

        private GameObject _playerLightObject = null;
        private Light _playerLight = null;
        public async Task Initialize(CancellationToken token)
        {

            _token = token;

            SetupMovementKeyCodes();
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

        private void SetupMovementKeyCodes()
        {
            _movementKeyCodes = new List<MovementKeyCode>
            {
                new MovementKeyCode('W', 0, 1, 0),
                new MovementKeyCode((char)273, 0, 1, 0),

                new MovementKeyCode('S', 0, -1, 0),
                new MovementKeyCode((char)274, 0, -1, 0),

                new MovementKeyCode('A', -1, 0, 0),
                new MovementKeyCode((char)276, -1, 0, 0),

                new MovementKeyCode('D', 1, 0, 0),
                new MovementKeyCode((char)275, 1, 0, 0),

                new MovementKeyCode('Q', 0, 0, -1),
                new MovementKeyCode('E', 0, 0, 1),
            };

        }

        public async Task EnterMap(PartyData partyData, EnterCrawlerMapData mapData, CancellationToken token)
        {
            _screenService.Open(ScreenId.Loading);

            while (_screenService.GetScreen(ScreenId.Loading) == null)
            {
                await Task.Delay(50);
            }


            CleanMap();
            _party = partyData;
            _party.InGuildHall = false;
            _world = await _worldService.GetWorld(_party.WorldId);

            if (_playerLight == null)
            {
                _cameraParent = _cameraController?.GetCameraParent();
                if (_playerLightObject == null)
                {
                    _playerLightObject = (GameObject)(await _assetService.LoadAssetAsync(AssetCategoryNames.UI, "PlayerLight", _cameraParent, _token, "Units"));
                }
                _playerLight = _clientEntityService.GetComponent<Light>(_playerLightObject);

                if (_playerLight != null)
                {
                    _playerLight.color = new UnityEngine.Color(1.0f, 0.9f, 0.8f, 1.0f);
                    _playerLight.intensity = 0;
                }

                PlayerLightController plc = _playerLightObject.GetComponent<PlayerLightController>();
                if (plc != null)
                {
                    plc.enabled = false;
                    plc.Init();
                }
            }

            ICrawlerMapTypeHelper helper = GetMapHelper(mapData.Map.CrawlerMapTypeId);

            _crawlerMapRoot = await helper.EnterMap(partyData, mapData, token);

            _crawlerMapRoot.MapTypeHelper = helper;
            await LoadDungeonAssets(_crawlerMapRoot, token);

            _queuedMoves.Clear();

            MovePartyTo(partyData, _party.MapX, _party.MapZ, _party.MapRot, token);

            _dispatcher.Dispatch(new CrawlerUIUpdate());
            await _crawlerService.SaveGame();

            _screenService.Close(ScreenId.Loading);
        }

        private async Task LoadDungeonAssets(CrawlerMapRoot mapRoot, CancellationToken token)
        {

            ZoneType ztype = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(mapRoot.Map.ZoneTypeId);

            if (ztype == null)
            {
                ztype = _gameData.Get<ZoneTypeSettings>(_gs.ch).GetData().FirstOrDefault(x => x.Name == "Outdoors");
            }

            string dungeonArtName = ztype.Art;

            string buildingArtFolder = _gameData.Get<BuildingArtSettings>(_gs.ch).Get(mapRoot.Map.BuildingArtId).Art;

            _assetService.LoadAsset(AssetCategoryNames.Dungeons, dungeonArtName, OnLoadDungeonAssets, null, null, token);

            _assetService.LoadAsset(AssetCategoryNames.Buildings, "CityAssets", OnLoadCityAssets, null, null, token, buildingArtFolder);

            while (mapRoot.DungeonAssets == null || mapRoot.DungeonMaterials == null || mapRoot.CityAssets == null)
            {
                await Task.Delay(1);
            }
        }
        private void OnLoadCityAssets(object obj, object data, CancellationToken token)
        {
            GameObject assetGo = obj as GameObject;

            if (assetGo == null)
            {
                return;
            }

            _crawlerMapRoot.CityAssets = assetGo.GetComponent<CityAssets>();
        }

        private void OnLoadDungeonAssets(object obj, object data, CancellationToken token)
        {
            GameObject assetGo = obj as GameObject;

            if (assetGo == null)
            {
                return;
            }

            _crawlerMapRoot.DungeonAssets = assetGo.GetComponent<DungeonAssets>();

            long materialSeed = _crawlerMapRoot.Map.ArtSeed / 5 + 1433;

            int matWeightSum = _crawlerMapRoot.DungeonAssets.Materials.Sum(x => x.Weight);

            int weightChosen = (int)materialSeed % matWeightSum;

            foreach (WeightedDungeonMaterials mat in _crawlerMapRoot.DungeonAssets.Materials)
            {
                weightChosen -= mat.Weight;

                if (weightChosen <= 0)
                {
                    _crawlerMapRoot.DungeonMaterials = mat.Materials;
                    break;
                }
            }

            // Get doormat for this level.

            List<WeightedMaterial> doorMats = _crawlerMapRoot.DungeonMaterials.GetMaterials(DungeonAssetIndex.Doors);

            long doorWeightSum = doorMats.Sum(x => x.Weight);

            long doorHash = _crawlerMapRoot.Map.ArtSeed / 3 + 317;

            long doorChosen = doorHash % doorWeightSum;

            foreach (WeightedMaterial wmat in doorMats)
            {
                doorChosen -= wmat.Weight;
                if (doorChosen <= 0)
                {
                    _crawlerMapRoot.DoorMat = wmat.Mat;
                    break;
                }
            }
        }

        public async Task OnClientResetCleanup(CancellationToken token)
        {
            CleanMap();
            await Task.CompletedTask;
        }


        public void CleanMap()
        {
            if (_crawlerMapRoot != null)
            {
                if (_crawlerMapRoot.DungeonAssets != null)
                {
                    _clientEntityService.Destroy(_crawlerMapRoot.DungeonAssets.gameObject);
                    _crawlerMapRoot.DungeonAssets = null;
                }
                if (_crawlerMapRoot.CityAssets != null)
                {
                    _clientEntityService.Destroy(_crawlerMapRoot.CityAssets.gameObject);
                    _crawlerMapRoot.CityAssets = null;
                }
                _crawlerMapRoot.DungeonMaterials = null;

                _clientEntityService.Destroy(_crawlerMapRoot.gameObject);
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

                if (InDungeonMap())
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
                
                // Uncomment for traditional BT layout.
                _camera.rect = new Rect(0, 0, 9f / 16f, 1);

                _camera.transform.localPosition = new Vector3(0, 0, -bz * 0.5f);
                _camera.transform.eulerAngles = new Vector3(0, 0, 0);
                _camera.farClipPlane = CrawlerMapConstants.BlockSize * 8;
                _camera.fieldOfView = 80f;
            }

            _cameraParent.transform.position = new Vector3(_crawlerMapRoot.DrawX, _crawlerMapRoot.DrawY, _crawlerMapRoot.DrawZ);
            _cameraParent.transform.eulerAngles = new Vector3(0, _crawlerMapRoot.DrawRot + 90, 0);
            _dispatcher.Dispatch(new SetWorldPicture(null,false));
            if (_playerLightObject != null && _camera != null)
            {
                _playerLightObject.transform.position = _camera.transform.position;
            }
        }


        private bool _updatingMovement = false;
        public bool UpdatingMovement()
        {
            return _updatingMovement;
        }

        public void ClearMovement()
        {
            _queuedMoves.Clear();
            _updatingMovement = false;
        }

        public void SetUpdatingMovement(bool updatingMovement)
        {
            _updatingMovement = updatingMovement;
        }

        const int maxQueuedMoves = 4;
        Queue<char> _queuedMoves = new Queue<char>();


        public async Task AddKeyInput(char keyChar, CancellationToken token)
        {
            if (_queuedMoves.Count < maxQueuedMoves)
            {
                if (_movementKeyCodes.Any(x=>x.Key == keyChar))
                {
                    _queuedMoves.Enqueue(keyChar);
                }
            }

            if (_updatingMovement)
            {
                return;
            }

            _updatingMovement = true;
            _taskService.ForgetTask(UpdateMovementInternal(token));
            await Task.CompletedTask;
        }


        public async Task UpdateMovement(CancellationToken token)
        {
            if (_queuedMoves.Count < maxQueuedMoves)
            {
                foreach (MovementKeyCode kc in _movementKeyCodes)
                {
                    if (_inputService.GetKeyDown(kc.Key))
                    {
                        _queuedMoves.Enqueue(kc.Key);
                        break;
                    }
                }
            }

            if (_updatingMovement)
            {
                return;
            }

            _updatingMovement = true;
            _taskService.ForgetTask(UpdateMovementInternal(token));
            await Task.CompletedTask;
        }

        private async Task UpdateMovementInternal(CancellationToken token)
        {
            _updatingMovement = true;
            bool isRotation = false;
            while (_queuedMoves.TryDequeue(out char currCommand))
            {
                MovementKeyCode kc = _movementKeyCodes.FirstOrDefault(x => x.Key == currCommand);
                if (kc == null)
                {
                    continue;
                }

                if (kc.RotationAmount == 0)
                {
                    await Move(kc.ForwardAmount, kc.RightAmount, token);
                }
                else
                {
                    await Rot(kc.RotationAmount, false, token);
                }

                await _crawlerService.OnFinishMove(kc.RotationAmount == 0, token);
                MovePartyTo(_party, _party.MapX, _party.MapZ, _party.MapRot, token, isRotation);

                await ApplyMagicBits(token);
               

                await _timeService.UpdateTime(_party, ECrawlerTimeUpdateTypes.Move);
            }
            _updatingMovement = false;
        }

        private async Task ApplyMagicBits(CancellationToken token)
        {
            if (HasMagicBit(_party.MapX, _party.MapZ, MapMagic.Spinner))
            {
                int rotateAmount = MathUtils.IntRange(-1, 2, _rand);
                if (rotateAmount != 0)
                {
                    await Rot(rotateAmount, true, token);
                    _dispatcher.Dispatch(new ShowPartyMinimap() { Party = _party, PartyArrowOnly = true });
                }
            }
            if (HasMagicBit(_party.MapX, _party.MapZ, MapMagic.NoMagic))
            {
                _party.Buffs.Clear();
            }
            if (HasMagicBit(_party.MapX, _party.MapZ, MapMagic.Drain))
            {
                CrawlerMapSettings mapSettings = _gameData.Get<CrawlerMapSettings>(_gs.ch);

                foreach (PartyMember pm in _party.GetActiveParty())
                {
                    if (pm.StatusEffects.HasBit(StatusEffects.Dead))
                    {
                        continue;
                    }

                    int healthLost = Math.Max(1, (int)(pm.Stats.Max(StatTypes.Health) * mapSettings.DrainHealthPercent));
                    healthLost = Math.Min(healthLost, pm.Stats.Curr(StatTypes.Health));
                    _crawlerStatService.Add(_party, pm, StatTypes.Health, StatCategories.Curr, -healthLost, ElementTypes.Physical);
                    if (pm.Stats.Curr(StatTypes.Health) < 1)
                    {
                        pm.StatusEffects.SetBit(StatusEffects.Dead);
                        continue;
                    }

                    int manaLost = Math.Max(1, (int)(pm.Stats.Max(StatTypes.Mana) * mapSettings.DrainManaPercent));
                    manaLost = Math.Min(manaLost, pm.Stats.Curr(StatTypes.Mana));
                    _crawlerStatService.Add(_party, pm, StatTypes.Mana, StatCategories.Curr, -manaLost);
                }
            }
        }

        public int GetBlockingBits(int sx, int sz, int ex, int ez, bool allowBuildingEntry)
        {
            ICrawlerMapTypeHelper helper = GetMapHelper(_crawlerMapRoot.Map.CrawlerMapTypeId);

            return helper.GetBlockingBits(_crawlerMapRoot, sx, sz, ex, ez, allowBuildingEntry);
        }

        const int moveFrames = 6;
        private async Task Move(int forward, int right, CancellationToken token)
        {
            float sin = (float)Math.Round(MathF.Sin(-_party.MapRot * Mathf.PI / 180f));
            float cos = (float)Math.Round(Mathf.Cos(-_party.MapRot * Mathf.PI / 180f));

            float nx = cos * forward + sin * right;
            float nz = sin * forward - cos * right;

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

            if (WallTypes.IsBlockingType(blockBits))
            {
                // Bonk
                _dispatcher.Dispatch(new ShowFloatingText("Bonk!", EFloatingTextArt.Error));
                return;
            }

            if (blockBits == WallTypes.Building)
            {
                if (TryEnterBuilding(_crawlerMapRoot, sx, sz, ex, ez, token))
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

            if (right != 0)
            {
                frames = frames * 1;
            }

            float dz = endDrawZ - startDrawZ;
            float dx = endDrawX - startDrawX;

            int dxgrid = ex - sx;
            int dzgrid = ez - sz;

            int cx = sx;
            int cz = sz;

            bool openEastDoor = dxgrid != 0;
            if (ex < sx)
            {
                cx = (sx + _crawlerMapRoot.Map.Width-1) % _crawlerMapRoot.Map.Width;
            }
            if (ez < sz)
            {
                cz = (sz + _crawlerMapRoot.Map.Height-1) % _crawlerMapRoot.Map.Height;
            }

            ClientMapCell mapCell = _crawlerMapRoot.GetCell(cx, cz);

            int assetPosition = (openEastDoor ? DungeonAssetPosition.EastWall : DungeonAssetPosition.NorthWall);

            DungeonAsset posAsset = mapCell.AssetPositions[assetPosition];

            if (posAsset != null)
            {
                if (posAsset.SetOpened(true))
                {
                    await Task.Delay(100);
                }
            }

            for (int frame = 1; frame < frames; frame++)
            {

                _crawlerMapRoot.DrawX = startDrawX + frame * dx / frames;
                _crawlerMapRoot.DrawZ = startDrawZ + frame * dz / frames;

                UpdateCameraPos(token);

                if (frame < frames - 1)
                {
                    await Task.Delay(1);
                }
            }

            if (posAsset != null)
            {
                posAsset.SetOpened(false);
            }

            ex = MathUtils.ModClamp(ex, _crawlerMapRoot.Map.Width);
            ez = MathUtils.ModClamp(ez, _crawlerMapRoot.Map.Height);

            _party.MapX = ex;
            _party.MapZ = ez;
        }

        private async Task Rot(int delta, bool fastRotate, CancellationToken token)
        {

            float startRot = _party.MapRot;
            float endRot = _party.MapRot + delta * 90;

            float deltaRot = endRot - startRot;

            int frames = moveFrames * 1;

            if (fastRotate)
            {
                frames = (int)(Math.Max(1, Math.Abs(delta)));
            }

            for (int frame = 1; frame <= frames; frame++)
            {
                _crawlerMapRoot.DrawRot = startRot + deltaRot * frame / frames;
                UpdateCameraPos(token);
                if (frame < frames)
                {
                    await Task.Delay(1);
                }
            }

            SetFullRot(endRot);

        }

        private void SetFullRot(float endRot)
        {
            _party.MapRot = MathUtils.ModClamp((int)endRot, 360);
            _crawlerMapRoot.DrawRot = _party.MapRot;
        }


        private async Task DrawNearbyMap(CancellationToken token)
        {

            try
            {
                if (_crawlerMapRoot == null)
                {
                    return;
                }

                int bz = CrawlerMapConstants.BlockSize;

                int cx = (int)(_party.MapX);
                int cz = (int)(_party.MapZ);

                int nonLoopExtraRadius = InDungeonMap()  ? 1 : 0;

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

                        int cellX = x;
                        int cellZ = z;

                        int worldX = x;
                        int worldZ = z;

                        int realCellX = (worldX + _crawlerMapRoot.Map.Width) % _crawlerMapRoot.Map.Width;
                        int realCellZ = (worldZ + _crawlerMapRoot.Map.Height) % _crawlerMapRoot.Map.Height;

                        ClientMapCell cell = _crawlerMapRoot.GetCell(cellX, cellZ);

                        if (_crawlerMapRoot.Map.CrawlerMapTypeId == CrawlerMapTypes.Outdoors &&
                            (offsetX >= ViewRadius + viewBufferSize ||
                            offsetZ >= ViewRadius + viewBufferSize))
                        {
                            if (cell.Content != null)
                            {
                                _clientEntityService.Destroy(cell.Content);
                                cell.Content = null;
                            }
                            continue;
                        }

                        if (!_crawlerMapRoot.Map.Looping &&


                            (worldX < -nonLoopExtraRadius || worldX >= _crawlerMapRoot.Map.Width + nonLoopExtraRadius ||
                            worldZ < -nonLoopExtraRadius || worldZ >= _crawlerMapRoot.Map.Height + nonLoopExtraRadius))
                        {
                            continue;
                        }


                        GameObject go = (GameObject)cell.Content;
                        if (go != null)
                        {
                            go.transform.position = new Vector3(worldX * bz, 0, worldZ * bz);
                        }
                        else
                        {
                            ICrawlerMapTypeHelper helper = GetMapHelper(_crawlerMapRoot.Map.CrawlerMapTypeId);

                            await helper.DrawCell(_world, _party, _crawlerMapRoot, cell, worldX, worldZ, realCellX, realCellZ, token);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "DrawNearbyMap");
            }
        }

        private bool TryEnterBuilding(CrawlerMapRoot mapRoot, int sx, int sz, int ex, int ez, CancellationToken token)
        {

            ForcedNextState nextState = _crawlerService.TryGetNextForcedState(mapRoot.Map, ex, ez);

            if (nextState != null)
            {

                SetFullRot(_party.MapRot + 180);
                UpdateCameraPos(token);
                _crawlerService.ChangeState(nextState.NextState, token, nextState.Detail);
                return true;
            }

            return false;
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

        public void MarkCellCleansed(int x, int z)
        {
            if (_party == null || _world == null)
            {
                return;
            }

            CrawlerMap map = _world.GetMap(_party.MapId);

            if(map == null)
            {
                return;
            }

            _party.CurrentMap.Cleansed.SetBit(map.GetIndex(x, z));
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

                CrawlerLootSettings lootSettings = _gameData.Get<CrawlerLootSettings>(_gs.ch);
                _crawlerService.ChangeState(ECrawlerStates.GiveLoot, _token, new GiveLootParams()
                {
                    Header = "You Explored " + map.Name + "!",
                    LootScale = (1 + status.TotalCells*lootSettings.ExploreLootScalePerCell),
                    BonusLevels = (int)(status.TotalCells*lootSettings.ExploreBonusLevelsPerCell),
                    MonsterExpCount = (int)(status.TotalCells*lootSettings.ExploreMonsterExpPerCell),
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
            _taskService.ForgetTask(DrawNearbyMap(token));
            _dispatcher.Dispatch(new ShowPartyMinimap() { Party = _party, PartyArrowOnly = rotationOnly });

        }

        private int IgnoreSecret(int wallVal)
        {
            if (wallVal == WallTypes.Barricade)
            {
                return WallTypes.None;
            }
            else if (wallVal == WallTypes.Secret)
            {
                return WallTypes.Wall;
            }
            return wallVal;
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

                    wti.Filename = sb.ToString() + SpriteNameCategories.Wall;
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
        }

        public bool InDungeonMap()
        {
            return _crawlerMapRoot != null && _crawlerMapRoot.Map != null && _crawlerMapRoot.Map.CrawlerMapTypeId == CrawlerMapTypes.Dungeon;
        }

        public bool IsIndoors()
        {
            return _crawlerMapRoot != null && _crawlerMapRoot.Map != null && _crawlerMapRoot.Map.IsIndoors;
        }

        public bool HasMagicBit(int x, int z, long bit)
        {
            return FlagUtils.IsSet(GetMagicBits(_party.MapId, x, z), bit);
        }

        public int GetMagicBits(long mapId, int x, int z)
        {
            if (_world == null)
            {
                return 0;
            }

            CrawlerMap map = _world.GetMap(mapId);

            if (map == null)
            {
                return 0;
            }

            int bits = map.Get(x, z, CellIndex.Magic);

            if (mapId == _party.MapId && _party.CurrentMap.Cleansed.HasBit(map.GetIndex(x,z)))
            {
                return 0;
            }

            return bits;
        }
    }
}