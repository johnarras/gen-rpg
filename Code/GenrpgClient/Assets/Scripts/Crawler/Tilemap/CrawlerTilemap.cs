﻿using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Zones.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Client.Assets.Constants;
using System;
using System.Text;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Crawler.Buffs.Constants;
using Genrpg.Shared.Crawler.Maps.Settings;

namespace Assets.Scripts.Crawler.Tilemaps
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
        public const int SimpleMax = 2;
        public const int Max = 3;
    }

    public class SpriteNameCategories
    {
        public const string Terrain = "Terrain";
        public const string Object = "Object";
        public const string Wall = "Wall";
        public const string Building = "Building";
    }

    public class CrawlerTilemap : BaseBehaviour
    {

        public GImage PartyImage;
        public GameObject ImageParent;
        public GText MagicText;
        public int _tileSize = 32;
        public int Width = 9;
        public int Height = 9;
        public bool InitFromExplicitData = false;

        public GameObject ContentRoot;

        const int DefaultTileISize = 32;

        Sprite _blankSprite = null;
        Sprite _graySprite = null;
        Sprite _stairSprite = null;
        Sprite _darkGraySprite = null;
        Sprite _trapSprite = null;
        Sprite _monsterSprite = null;
        private Image[,,] _tiles;
        private GText[,] _text;



        private ICrawlerWorldService _worldService;
        private ICrawlerMapService _crawlerMapService;
        private ICrawlerService _crawlerService;

        private CrawlerMap _map = null;
        private PartyData _party = null;
        private CrawlerMapStatus _mapStatus = null;
        private bool _isBigMap = false;
        private int _mapDepth = TilemapIndexes.Max;
        private int _xCenter = 0;
        private int _zCenter = 0;

        private Color _whiteColor = Color.white;
        private Color _ghostColor = Color.gray;

        private SpriteAtlas _atlas;
        private Sprite[] _sprites;

        private Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

        public override void Init()
        {
            base.Init();
            if (InitFromExplicitData)
            {
                return;
            }
            AddListener<ShowPartyMinimap>(OnShowPartyMinimap);
            AddListener<ClearCrawlerTilemaps>(OnClearCrawlerTilemaps);
            if (_spriteCache.Keys.Count < 1)
            {
                _assetService.LoadAssetInto(this, AssetCategoryNames.Atlas, "CrawlerMinimap", OnLoadAtlas, null, GetToken());
            }
        }

        private void InitImages(int width, int height, int spriteSize)
        {
            Width = width;
            Height = height;
            _tileSize = spriteSize;

            int maxSize = Mathf.Max(width, height);

            while (maxSize > 48)
            {
                _tileSize /= 2;
                maxSize /= 2;
                _isBigMap = true;
            }

            if (PartyImage != null)
            {
                RectTransform rect = PartyImage.gameObject.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(_tileSize, _tileSize);
            }

            _clientEntityService.DestroyAllChildren(ImageParent);

            _mapDepth = (_isBigMap ? TilemapIndexes.SimpleMax : TilemapIndexes.Max);

            _tiles = new Image[Width, Height, _mapDepth];
            _text = new GText[Width, Height];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    for (int l = 0; l < _mapDepth; l++)
                    {
                        GameObject go = new GameObject() { name = x + "." + z + "." + l };
                        go.transform.parent = ImageParent.transform;
                        go.transform.localScale = Vector3.one;
                        go.transform.localPosition = new Vector3(GetTileOffSetPos(x,Width,_tileSize), GetTileOffSetPos(z, Height, _tileSize), l);
                        Image image = go.AddComponent<Image>();
                        image.useSpriteMesh = true;
                        image.maskable = false;
                        image.raycastTarget = false;
                        RectTransform rect = go.GetComponent<RectTransform>();
                        rect.sizeDelta = new Vector2(_tileSize, _tileSize);
                        _tiles[x,z,l] = image;
                        image.sprite = _blankSprite;
                    }
                }
            }
        }

        private void ShowText(int x, int z, string text)
        {
            if (_text[x,z] != null)
            {
                _uiService.SetText(_text[x, z], text);
            }
            else if (!string.IsNullOrEmpty(text))
            {
                _text[x, z] = _clientEntityService.FullInstantiate<GText>(MagicText);
                _text[x, z].transform.SetParent(ImageParent.transform);
                _text[x,z].transform.localScale = Vector3.one;
                _text[x,z].transform.localPosition = new Vector3(GetTileOffSetPos(x, Width, _tileSize), GetTileOffSetPos(z, Height, _tileSize), 10);

                _uiService.SetText(_text[x,z], text);   
            }
        }

        private float GetTileOffSetPos(int x, int mapSize, int tileSize)
        {
            return (x - mapSize / 2) * tileSize;
        }

        public async Awaitable Init(CrawlerTilemapInitData initData)
        {
            Width = initData.Width;
            Height = initData.Height;
            _xCenter = initData.XOffset + Width / 2;
            _zCenter = initData.ZOffset + Height / 2;

            _party = _crawlerService.GetParty();

            if (_party == null)
            {
                return;
            }

            _map = _worldService.GetMap(initData.MapId);

            _mapStatus = _party.Maps.FirstOrDefault(x => x.MapId == _map.IdKey);

            if (_spriteCache.Keys.Count < 1)
            {
                _assetService.LoadAssetInto(this, AssetCategoryNames.Atlas, "CrawlerMinimap", OnLoadAtlas, null, GetToken());
            }
            await Task.CompletedTask;
        }

        private string[] _allTerrainSuffixes = new string[] { SpriteNameCategories.Terrain, SpriteNameCategories.Object };
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

                foreach (string suffix in _allTerrainSuffixes)
                {
                    if (spriteName.IndexOf(suffix) >= 0)
                    {
                        string entityName = spriteName.Replace(suffix, "");

                        ZoneType zone = zones.FirstOrDefault(x => x.Icon == entityName);

                        BuildingType btype = buildings.FirstOrDefault(x => x.Icon == entityName);

                        if (zone != null)
                        {
                            _spriteCache[spriteName] = _atlas.GetSprite(spriteName);

                            _spriteCache[suffix + zone.IdKey] = _atlas.GetSprite(spriteName);
                        }

                        if (btype != null)
                        {
                            _spriteCache[btype.Icon] = _atlas.GetSprite(spriteName);
                            _spriteCache[SpriteNameCategories.Building + btype.IdKey] = _atlas.GetSprite(spriteName);
                        }
                    }
                }


                if (spriteName.IndexOf(SpriteNameCategories.Wall) >= 0)
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

            _blankSprite = _atlas.GetSprite("Blank");
            _graySprite = _atlas.GetSprite("Gray");
            _stairSprite = _atlas.GetSprite("Stairs");
            _trapSprite = _atlas.GetSprite("Trap");
            _monsterSprite = _atlas.GetSprite("Monster");
            _darkGraySprite = _atlas.GetSprite("DarkGray");

            InitImages(Width, Height, _tileSize);
            ShowMapWithCenter(_xCenter, _zCenter, false);
        }

        private void ShowBlank(int x, int z)
        {
            for (int l = 0; l < _mapDepth; l++)
            {
                _tiles[x, z, l].sprite = _blankSprite;
            }
        }
        private void ShowGray(int x, int z)
        {
            for (int l = 0; l < _mapDepth; l++)
            {
                _tiles[x, z, l].sprite = (l == 0 ? _graySprite : _blankSprite);
                _tiles[x, z, l].color = Color.white;
            }
        }

        private void ShowOutOfBounds(int x, int z)
        {
            for (int l = 0; l < _mapDepth; l++)
            {
                _tiles[x, z, l].sprite = (l == 0 ? _darkGraySprite : _blankSprite);
                _tiles[x, z, l].color = Color.white;
            }
            ShowText(x, z, null);
        }

        private void OnShowPartyMinimap(ShowPartyMinimap partyMap)
        {
            _party = partyMap.Party;
            if (_party == null)
            {
                return;
            }             

            if (_map == null || _map.IdKey != _party.MapId)
            {
                ClearMap();
                _map = _worldService.GetMap(_party.MapId);
            }

            if (_mapStatus == null || _mapStatus.MapId != _map.IdKey)
            {
                _mapStatus = _party.Maps.FirstOrDefault(x => x.MapId == _map.IdKey);
            }
            ShowMapWithCenter(_party.MapX, _party.MapZ, partyMap.PartyArrowOnly);
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
                    for (int l = 0; l < _mapDepth; l++)
                    {
                        _tiles[x, z, l].sprite = _blankSprite;
                    }
                }
            }
            _map = null;
            _mapStatus = null;
        }

        private const int GhostImageWidth = 1;

        private void ShowMapWithCenter(int xpos, int zpos, bool showPartyOnly)
        {
           
            StringBuilder sb = new StringBuilder();
            if (_party == null)
            {
                return;
            }

            if (_party.Buffs.Get(PartyBuffs.Mapping) == 0)
            {
                _clientEntityService.SetActive(ContentRoot, false);
            }
            else
            {
                _clientEntityService.SetActive(ContentRoot, true);
            }

            if (_map == null || _tiles == null)
            {
                return;
            }

            _xCenter = xpos;
            _zCenter = zpos;

            if (PartyImage != null)
            {
                int partyXCell = _party.MapX - _xCenter + Width/2;
                int partyZCell = _party.MapZ - _zCenter + Height/2;

                if (partyXCell >= 0 && partyXCell < Width &&
                    partyZCell >= 0 && partyZCell < Height)
                {

                    if (_spriteCache.TryGetValue("PlayerArrow", out Sprite playerArrow))
                    {
                        Image tile = _tiles[partyXCell, partyZCell, 0];
                        PartyImage.sprite = playerArrow;
                        PartyImage.transform.position = tile.transform.position;

                        RectTransform rectTransform = PartyImage.GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            int mapRot = _party.MapRot;
                            if (mapRot % 180 == 0)
                            {
                                mapRot += 90;
                            }
                            else
                            {
                                mapRot -= 90;
                            }
                            rectTransform.localEulerAngles = new Vector3(0, 0, mapRot);
                        }
                    }
                }
            }

            if (showPartyOnly)
            {
                return;
            }

            for (int ix = 0; ix < Width; ix++)
            {
                int x = (ix + xpos - Width / 2);
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
                    int z = (iz + zpos - Height / 2);
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
                        ShowOutOfBounds(ix, iz);
                        continue;
                    }

                    if (_mapDepth < TilemapIndexes.Max && x == Width / 2 && z == Height / 2)
                    {
                        continue;
                    }

                    bool showGhostImage = false;

                    if (_map.CrawlerMapTypeId != CrawlerMapTypes.City)
                    {
                        int index = _map.GetIndex(x, z);

                        if (
                            // false && 
                            _mapStatus != null && _mapStatus.MapId == _map.IdKey &&
                            !_party.CompletedMaps.HasBit(_map.IdKey) && !_mapStatus.Visited.HasBit(index))
                        {
                            if (_map.Get(x, z, CellIndex.Terrain) > 0 && !InitFromExplicitData &&
                                    Mathf.Abs(ix - Width / 2) <= GhostImageWidth && Mathf.Abs(iz - Height / 2) <= GhostImageWidth)
                            {
                                showGhostImage = true;
                            }
                            else
                            {

                                ShowGray(ix, iz);
                                ShowText(ix, iz, null);
                                continue;
                            }
                        }
                    }

                    Vector3Int pos = new Vector3Int(ix, iz, 0);

                    string terrainName = SpriteNameCategories.Terrain + _map.Get(x, z, CellIndex.Terrain);
                    if (_spriteCache.TryGetValue(terrainName, out Sprite terrainSprite))
                    {
                        if (terrainSprite != null)
                        {
                            _tiles[ix, iz, TilemapIndexes.Terrain].sprite = terrainSprite;
                        }
                        else
                        {
                            _tiles[ix, iz, TilemapIndexes.Terrain].sprite = _blankSprite;
                        }
                    }
                    else
                    {
                        if (_party.CompletedMaps.HasBit(_map.IdKey))
                        {
                            _tiles[ix, iz, TilemapIndexes.Terrain].sprite = _graySprite;
                        }
                        else
                        {
                            _tiles[ix, iz, TilemapIndexes.Terrain].sprite = _blankSprite;
                        }
                    }

                    bool didSetObject = false;
                    long treeIndex = _map.Get(x, z, CellIndex.Tree);
                    if (treeIndex > 0)
                    {
                        if (_spriteCache.TryGetValue(SpriteNameCategories.Object + treeIndex, out Sprite objSprite))
                        {
                            _tiles[ix, iz, TilemapIndexes.Object].sprite = objSprite;
                            didSetObject = true;
                        }
                    }


                    if (_spriteCache.TryGetValue(SpriteNameCategories.Building + _map.Get(x, z, CellIndex.Building), out Sprite buildingSprite))
                    {
                        _tiles[ix, iz, TilemapIndexes.Object].sprite = buildingSprite;
                        didSetObject = true;
                    }
                    if (_map.CrawlerMapTypeId == CrawlerMapTypes.Dungeon && !showGhostImage)
                    {
                        MapCellDetail detail = _map.Details.FirstOrDefault(d => d.X == x && d.Z == z);

                        if (detail != null && detail.EntityTypeId == EntityTypes.Map)
                        {
                            _tiles[ix, iz, TilemapIndexes.Object].sprite = _stairSprite;
                            didSetObject = true;
                        }

                        if (!didSetObject)
                        {
                            int encounterBits = _map.Get(x, z, CellIndex.Encounter);

                            if (encounterBits == MapEncounters.Monsters)
                            {
                                _tiles[ix, iz, TilemapIndexes.Object].sprite = _monsterSprite;
                                didSetObject = true;
                            }
                            else if (encounterBits == MapEncounters.Trap)
                            {
                                _tiles[ix, iz, TilemapIndexes.Object].sprite = _trapSprite;
                                didSetObject = true;
                            }
                        }
                    }

                    if (!didSetObject)
                    {
                        _tiles[ix, iz, TilemapIndexes.Object].sprite = _blankSprite;
                    }


                    if (_mapDepth > TilemapIndexes.Walls)
                    {
                        FullWallTileImage image = _crawlerMapService.GetMinimapWallFilename(_map, x, z);
                        if (image != null && image.RefImage.Filename == "OOOO" + SpriteNameCategories.Wall)
                        {
                            _tiles[ix, iz, TilemapIndexes.Walls].sprite = _blankSprite;
                        }
                        else
                        {
                            if (_spriteCache.TryGetValue(image.RefImage.Filename + image.RotAngle, out Sprite wallSprite))
                            {
                                _tiles[ix, iz, TilemapIndexes.Walls].sprite = wallSprite;


                                RectTransform rectTransform = _tiles[ix, iz, TilemapIndexes.Walls].GetComponent<RectTransform>();
                                if (rectTransform != null)
                                {
                                    int mapRot = (int)image.RotAngle;
                                    rectTransform.localEulerAngles = new Vector3(0, 0, mapRot);
                                }
                            }
                            else
                            {
                                _tiles[ix, iz, TilemapIndexes.Walls].sprite = _blankSprite;
                            }
                        }
                    }

                    for (int i = 0; i < _tiles.GetLength(2); i++)
                    {
                        _tiles[ix, iz, i].color = (showGhostImage ? _ghostColor : _whiteColor);
                    }

                    int magicBits = _crawlerMapService.GetMagicBits(_map.IdKey, x, z);


                    if (showGhostImage || magicBits == 0)
                    {
                        ShowText(ix, iz, null);
                    }
                    else
                    {
                        IReadOnlyList<MapMagicType> magicTypes = _gameData.Get<MapMagicSettings>(_gs.ch).GetData();
                        sb.Clear();
                        for (int i = 0; i < magicTypes.Count; i++)
                        {
                            if (FlagUtils.IsSet(magicBits, (1 << (int)magicTypes[i].IdKey)))
                            {
                                sb.Append(magicTypes[i].MapSymbol);
                            }
                        }
                        ShowText(ix, iz, sb.ToString());
                    }
                
                }
            }
        }

        private void OnClearCrawlerTilemaps(ClearCrawlerTilemaps clearMaps)
        {
            ClearMap();
        }
    }
}
