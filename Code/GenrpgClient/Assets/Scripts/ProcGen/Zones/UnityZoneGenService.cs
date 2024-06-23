
using System;
using System.Collections.Generic;
using System.Text;
using ClientEvents;
using System.Threading;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Interfaces;
using UI.Screens.Constants;
using Assets.Scripts.Tokens;
using Genrpg.Shared.Login.Messages.LoadIntoMap;
using Assets.Scripts.MapTerrain;
using UnityEngine; // Needed
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Characters.Utils;
using Genrpg.Shared.MapServer.Services;
using Assets.Scripts.ProcGen.RandomNumbers;
using System.Threading.Tasks;

public class UnityZoneGenService : ZoneGenService
{
    public const string LoadMapURLSuffix = "/LoadMap";

    public static string LoadedMapId = "";

    public const float ObjectScale = 1.0f;

    protected IScreenService _screenService;
    protected IMapTerrainManager _terrainManager;
    private IWebNetworkService _webNetworkService;
    private IRealtimeNetworkService _networkService;

    private CancellationTokenSource _mapTokenSource;
    private CancellationToken _mapToken;
    private CancellationToken _gameToken;
    private IAssetService _assetServce;
    private IPlayerManager _playerManager;

    public override void SetGameToken(CancellationToken token)
    {
        _gameToken = token;
    }

    public override void CancelMapToken()
    {
        _mapTokenSource?.Cancel();
        _mapTokenSource?.Dispose();
        _mapTokenSource = null;
    }

    public override void InstantiateMap(string worldId)
    {
        _mapTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_gameToken);
        _mapToken = _mapTokenSource.Token;
        foreach (IInjectable service in _gs.loc.GetVals())
        {
            if (service is IMapTokenService tokenService)
            {
                tokenService.SetMapToken(_mapToken);
            }
        }
        AwaitableUtils.ForgetAwaitable(InnerGenerate(worldId, _mapToken));
    }

    protected async Awaitable InnerGenerate(string worldId, CancellationToken token)
    {
        if (_md.GeneratingMap)
        {
            return;
        }

        _screenService.CloseAll();
        _screenService.Open(ScreenId.Loading);
        await Awaitable.WaitForSecondsAsync(0.1f, cancellationToken: token);
        _md.GeneratingMap = true;
        RenderSettings.fog = false;
        await Awaitable.NextFrameAsync(cancellationToken: token);
        // Now carry out the actual generation steps
        List<IZoneGenerator> genlist = new List<IZoneGenerator>();

        if (string.IsNullOrEmpty(LoadedMapId))
        {
            genlist.Add(new ClearMapData());

            genlist.Add(new GenerateMap());

            genlist.Add(new SetupMapData());

            genlist.Add(new SetBasicTerrainTextures());

            genlist.Add(new SetBaseTerrainHeights());

            genlist.Add(new AddZoneNoise());

            genlist.Add(new SetupMountainDecayPower());

            // Get the centerpoints
            genlist.Add(new AddZoneCenters());

            // Connect the zone centers with roads.
            genlist.Add(new ConnectZoneCenters());

            // Setup road distances first time so we know where these roads are.
            genlist.Add(new SetupRoadDistances());

            // Add secondary locations all over the map, not too close to caves or other locations.
            genlist.Add(new AddSecondaryLocations());

            // Connect secondary locations to nearest? road.
            genlist.Add(new ConnectSecondaryLocations());

            genlist.Add(new SetupRoadDistances());
                
            genlist.Add(new CreateConnectedZones());

            genlist.Add(new AddEdgeMountains());

            genlist.Add(new SetupNearbyZones());

            genlist.Add(new AddMiddleMountains());

            genlist.Add(new AddMountainNoise());

            genlist.Add(new SetMountainHeights());

            genlist.Add(new RemoveSetupZonePatches());

            genlist.Add(new SetupRoadDistances());

            genlist.Add(new SetupOverrideTerrainPatches());

            genlist.Add(new AddOutcroppings());

            genlist.Add(new AddCrevices());
            
            genlist.Add(new AddDetailHeights());

            genlist.Add(new AddRoadDips());

            genlist.Add(new RaiseOrLowerZones());

            genlist.Add(new AddOceans());

            genlist.Add(new AddLocationPatches());

            genlist.Add(new AddNPCs());

            genlist.Add(new AddMapMods());

            genlist.Add(new SetupTerrainPatches());

            genlist.Add(new AddBridges());

            genlist.Add(new SmoothRoadEdges());

            genlist.Add(new SmoothHeightsFinal());

            genlist.Add(new AddWater());

            genlist.Add(new SetfinalTerrainHeights());

            genlist.Add(new AddTrees());

            genlist.Add(new AddRocks());

            genlist.Add(new AddFences());

            genlist.Add(new AddResourceNodes());

            genlist.Add(new AddClutter());

            genlist.Add(new AddChests());

            genlist.Add(new AddPlants());

            genlist.Add(new AddRoadBorders());

            genlist.Add(new DirtyRoads());

            genlist.Add(new AddSteepnessTextures());

            genlist.Add(new AddMountainTextures());

            genlist.Add(new AddRandomDirt());

            genlist.Add(new SetTerrainTextures());

            genlist.Add(new SetBelowLandTerrainTextures());

            genlist.Add(new SmoothTerrainTexturesFinal());

            genlist.Add(new SetFinalTerrainTextures());

            genlist.Add(new CreateMinimap());

            genlist.Add(new CreatePathfindingData());

            genlist.Add(new AddMonsterSpawns());

            genlist.Add(new AddQuests());

            genlist.Add(new SetMapSpawnPoint());

            genlist.Add(new SaveMap());

            genlist.Add(new UploadMap());

            genlist.Add(new AfterGenerateMap());

        }
        else
        {
            genlist.Add(new ClearMapData());

            genlist.Add(new SetupMapData());

            genlist.Add(new AddMinGroundLevel());

            genlist.Add(new LoadMinimap());

            genlist.Add(new LoadPathfinding());

            genlist.Add(new SetFinalRenderSettings());

            genlist.Add(new AfterGenerateMap());

            genlist.Add(new AddPlayerToMap());

            genlist.Add(new LoadInitialData());

        }

        foreach (IZoneGenerator gen in genlist)
        {
            _gs.loc.Resolve(gen);
        }

        StringBuilder output = new StringBuilder();
        DateTime totalStartTime = DateTime.UtcNow;

        int currStep = 0;
        int totalSteps = genlist.Count;
        while (genlist.Count > 0)
        {
            currStep++;
            IZoneGenerator gen = genlist[0];
            genlist.RemoveAt(0);
            ShowLoadingPercentEvent showPercent = new ShowLoadingPercentEvent()
            {
                CurrStep = currStep,
                TotalSteps = totalSteps,
            };
            _dispatcher.Dispatch(showPercent);
            DateTime startTime = DateTime.UtcNow;
            _logService.Info("StageStart: " + currStep + " " + gen.GetType().Name + " Time: " + DateTime.UtcNow);
            try
            {
                await gen.Generate(token);
                _logService.Info("StageEnd: " + currStep + " " + gen.GetType().Name + " Time: " + DateTime.UtcNow);
            }
            catch (Exception e)
            {
                ShowGenError(e.Message + "\n-----------\n" + e.StackTrace);
                return;
            }
            DateTime endTime = DateTime.UtcNow;

            output.Append("Stage: " + currStep + ": " + gen.GetType().Name + " -- " + (endTime - startTime).TotalSeconds + "\n");

            gen = null;

            await Awaitable.NextFrameAsync(cancellationToken: token);

            await Awaitable.NextFrameAsync(cancellationToken: token);
        }

        await Awaitable.NextFrameAsync(cancellationToken: token);

        await Awaitable.NextFrameAsync(cancellationToken: token);
        output.Append("------------------\n" +
            (DateTime.UtcNow - totalStartTime).TotalSeconds);

        _logService.Debug(output.ToString());

        // Wait for everything to get downloaded.

        _md.ClearGenerationData();

        await Awaitable.NextFrameAsync(cancellationToken: token);

        await Awaitable.NextFrameAsync(cancellationToken: token);


        _dispatcher.Dispatch(new MapIsLoadedEvent());
        _md.GeneratingMap = false;
        await Awaitable.WaitForSecondsAsync(1.0f, cancellationToken: token);
        _playerManager.MoveAboveObstacles();
        await Awaitable.WaitForSecondsAsync(1.0f, cancellationToken: token);


        await Awaitable.NextFrameAsync(cancellationToken: token);
    }

    public override void ShowGenError(string msg)
    {
        base.ShowGenError(msg);
        _md.GeneratingMap = false;
    }




    public override void SetAllAlphamaps(float[,,] alphaMaps, CancellationToken token)
    {
        if (alphaMaps == null)
        {
            return;
        }
        for (int x = 0; x < _mapProvider.GetMap().GetHwid(); x++)
        {
            for (int y = 0; y < _mapProvider.GetMap().GetHhgt(); y++)
            {
                float alphaTotal = 0.0f;
                for (int i = 0; i < MapConstants.MaxTerrainIndex; i++)
                {
                    _md.alphas[x, y, i] = MathUtils.Clamp(0, _md.alphas[x, y, i], 1);
                    alphaTotal += _md.alphas[x, y, i];
                }
                if (alphaTotal <= 0)
                {
                    _md.alphas[x, y, MapConstants.BaseTerrainIndex] = 0.75f;
                    _md.alphas[x, y, MapConstants.DirtTerrainIndex] = 0.25f;
                }
                else
                {
                    for (int i = 0; i < MapConstants.MaxTerrainIndex; i++)
                    {
                        _md.alphas[x, y, i] /= alphaTotal;
                        _md.alphas[x, y, i] = MathUtils.Clamp(0, _md.alphas[x, y, i], 1);
                    }                   
                }
            }
        }


        for (int gx = 0; gx < _mapProvider.GetMap().BlockCount; gx++)
        {
            for (int gy = 0; gy < _mapProvider.GetMap().BlockCount; gy++)
            {
                TerrainData tdata = _terrainManager.GetTerrainData(gx, gy);
                if (tdata == null)
                {
                    continue;
                }

                TerrainPatchData patch = _terrainManager.GetTerrainPatch(gx, gy);

                if (patch == null)
                {
                    continue;
                }

                if (patch.baseAlphas == null)
                {
                    patch.baseAlphas = new float[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize, MapConstants.MaxTerrainIndex];
                }

                int startx = gy * (MapConstants.TerrainPatchSize - 1);
                int starty = gx * (MapConstants.TerrainPatchSize - 1);

                for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
                {
                    for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
                    {
                        patch.mainZoneIds[x, y] = (byte)_md.mapZoneIds[startx + x, starty + y];
                        patch.subZoneIds[x, y] = (byte)_md.subZoneIds[startx + x, starty + y];
                        for (int index = 0; index < MapConstants.MaxTerrainIndex; index++)
                        {
                            patch.baseAlphas[x, y, index] = _md.alphas[x + startx, y + starty, index];
                        }
                    }
                }


                AwaitableUtils.ForgetAwaitable(SetOnePatchAlphamaps(patch, token));
            }
        }

    }




    public override async Awaitable SetOnePatchAlphamaps(TerrainPatchData patch, CancellationToken token)
    {
        patch.HaveSetAlphamaps = false;
        Terrain terr = patch.terrain as Terrain;
        TerrainData terrainData = patch.terrainData as TerrainData;

        int zoneIdCount = patch.FullZoneIdList.Count;

        int currchannels = patch.TerrainTextureIndexes.Count;

        int size = MapConstants.TerrainPatchSize * MapConstants.AlphaMapsPerTerrainCell;
        float[,,] newAlphas = new float[size,size, currchannels];

        int pauseSize = MapConstants.TerrainPatchSize / 4;
        int pauseVal = pauseSize / 2;

        MyRandom rand = new MyRandom(patch.X * 13 + patch.Y * 17 + _mapProvider.GetMap().Seed / 3);

        if (patch.baseAlphas == null)
        {
            patch.baseAlphas = new float[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize, MapConstants.MaxTerrainIndex];
        }

        int firstZoneIndex = -1;
        int zoneId = -1;
        int zoneIndex = -1;
        int otherZoneCheckLength = 13;
        float[] oneCellAlphas = new float[currchannels];
        float[] cellZoneWeights = new float[zoneIdCount];
        float tempAlphaTotal = 0;
        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            if (x % pauseSize == pauseVal)
            {
                await Awaitable.NextFrameAsync(cancellationToken: token);
            }
            if (patch == null || patch.FullZoneIdList == null || patch.mainZoneIds == null)
            {
                continue;
            }

            for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
            {
                for (int c = 0; c < oneCellAlphas.Length; c++)
                {
                    oneCellAlphas[c] = 0;
                }
                zoneIndex = -1;
                zoneId = patch.mainZoneIds[x, y];
                tempAlphaTotal = 0;
                for (int i = 0; i < zoneIdCount; i++)
                {
                    cellZoneWeights[i] = 0;
                }

                for (int i = 0; i < zoneIdCount; i++)
                {
                    if (patch.FullZoneIdList[i] == zoneId)
                    {
                        firstZoneIndex = i;
                        cellZoneWeights[i] = 1;
                        zoneIndex = i;
                        break;
                    }
                }
                if (zoneIndex < 0)
                {
                    zoneIndex = 0;
                    cellZoneWeights[0] = 1;
                }

                int baseZoneId = patch.subZoneIds[x, y];
                bool adjacentToOtherZoneId = false;
                if (baseZoneId > 0)
                {
                    for (int xx = x-1; xx <= x+1; xx++)
                    {                     
                        if (xx < 0 || xx >= MapConstants.TerrainPatchSize)
                        {
                            continue;
                        }
                        for (int yy = y-1; yy <= y+1; yy++)
                        {
                            if (yy < 0 || yy >= MapConstants.TerrainPatchSize)
                            {
                                continue;
                            }
                            if (patch.subZoneIds[xx,yy] != baseZoneId)
                            {
                                adjacentToOtherZoneId = true;
                                break;
                            }
                        }
                        if (adjacentToOtherZoneId)
                        {
                            break;
                        }
                    }
                }

                if (baseZoneId > 0)
                {
                    if (patch.FullZoneIdList.Contains(baseZoneId))
                    {
                        float basePct = (adjacentToOtherZoneId ? 0.5f : 1);
                        for (int i = 0; i < cellZoneWeights.Length; i++)
                        {
                            if (patch.FullZoneIdList[i] == baseZoneId)
                            {
                                cellZoneWeights[i] = basePct;
                            }
                            else
                            {
                                cellZoneWeights[i] *= (1 - basePct);
                            }
                        }
                    }
                }
                else
                {
                    for (int times = 0; times < 3; times++)
                    {
                        int offsetX = x + rand.Next() % (2 * otherZoneCheckLength + 1) - otherZoneCheckLength;
                        int offsetY = y + rand.Next() % (2 * otherZoneCheckLength + 1) - otherZoneCheckLength;
                        if (offsetX >= 0 && offsetX < MapConstants.TerrainPatchSize &&
                            offsetY >= 0 && offsetY < MapConstants.TerrainPatchSize)
                        {
                            int offsetZoneIndex = -1;
                            for (int i = 0; i < zoneIdCount; i++)
                            {
                                if (patch.FullZoneIdList[i] == patch.mainZoneIds[offsetX, offsetY])
                                {
                                    offsetZoneIndex = i;
                                    break;
                                }
                            }
                            if (offsetZoneIndex > 0)
                            {
                                for (int i = 0; i < zoneIdCount; i++)
                                {
                                    cellZoneWeights[i] /= 2;
                                    if (i == offsetZoneIndex)
                                    {
                                        cellZoneWeights[i] += 0.5f;
                                    }
                                }
                            }
                        }
                    }
                }

                for (int z = 0; z < zoneIdCount; z++)
                {
                    if (cellZoneWeights[z] > 0)
                    {
                        Zone zone = _mapProvider.GetMap().Get<Zone>(patch.FullZoneIdList[z]);

                        int baseIndex = patch.TerrainTextureIndexes.IndexOf(zone.BaseTextureTypeId);

                        for (int index = 0; index < MapConstants.MaxTerrainIndex; index++)
                        {
                            long textureTypeId = zone.GetTerrainTextureByIndex(index);
                            int fullIndex = patch.TerrainTextureIndexes.IndexOf(textureTypeId);
                            oneCellAlphas[fullIndex] = patch.baseAlphas[x, y, index] * cellZoneWeights[z];
                            tempAlphaTotal += oneCellAlphas[fullIndex];
                        }
                    }
                }

                if (tempAlphaTotal < 0.01f)
                {
                    oneCellAlphas[0] = 1;
                    await Awaitable.NextFrameAsync(cancellationToken: token);
                }
                else
                { 
                    for (int c = 0; c < currchannels; c++)
                    {
                        oneCellAlphas[c] /= tempAlphaTotal;
                    }
                }

                for (int xx = 0; xx < MapConstants.AlphaMapsPerTerrainCell; xx++)
                {
                    for (int yy = 0; yy < MapConstants.AlphaMapsPerTerrainCell; yy++)
                    {
                        int xpos = x * MapConstants.AlphaMapsPerTerrainCell + xx;
                        int ypos = y * MapConstants.AlphaMapsPerTerrainCell + yy;

                        for (int i = 0; i < currchannels; i++)
                        {
                            newAlphas[xpos, ypos, i] = oneCellAlphas[i];
                        }
                    }
                }
            }
        }

        if (terr == null || terr.terrainData == null)
        {
            return;
        }

        if (terr.terrainData.terrainLayers == null ||
              terr.terrainData.terrainLayers.Length != newAlphas.GetLength(2))
        {
            _logService.Info("Setting wrong terrainLayer sizes on " + patch.X + " -- " + patch.Y);
        }

        if (terr == null || terr.terrainData == null)
        {
            return;
        }

        terr.terrainData.SetAlphamaps(0, 0, newAlphas);
        terr.Flush();
        _terrainManager.SetOneTerrainNeighbors(patch.X, patch.Y);
        patch.HaveSetAlphamaps = true;
    }


    public override void SetAllHeightmaps(float[,] heights, CancellationToken token)
    {
        if (heights == null)
        {
            return;
        }

        for (int gx = 0; gx < _mapProvider.GetMap().BlockCount; gx++)
        {
            for (int gy = 0; gy < _mapProvider.GetMap().BlockCount; gy++)
            {
                SetOnePatchHeightmaps(_terrainManager.GetTerrainPatch(gx, gy), heights);
            }
        }

    }

    public override void SetOnePatchHeightmaps (TerrainPatchData patch, float[,] globalHeights, float[,] heightOverrides = null)
    { 
        if (_gs == null || patch == null)
        {
            return;
        }

        Terrain terr = patch.terrain as Terrain;
        TerrainData terrainData = patch.terrainData as TerrainData;

        if (terr == null || terrainData == null)
        {
            return;
        }


        int gx = patch.X;
        int gy = patch.Y;
        if (gx < 0 || gy < 0 || _md == null || gx >= _mapProvider.GetMap().BlockCount ||
            gy >= _mapProvider.GetMap().BlockCount)
        {
            return;
        }
        if (heightOverrides == null || heightOverrides.GetLength(0) < MapConstants.TerrainPatchSize ||
            heightOverrides.GetLength(1) < MapConstants.TerrainPatchSize)
        {
            heightOverrides = null;
            if (globalHeights == null)
            {
                return;
            }
        }

        if (heightOverrides != null)
        {
            terrainData.SetHeights(0, 0, heightOverrides);
            return;
        }
            
        float[,] localHeights= new float[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];

        int startx = gy * (MapConstants.TerrainPatchSize-1);
        int starty = gx * (MapConstants.TerrainPatchSize-1);

        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
            {
                if (heightOverrides != null)
                {
                    localHeights[x, y] = heightOverrides[x, y];
                }
                else
                {
                    localHeights[x, y] = globalHeights[startx + x, starty + y];
                }
            }
        }
        terrainData.SetHeights(0, 0, localHeights);

    }

    static DateTime lastLoadClick = DateTime.UtcNow.AddMinutes(-1);
    public override void LoadMap(LoadIntoMapCommand loadData)
    {

   
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(loadData.MapId) && !string.IsNullOrEmpty(InitClient.EditorInstance.CurrMapId))
        {
            loadData.MapId = InitClient.EditorInstance.CurrMapId;
        }
#else
        loadData.GenerateMap = false;  
#endif

        if (string.IsNullOrEmpty(loadData.MapId))
        {
            _logService.Info("No world id chosen!");
            return;
        }

        if ((DateTime.UtcNow - lastLoadClick).TotalSeconds < 5)
        {
            return;
        }
        lastLoadClick = DateTime.UtcNow;


        if (loadData.GenerateMap)
        {
            _logService.Info("Generating map " + loadData.MapId);
            UnityZoneGenService.LoadedMapId = "";
        }
        else
        {
            UnityZoneGenService.LoadedMapId = loadData.MapId;
            _logService.Info("Loading map " + loadData.MapId);
        }

        string postData = SerializationUtils.Serialize(loadData);
        
        _webNetworkService.SendClientWebCommand(loadData, _gameToken);
    }

    public override async Awaitable OnLoadIntoMap(LoadIntoMapResult data, CancellationToken token)
    {

        try
        {
            _gs.ch = new Character(_repoService);
            CharacterUtils.CopyDataFromTo(data.Char, _gs.ch);
            _assetServce.SetWorldAssetEnv(data.WorldDataEnv);
            _networkService.SetRealtimeEndpoint(data.Host, data.Port, data.Serializer);
            _screenService.CloseAll();
            _terrainManager.ClearPatches();
            _terrainManager.ClearMapObjects();

            MinimapUI.SetTexture(null);
            _gs.ch.SetGameDataOverrideList(data.OverrideList);
            _gameData.AddData(data.GameData);

            if (data == null || data.Map == null || data.Char == null)
            {
                _screenService.Open(ScreenId.CharacterSelect);
                return;
            }

            _gs.ch.MapId = data.Map.Id;

            if (!data.Generating && (data.Map.Zones == null || data.Map.Zones.Count < 1))
            {
                _screenService.Open(ScreenId.CharacterSelect);
                return;
            }

            foreach (IUnitData dataSet in data.CharData)
            {
                dataSet.AddTo(_gs.ch);
            }

            data.Stores?.AddTo(_gs.ch);

            _mapProvider.SetMap(data.Map);

            _mapProvider.SetSpawns(new MapSpawnData() { Id = _mapProvider.GetMap().Id.ToString() });

            bool fixSeeds = false;

#if UNITY_EDITOR

            InitClient initComp = InitClient.EditorInstance;

            if (initComp != null &&
                initComp.WorldSize >= 3 &&
                initComp.ZoneSize >= 1 &&
                initComp.MapGenSeed > 0)
            {
                fixSeeds = true;
                if (string.IsNullOrEmpty(UnityZoneGenService.LoadedMapId))
                {
                    _mapProvider.GetMap().BlockCount = initComp.WorldSize;
                    _mapProvider.GetMap().ZoneSize = initComp.ZoneSize;
                    _mapProvider.GetMap().Seed = initComp.MapGenSeed;
                }
            }

#endif



            if (string.IsNullOrEmpty(UnityZoneGenService.LoadedMapId) && !fixSeeds)
            {
                _mapProvider.GetMap().Seed = (int)(DateTime.UtcNow.Ticks % 2000000000);
                foreach (Zone item in _mapProvider.GetMap().Zones)
                {
                    item.Seed = (int)(DateTime.UtcNow.Ticks % 1000000000 + item.IdKey * 235622);
                }
            }

            MapGenData mgd = new MapGenData();

            _gs.loc.Set<IMapGenData>(mgd);
            IClientRandom clientRandom = new ClientRandom(_mapProvider.GetMap().Seed);
            _gs.loc.Set(clientRandom);
            _gs.loc.ResolveSelf();

            _terrainManager.ClearPatches();

            if (_mapProvider.GetMap() == null)
            {
                _screenService.CloseAll();
                _screenService.Open(ScreenId.CharacterSelect);
                _logService.Message("Map failed to download");
                return;
            }

            InstantiateMap(_mapProvider.GetMap().Id);
        }
        catch (Exception e)
        {
            _logService.Exception(e, "OnLoadIntoMap");
        }

        await Task.CompletedTask;
    }


    public override void InitTerrainSettings(int gx, int gy, int patchSize, CancellationToken token)
    {

        if (_md == null)
        {
            return;
        }

        TerrainPatchData patch = _terrainManager.GetTerrainPatch(gx, gy);
        if (patch == null)
        {
            return;
        }

        Terrain terr = patch.terrain as Terrain;
        if (terr == null)
        {
            return;
        }
        LODGroup lg = terr.entity().GetComponent<LODGroup>();


        terr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Simple;
        terr.treeDistance = 400;
        terr.treeBillboardDistance = 400;
        terr.treeCrossFadeLength = 5;
        terr.treeMaximumFullLODCount = 500;
        terr.basemapDistance = 250;      
        terr.heightmapPixelError = 10;
        terr.detailObjectDensity = 0.5f;
        terr.detailObjectDistance = 200;
        terr.drawHeightmap = true;
        terr.drawTreesAndFoliage = true;
        terr.collectDetailPatches = false;
        terr.drawInstanced = true;
        terr.allowAutoConnect = true;
        terr.keepUnusedRenderingResources = true;
        
        if (terr.terrainData != null)
        {
            terr.terrainData.baseMapResolution = (MapConstants.TerrainPatchSize - 1)/2;
            terr.terrainData.heightmapResolution = patchSize;
            terr.terrainData.alphamapResolution = patchSize * MapConstants.AlphaMapsPerTerrainCell;
            terr.terrainData.SetDetailResolution(MapConstants.DetailResolution, MapConstants.DetailResolutionPerPatch);
        }
    }
}