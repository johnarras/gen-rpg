using Assets.Scripts.Buildings;
using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.UI.Services;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.ProcGen.Settings.Texturse;
using System.Threading;
using UnityEngine;
using GEntity = UnityEngine.GameObject;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{
    public abstract class BaseCrawlerMapTypeHelper : ICrawlerMapTypeHelper
    {
        protected IAssetService _assetService;
        protected IUIInitializable _uIInitializable;
        protected ILogService _logService;
        protected IGameData _gameData;

        public abstract ECrawlerMapTypes GetKey();

        public abstract UniTask<CrawlerMapRoot> Enter(UnityGameState gs, PartyData partyData, EnterCrawlerMapData mapData, CancellationToken token);

        public abstract int GetBlockingBits(UnityGameState gs, CrawlerMapRoot mapRoot, int sx, int sz, int ex, int ez);

        public abstract UniTask DrawCell(UnityGameState gs, CrawlerMapRoot mapRoot, UnityMapCell cell, int xpos, int zpos, CancellationToken token);

        protected void AddWallComponent(UnityGameState gs, GameObject asset, GameObject parent, Vector3 offset, Vector3 euler)
        {
            GameObject obj = GEntityUtils.FullInstantiate(gs, asset);
            GEntityUtils.AddToParent(obj, parent);
            obj.transform.localPosition = offset;
            obj.transform.eulerAngles = euler;
        }
        protected void OnDownloadBuilding(UnityGameState gs, object obj, object data, CancellationToken token)
        {
            GEntity go = obj as GEntity;

            if (go == null)
            {
                return;
            }

            CrawlerObjectLoadData loadData = data as CrawlerObjectLoadData;

            if (loadData == null || loadData.MapCell == null || loadData.BuildingType == null || loadData.MapRoot == null)
            {
                GEntityUtils.Destroy(go);
                return;
            }

            MapBuilding mapBuilding = GEntityUtils.GetComponent<MapBuilding>(go);

            if (mapBuilding != null)
            {
                mapBuilding.Init(loadData.BuildingType, new OnSpawn());
            }
            go.transform.eulerAngles = new Vector3(0, loadData.Angle, 0);

        }

        protected virtual void LoadTerrainTexture (UnityGameState gs, GameObject parent, long terrainTextureId, CancellationToken token)
        {
            TextureType ttype = _gameData.Get<TextureTypeSettings>(null).Get(terrainTextureId);

            if (ttype != null && !string.IsNullOrEmpty(ttype.Art))
            {
                _assetService.LoadAssetInto(gs, parent, AssetCategoryNames.TerrainTex, ttype.Art, OnDownloadTerrainTexture,parent, token);
            }
        }

        private void OnDownloadTerrainTexture(UnityGameState gs, object obj, object data, CancellationToken token)
        {

            GEntity parent = data as GEntity;

            if (parent == null)
            {
                return;
            }

            GEntity go = obj as GEntity;

            if (go == null)
            {
                return;
            }

            TextureList tlist = GEntityUtils.GetComponent<TextureList>(go);

            if (tlist == null || tlist.Textures == null || tlist.Textures.Count < 1 || tlist.Textures[0] == null)
            {
                GEntityUtils.Destroy(go);
                return;
            }


            GImage image = GEntityUtils.GetComponent<GImage>(parent);

            if (image == null)
            {
                GEntityUtils.Destroy(go);
                return;
            }
            
            Sprite spr = Sprite.Create(tlist.Textures[0], new Rect(0, 0, tlist.Textures[0].width, tlist.Textures[0].height), Vector2.zero);

            image.sprite = spr;
        }
    }
}
