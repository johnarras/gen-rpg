using Assets.Scripts.Crawler.GameEvents;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Zones.Settings;
using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Assets.Scripts.Crawler.TIlemaps
{
    public class CrawlerTilemapInitData
    {
        public long MapId;
        public int XOffset;
        public int ZOffset;
        public int Width;
        public int Height;
    }

    public class TilemapIndexes
    {
        public const int Terrain = 0;
        public const int Object = 1;
        public const int Walls = 2;
        public const int Dynamic = 3;
        public const int Max = 4;
    }

    public class SpriteNameSuffixes
    {
        public const string Terrain = "Terrain";
        public const string Object = "Object";
        public const string Wall = "Wall";
        public const string Building = "Building";

        public static readonly string[] AllTerrainSuffixes = { Terrain, Object };
    }

    public class CrawlerTilemap : BaseBehaviour
    {

        Sprite _blankSprite = null;
        private Image[,,] _tiles;

        private ICrawlerWorldService _worldService;
        private ICrawlerMapService _mapService;

        private CrawlerMap _map;

        private int _xpos = 0;
        private int _zpos = 0;

        public GameObject ImageParent;
        public int SpriteSize = 32;
        public int Width = 9;
        public int Height = 9;

        private SpriteAtlas _atlas;
        private Sprite[] _sprites;

        private Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

        public override void Init()
        {
            _dispatcher.AddEvent<ShowPartyMinimap>(this, OnShowPartyMinimap);
            base.Init();
            if (_spriteCache.Keys.Count < 1)
            {
                _assetService.LoadAssetInto(this, AssetCategoryNames.Atlas, "CrawlerMinimap", OnLoadAtlas, null, GetToken());
            }


        }

        private void InitImages(int width, int height, int spriteSize)
        {
            Width = width;
            Height = height;
            SpriteSize = spriteSize;

            GEntityUtils.DestroyAllChildren(ImageParent);
            _tiles = new Image[Width, Height, TilemapIndexes.Max];

            int xoffset = 0;
            int zoffset = 0;
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    for (int l = 0; l < TilemapIndexes.Max; l++)
                    {
                        GameObject go = new GameObject() { name = x + "." + z + "." + l };
                        go.transform.parent = ImageParent.transform;
                        go.transform.localScale = Vector3.one;
                        go.transform.localPosition = new Vector3((x - Width/2) * SpriteSize + xoffset, (z - Height/2) * SpriteSize + zoffset, l);
                        Image image = go.AddComponent<Image>();
                        image.useSpriteMesh = true;
                        image.maskable = false;
                        image.raycastTarget = false;
                        RectTransform rect = go.GetComponent<RectTransform>();
                        rect.sizeDelta = new Vector2(SpriteSize, SpriteSize);
                        _tiles[x,z,l] = image;
                    }
                }
            }
        }

        public async Awaitable Init(CrawlerTilemapInitData initData)
        {
            InitImages(initData.Width, initData.Height, SpriteSize);
            _xpos = initData.XOffset;
            _xpos = initData.ZOffset;

            _map = _worldService.GetMap(initData.MapId);
            await Task.CompletedTask;
        }

        private void OnLoadAtlas(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;
            if (go == null)
            {
                return;
            }

            SpriteAtlasContainer cont = go.GetComponent<SpriteAtlasContainer>();
            if (cont == null || cont.Atlas == null)
            {
                return;
            }

            _spriteCache = new Dictionary<string, Sprite>();

            _atlas = cont.Atlas;

            _sprites = new Sprite[_atlas.spriteCount];

            _atlas.GetSprites(_sprites);

            IReadOnlyList<ZoneType> zones = _gameData.Get<ZoneTypeSettings>(_gs.ch).GetData();
            IReadOnlyList<BuildingType> buildings = _gameData.Get<BuildingSettings>(_gs.ch).GetData();

            for (int i = 0; i < _sprites.Length; i++)
            {
                _sprites[i].name = _sprites[i].name.Replace("(Clone)", "");
                string spriteName = _sprites[i].name;

                bool foundSuffix = false;
                foreach (string suffix in SpriteNameSuffixes.AllTerrainSuffixes)
                {
                    if (spriteName.IndexOf(suffix) >= 0)
                    {
                        string zoneName = spriteName.Replace(suffix, "");

                        ZoneType zone = zones.FirstOrDefault(x => x.Icon == zoneName);

                        if (zone != null)
                        {
                            _spriteCache[spriteName] = _atlas.GetSprite(spriteName);

                            _spriteCache[suffix + zone.IdKey] = _atlas.GetSprite(spriteName);
                        }
                        foundSuffix = true;
                    }
                }

                if (!foundSuffix)
                {
                    if (spriteName.IndexOf(SpriteNameSuffixes.Building) >= 0)
                    {
                        string buildingName = spriteName.Replace(SpriteNameSuffixes.Building, "");

                        BuildingType btype = buildings.FirstOrDefault(x=>x.Icon == buildingName);
                        if (btype != null)
                        {
                            _spriteCache[buildingName] = _atlas.GetSprite(spriteName);
                            _spriteCache[SpriteNameSuffixes.Building + btype.IdKey] = _atlas.GetSprite(spriteName);
                        }
                    }

                    if (spriteName.IndexOf(SpriteNameSuffixes.Wall) >= 0)
                    {
                        for (int r = 0; r < 4; r++)
                        {
                            string wallName = spriteName + (r * 90);

                            _spriteCache[wallName] = _atlas.GetSprite(spriteName);
                        }
                        continue;
                    }

                    _spriteCache[spriteName] = _atlas.GetSprite(spriteName);

                }
            }

            _blankSprite = _atlas.GetSprite("Blank");

            InitImages(Width, Height, SpriteSize);
            ShowMap(_xpos, _zpos);
        }

        private void ShowBlank(int x, int z)
        {
            for (int l = 0; l < TilemapIndexes.Max; l++)
            {
                _tiles[x, z, l].sprite = _blankSprite;
            }
        }

        private PartyData _party = null;
        private void OnShowPartyMinimap(ShowPartyMinimap partyMap)
        {
            _party = partyMap.Party;
            if (_party == null)
            {
                return;
            }
            if (_map == null || _map.IdKey != _party.MapId)
            {
                _map = _worldService.GetMap(_party.MapId);
                ClearMap();
            }

            if (_spriteCache.TryGetValue("PlayerArrow", out Sprite playerArrow))
            {
                _tiles[Width/2,Height/2, TilemapIndexes.Dynamic].sprite = playerArrow;

                RectTransform rectTransform = _tiles[Width/2,Height/2, TilemapIndexes.Dynamic].GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    int mapRot = _party.MapRot;
                    if (mapRot % 180 == 0)
                    {
                        mapRot += 180;
                    }
                    rectTransform.localEulerAngles = new Vector3(0, 0, mapRot+90);
                }
            }

            if (partyMap.PartyArrowOnly)
            {
                return;
            }


            ShowMap(_party.MapX, _party.MapZ);
        }

        private void ClearMap()
        {
            if (_tiles == null)
            {
                return;
            }
            for (int x = 0; x < Width; x++)
            {
                for (int z = 0; z < Height; z++)
                {
                    for (int l = 0; l < TilemapIndexes.Max; l++)
                    {
                        _tiles[x, z, l].sprite = _blankSprite;
                    }
                }
            }
        }

        public void ShowMap (int xpos, int ypos)
        {
            if (_map == null)
            {
                return;
            }

            for (int ix = 0; ix < Width; ix++)
            {
                int x = (ix + xpos - Width/2);
                if (_map.Looping)
                {
                    if (x < 0)
                    {
                        x += _map.Width;
                    }
                    x = x % _map.Width;
                }

                for (int iz = 0; iz < Height; iz++)
                {
                    int z = (iz + ypos - Height/2);
                    if (_map.Looping)
                    {
                        if (z < 0)
                        {
                            z += _map.Height;
                        }
                        z = z % _map.Height;
                    }

                    if (x < 0 || x >= _map.Width || z < 0 || z >= _map.Height)
                    {
                        ShowBlank(ix,iz);
                        continue;
                    }

                    Vector3Int pos = new Vector3Int(ix,iz, 0);

                    string terrainName = SpriteNameSuffixes.Terrain + _map.Get(x, z, CellIndex.Terrain);
                    if (_spriteCache.TryGetValue( terrainName , out Sprite terrainSprite))
                    {
                        _tiles[ix, iz, TilemapIndexes.Terrain].sprite = terrainSprite;
                    }
                    else
                    {
                        _tiles[ix, iz, TilemapIndexes.Terrain].sprite = _blankSprite;
                    }

                    bool didSetObject = false;
                    long treeIndex = _map.Get(x, z, CellIndex.Tree);
                    if (treeIndex > 0)
                    {
                        if (_spriteCache.TryGetValue(SpriteNameSuffixes.Object + treeIndex, out Sprite objSprite))
                        {
                            _tiles[ix, iz, TilemapIndexes.Object].sprite = objSprite;
                            didSetObject = true;
                        }
                    }


                    if (_spriteCache.TryGetValue(SpriteNameSuffixes.Building + _map.Get(x, z, CellIndex.Building), out Sprite buildingSprite))
                    {
                        
                        _tiles[ix, iz, TilemapIndexes.Object].sprite = buildingSprite;
                        didSetObject = true;
                    }

                    if (!didSetObject)
                    {
                        _tiles[ix, iz, TilemapIndexes.Object].sprite = _blankSprite;
                    }

                    FullWallTileImage image = _mapService.GetMinimapWallFilename(_map, x, z);
                    if (image != null && image.RefImage.Filename == "OOOO" + SpriteNameSuffixes.Wall)
                    {
                        _tiles[ix, iz, TilemapIndexes.Walls].sprite = _blankSprite;
                    }
                    else
                    {
                        if (_spriteCache.TryGetValue(image.RefImage.Filename + image.RotAngle, out Sprite wallSprite))
                        {
                            _tiles[ix, iz, TilemapIndexes.Walls].sprite = wallSprite;


                            RectTransform rectTransform = _tiles[ix,iz,TilemapIndexes.Walls].GetComponent<RectTransform>();
                            if (rectTransform != null)
                            {
                                int mapRot = (int)image.RotAngle;
                                if (mapRot % 180 == 0)
                                {
                                    //mapRot += 180;
                                }
                                rectTransform.localEulerAngles = new Vector3(0, 0, mapRot);
                            }
                        }
                        else
                        {
                            _tiles[ix, iz, TilemapIndexes.Walls].sprite = _blankSprite;
                        }
                    }
                }
            }
        }
    }
}
