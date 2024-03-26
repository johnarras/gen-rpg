using Assets.Scripts.Buildings;
using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Maps.Services.Helpers;
using Assets.Scripts.Dungeons;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Dungeons.Settings;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using GEntity = UnityEngine.GameObject;

namespace Assets.Scripts.Crawler.Services.CrawlerMaps
{
    public class CrawlerMapService : ICrawlerMapService
    {
        IAssetService _assetService;
        ICameraController _cameraController;
        ICrawlerService _crawlerService;

        const int ViewRadius = 8;
        CrawlerMapRoot _crawlerMap = null;
        private CancellationToken _token;

        private GameObject _cameraParent = null;
        private Camera _camera = null;

        private PartyData _party;

        private Dictionary<ECrawlerMapTypes, ICrawlerMapTypeHelper> _mapTypeHelpers;

        public async Task Setup(GameState gs, CancellationToken token)
        {
            _cameraParent = _cameraController?.GetCameraParent();

            _mapTypeHelpers = ReflectionUtils.SetupDictionary<ECrawlerMapTypes, ICrawlerMapTypeHelper>(gs);

            _token = token;
            await Task.CompletedTask;
        }

        private ICrawlerMapTypeHelper GetHelper(ECrawlerMapTypes mapType)
        {
            if (_mapTypeHelpers.TryGetValue(mapType, out ICrawlerMapTypeHelper helper))
            {
                return helper;
            }
            return null;
        }
      

        public async UniTask EnterMap (UnityGameState gs, PartyData partyData, EnterCrawlerMapData mapData, CancellationToken token)
        {
            CleanMap();
            _party = partyData;


            ICrawlerMapTypeHelper helper = GetHelper(mapData.MapId == 1 ? ECrawlerMapTypes.City : ECrawlerMapTypes.Dungeon);

            _crawlerMap = await helper.Enter(gs, partyData, mapData, token);

            await LoadDungeonAssets(gs, _crawlerMap, token);

            await DrawNearbyMap(gs, token);

            await UpdateCameraPos(token);

            queuedMoves.Clear();

            await _crawlerService.SaveGame();
        }

        private async UniTask LoadDungeonAssets(UnityGameState gs, CrawlerMapRoot mapRoot, CancellationToken token)
        {

            _assetService.LoadAsset(gs, AssetCategoryNames.Dungeons, mapRoot.Map.DungeonArt.Art, OnLoadDungeonAssets, null, null, token);

            while (mapRoot.Assets == null)
            {
                await UniTask.NextFrame(token);
            }
        }

        private void OnLoadDungeonAssets(UnityGameState gs, object obj, object data, CancellationToken token)
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
        
        private async UniTask UpdateCameraPos(CancellationToken token)
        {
            await UniTask.CompletedTask;
            if (_crawlerMap == null)
            {
                return;
            }

            if (_cameraParent == null)
            {
                _cameraParent = _cameraController.GetCameraParent();
            }

            int bz = CrawlerMapConstants.BlockSize;

            if (_camera == null)
            {

                _camera = _cameraController.GetMainCamera();
                _camera.transform.localPosition = new Vector3(0,0,-bz*0.3f);
                _camera.transform.eulerAngles = new Vector3(0, 0, 0);
                _camera.farClipPlane = CrawlerMapConstants.BlockSize * 8;
                _camera.rect = new Rect(0, 0, 9f / 16f, 1);
                _camera.fieldOfView = 60f;
            }

            _cameraParent.transform.position = new Vector3(_crawlerMap.DrawX, _crawlerMap.DrawY, _crawlerMap.DrawZ);
            _cameraParent.transform.eulerAngles = new Vector3(0, _crawlerMap.DrawRot+90, 0);
            _party.WorldPanel.SetPicture(null);

        }


        public bool UpdatingMovement()
        {
            return _updatingMovement;
        }
        private bool _updatingMovement = false;



        const int maxQueuedMoves = 2;
        Queue<KeyCode> queuedMoves = new Queue<KeyCode>();

        public async UniTask UpdateMovement(UnityGameState gs, CancellationToken token)
        {
            if (queuedMoves.Count < maxQueuedMoves)
            {
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    queuedMoves.Enqueue(KeyCode.W);
                }
                else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    queuedMoves.Enqueue(KeyCode.S);
                }
                else if (Input.GetKeyDown(KeyCode.Q))
                {
                    queuedMoves.Enqueue(KeyCode.Q);
                }
                else if (Input.GetKey(KeyCode.E))
                {
                    queuedMoves.Enqueue(KeyCode.E);
                }
                else if (Input.GetKey(KeyCode.A))
                {
                    queuedMoves.Enqueue(KeyCode.A);
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    queuedMoves.Enqueue(KeyCode.D);
                }
            }

            if (_updatingMovement)
            {
                return;
            }

            _updatingMovement = true;
            while (queuedMoves.TryDequeue(out KeyCode currCommand))
            {
                if (currCommand == KeyCode.W)
                {
                    await Move(gs, 1, 0, token);
                }
                else if (currCommand == KeyCode.S)
                {
                    await Move(gs, -1, 0, token);
                }
                else if (currCommand == KeyCode.Q)
                {
                    await Move(gs, 0, -1, token);
                }
                else if (currCommand == KeyCode.E)
                {
                    await Move(gs, 0, 1, token);
                }
                else if (currCommand == KeyCode.A)
                {
                    await Rot(gs, -1, token);
                }
                else if (currCommand == KeyCode.D)
                {
                    await Rot(gs, 1, token);
                }
            }
            _updatingMovement = false;
        }


        const int moveFrames = 6;
        private async UniTask Move(UnityGameState gs, int forward, int left, CancellationToken token)
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
                if (ex < 0 || ex >= _crawlerMap.Map.XSize ||
                    ez < 0 || ez >= _crawlerMap.Map.ZSize)
                {
                    // Bonk
                    FloatingTextScreen.Instance.ShowError("Edge!");
                    return;
                }
            }

            ICrawlerMapTypeHelper helper = GetHelper(_crawlerMap.Map.MapType);


            int blockBits = helper.GetBlockingBits(gs, _crawlerMap, sx, sz, ex, ez);

            if (blockBits == WallTypes.Wall || blockBits == WallTypes.Secret)
            {
                // Bonk
                FloatingTextScreen.Instance.ShowError("Bonk!");
                return;
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

                await UpdateCameraPos(token);

                await UniTask.NextFrame(token);
            }

            ex = MathUtils.ModClamp(ex, _crawlerMap.Map.XSize);
            ez = MathUtils.ModClamp(ez, _crawlerMap.Map.ZSize);

            _crawlerMap.DrawX = ex * CrawlerMapConstants.BlockSize;
            _crawlerMap.DrawZ = ez * CrawlerMapConstants.BlockSize;
            _party.MapX = ex;
            _party.MapZ = ez;
            await UpdateCameraPos(token);
            await DrawNearbyMap(gs, token);

        }

        private async UniTask Rot(UnityGameState gs, int delta, CancellationToken token)
        {

            float startRot = _party.MapRot;
            float endRot = _party.MapRot + delta * 90;

            float deltaRot = endRot - startRot;

            int frames = moveFrames * 1;

            for (int frame = 1; frame <= frames; frame++)
            {              
                _crawlerMap.DrawRot = startRot + deltaRot * frame/frames;
                await UpdateCameraPos(token);
                if (frame < frames)
                {
                    await UniTask.NextFrame(token);
                }
            }

            _party.MapRot = MathUtils.ModClamp((int)endRot, 360);
            _crawlerMap.DrawRot = _party.MapRot;

            await UniTask.CompletedTask;
        }


        private async UniTask DrawNearbyMap(UnityGameState gs, CancellationToken token)
        {
            if (_crawlerMap == null)
            {
                return;
            }

            int bz = CrawlerMapConstants.BlockSize;

            int cx = (int)(_party.MapX);
            int cz = (int)(_party.MapZ);

            for (int x = cx-ViewRadius; x<= cx+ViewRadius; x++)
            {
                for (int z = cz-ViewRadius; z<= cz+ViewRadius; z++)
                {
                    int worldX = x;
                    int worldZ = z;

                    int cellX = x;
                    int cellZ = z;

                    while (cellX < 0)
                    {
                        cellX += _crawlerMap.Map.XSize;
                    }
                    while (cellZ < 0)
                    {
                        cellZ += _crawlerMap.Map.ZSize;
                    }

                    cellX %= _crawlerMap.Map.XSize;
                    cellZ %= _crawlerMap.Map.ZSize;

                    UnityMapCell cell = _crawlerMap.GetCell(cellX, cellZ);

                    if (_crawlerMap.Map.Looping || 
                        (worldX >= 0 && worldX < _crawlerMap.Map.XSize &&
                        worldZ >= 0 && worldZ < _crawlerMap.Map.ZSize))
                    {
                        if (cell.Content != null)
                        {
                            cell.Content.transform.position = new Vector3(worldX * bz, 0, worldZ * bz);
                        }
                        else
                        {
                            ICrawlerMapTypeHelper helper = GetHelper(_crawlerMap.Map.MapType);

                            await helper.DrawCell(gs, _crawlerMap, cell, worldX, worldZ, token);
                        }
                    }
                }
            }
        }
    }
}
