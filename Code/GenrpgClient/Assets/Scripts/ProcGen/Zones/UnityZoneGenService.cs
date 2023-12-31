using Cysharp.Threading.Tasks;
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

public class UnityZoneGenService : ZoneGenService
{
    public const string LoadMapURLSuffix = "/LoadMap";

    public static string LoadedMapId = "";

    public static Texture2D mapTexture;

    public const float ObjectScale = 1.0f;

    public static string GenErrorMsg = "";


    protected IScreenService _screenService;
    protected IMapTerrainManager _terrainManager;
    private IWebNetworkService _webNetworkService;
    private IRealtimeNetworkService _networkService;

    private CancellationTokenSource _mapTokenSource;
    private CancellationToken _mapToken;
    private CancellationToken _gameToken;
    private IAssetService _assetServce;

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

    public override void InstantiateMap(UnityGameState gs, string worldId)
    {
        _mapTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_gameToken);
        _mapToken = _mapTokenSource.Token;
        foreach (IService service in gs.loc.GetVals())
        {
            if (service is IMapTokenService tokenService)
            {
                tokenService.SetMapToken(_mapToken);
            }
        }
        InnerGenerate(gs, worldId, _mapToken).Forget();
    }

    protected async UniTask InnerGenerate(UnityGameState gs, string worldId, CancellationToken token)
    {
        if (gs.md.GeneratingMap)
        {
            return;
        }

        _screenService.CloseAll(gs);
        _screenService.Open(gs, ScreenId.Loading);
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: token);
        gs.md.GeneratingMap = true;
        RenderSettings.fog = false;
        await UniTask.NextFrame( cancellationToken: token);
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
            gs.loc.Resolve(gen);
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
            gs.Dispatch(showPercent);
            DateTime startTime = DateTime.UtcNow;
            gs.logger.Info("StageStart: " + currStep + " " + gen.GetType().Name + " Time: " + DateTime.UtcNow);
            try
            {
                await gen.Generate(gs, token);
                gs.logger.Info("StageEnd: " + currStep + " " + gen.GetType().Name + " Time: " + DateTime.UtcNow);
            }
            catch (Exception e)
            {
                ShowGenError(gs, e.Message + "\n-----------\n" + e.StackTrace);
                return;
            }
            DateTime endTime = DateTime.UtcNow;

            if (!String.IsNullOrEmpty(GenErrorMsg))
            {
                gs.logger.Error("GENERATION FAILURE: " + GenErrorMsg);
                _screenService.CloseAll(gs);
                _screenService.Open(gs, ScreenId.CharacterSelect);
                return;
            }

            output.Append("Stage: " + currStep + ": " + gen.GetType().Name + " -- " + (endTime - startTime).TotalSeconds + "\n");

            gen = null;

            await UniTask.NextFrame( cancellationToken: token);

            await UniTask.NextFrame( cancellationToken: token);
        }

        await UniTask.NextFrame( cancellationToken: token);

        await UniTask.NextFrame( cancellationToken: token);
        output.Append("------------------\n" +
            (DateTime.UtcNow - totalStartTime).TotalSeconds);

        gs.logger.Debug(output.ToString());



        // Wait for everything to get downloaded.

        gs.md.ClearGenerationData();


        await UniTask.NextFrame( cancellationToken: token);

        await UniTask.NextFrame( cancellationToken: token);


        gs.Dispatch(new MapIsLoadedEvent());
        gs.md.GeneratingMap = false;
        await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: token);
        PlayerObject.MoveAboveObstacles();
        await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: token);


        await UniTask.NextFrame( cancellationToken: token);
    }

    public override void ShowGenError(UnityGameState gs, string msg)
    {
        base.ShowGenError(gs, msg);
        gs.md.GeneratingMap = false;
    }




    public override void SetAllAlphamaps(UnityGameState gs, float[,,] alphaMaps, CancellationToken token)
    {
        if ( alphaMaps == null)
        {
            return;
        }
        for (int x = 0; x < gs.map.GetHwid(); x++)
        {
            for (int y = 0; y < gs.map.GetHhgt(); y++)
            {
                float alphaTotal = 0.0f;
                for (int i = 0; i < MapConstants.MaxTerrainIndex; i++)
                {
                    gs.md.alphas[x, y, i] = MathUtils.Clamp(0, gs.md.alphas[x, y, i], 1);
                    alphaTotal += gs.md.alphas[x, y, i];
                }
                if (alphaTotal <= 0)
                {
                    gs.md.alphas[x, y, MapConstants.BaseTerrainIndex] = 0.75f;
                    gs.md.alphas[x, y, MapConstants.DirtTerrainIndex] = 0.25f;
                }
                else
                {
                    for (int i = 0; i < MapConstants.MaxTerrainIndex; i++)
                    {
                        gs.md.alphas[x, y, i] /= alphaTotal;
                        gs.md.alphas[x, y, i] = MathUtils.Clamp(0, gs.md.alphas[x, y, i], 1);
                    }                   
                }
            }
        }


        for (int gx = 0; gx < gs.map.BlockCount; gx++)
        {
            for (int gy = 0; gy < gs.map.BlockCount; gy++)
            {
                TerrainData tdata = gs.md.GetTerrainData(gs, gx, gy);
                if (tdata == null)
                {
                    continue;
                }

                TerrainPatchData patch = gs.md.terrainPatches[gx, gy] as TerrainPatchData;

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
                        patch.mainZoneIds[x, y] = (byte)gs.md.mapZoneIds[startx + x, starty + y];
                        patch.subZoneIds[x, y] = (byte)gs.md.subZoneIds[startx + x, starty + y];
                        for (int index = 0; index < MapConstants.MaxTerrainIndex; index++)
                        {
                            patch.baseAlphas[x, y, index] = gs.md.alphas[x + startx, y + starty, index];
                        }
                    }
                }


                SetOnePatchAlphamaps(gs, gs.md.terrainPatches[gx, gy], token).Forget();
            }
        }

    }




    public override async UniTask SetOnePatchAlphamaps(UnityGameState gs, TerrainPatchData patch, CancellationToken token)
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

        MyRandom rand = new MyRandom(patch.X * 13 + patch.Y * 17 + gs.map.Seed / 3);


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
                if (UnityAssetService.LoadSpeed == LoadSpeed.Normal)
                {
                    await UniTask.NextFrame( cancellationToken: token);
                }
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
                        Zone zone = gs.map.Get<Zone>(patch.FullZoneIdList[z]);

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
                    await UniTask.NextFrame( cancellationToken: token);
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
            gs.logger.Info("Setting wrong terrainLayer sizes on " + patch.X + " -- " + patch.Y);
        }

        if (terr == null || terr.terrainData == null)
        {
            return;
        }

        terr.terrainData.SetAlphamaps(0, 0, newAlphas);
        terr.Flush();
        gs.md.SetOneTerrainNeighbors(gs, patch.X, patch.Y);
        patch.HaveSetAlphamaps = true;
    }


    public override void SetAllHeightmaps(UnityGameState gs, float[,] heights, CancellationToken token)
    {
        if (  heights == null)
        {
            return;
        }

        for (int gx = 0; gx < gs.map.BlockCount; gx++)
        {
            for (int gy = 0; gy < gs.map.BlockCount; gy++)
            {
                SetOnePatchHeightmaps(gs, gs.md.terrainPatches[gx,gy], heights);
            }
        }

    }

    public override void SetOnePatchHeightmaps (UnityGameState gs, TerrainPatchData patch, float[,] globalHeights, float[,] heightOverrides = null)
    { 
        if (gs == null || patch == null)
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
        if (gx < 0 || gy < 0 || gs.md == null || gx >= gs.map.BlockCount ||
            gy >= gs.map.BlockCount)
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

    static Material _loadedTerrainMaterial = null;
    static DateTime lastLoadClick = DateTime.UtcNow.AddMinutes(-1);
    public override void LoadMap(UnityGameState gsIn, LoadIntoMapCommand loadData)
    {
        UnityGameState gs = gsIn as UnityGameState;

        
#if !UNITY_EDITOR
        loadData.GenerateMap = false;       
#else
        if (string.IsNullOrEmpty(loadData.MapId) && !string.IsNullOrEmpty(InitClient.Instance.CurrMapId))
        {
            loadData.MapId = InitClient.Instance.CurrMapId;
        }
#endif

        if (string.IsNullOrEmpty(loadData.MapId))
        {
            gs.logger.Info("No world id chosen!");
            return;
        }

        if ((DateTime.UtcNow - lastLoadClick).TotalSeconds < 5)
        {
            return;
        }
        lastLoadClick = DateTime.UtcNow;

        if (gs.md != null)
        {
            _terrainManager.ClearPatches(gs);
            _terrainManager.ClearMapObjects(gs);
        }

        gs.md = new MapGenData();
        if (gs.ch != null)
        {
            gs.ch.MapId = loadData.MapId;
        }
        if (loadData.GenerateMap)
        {
            gs.logger.Info("Generating map " + loadData.MapId);
            UnityZoneGenService.LoadedMapId = "";
        }
        else
        {
            UnityZoneGenService.LoadedMapId = loadData.MapId;
            gs.logger.Info("Loading map " + loadData.MapId);
        }

        string postData = SerializationUtils.Serialize(loadData);
        
        _webNetworkService.SendClientWebCommand(loadData, _gameToken);
    }

    public override async UniTask OnLoadIntoMap(UnityGameState gs, LoadIntoMapResult data, CancellationToken token)
    {

        try
        {
            gs.ch = data.Char;

            _assetServce.SetWorldAssetEnv(data.WorldDataEnv);
            _networkService.SetRealtimeEndpoint(data.Host, data.Port, data.Serializer);
            _screenService.CloseAll(gs);

            gs.ch.SetGameDataOverrideList(data.OverrideList);


            gs.data.AddData(data.GameData);

            if (data == null || data.Map == null || data.Char == null)
            {
                _screenService.Open(gs, ScreenId.CharacterSelect);
                return;
            }
            if (!data.Generating && (data.Map.Zones == null || data.Map.Zones.Count < 1))
            {
                _screenService.Open(gs, ScreenId.CharacterSelect);
                return;
            }

            foreach (IUnitData dataSet in data.CharData)
            {
                dataSet.AddTo(gs.ch);
            }

            data.Stores?.AddTo(gs.ch);

            gs.map = data.Map;

            gs.spawns = new MapSpawnData() { Id = gs.map.Id.ToString() };

            if (gs.map == null)
            {
                _screenService.CloseAll(gs);
                _screenService.Open(gs, ScreenId.CharacterSelect);
                gs.logger.Message("Map failed to download");
                return;
            }

            InstantiateMap(gs, gs.map.Id);
        }
        catch (Exception e)
        {
            gs.logger.Exception(e, "OnLoadIntoMap");
        }
        await UniTask.CompletedTask;
    }


    public override void InitTerrainSettings(UnityGameState gs, int gx, int gy, int patchSize, CancellationToken token)
    {

        if (gs.md == null)
        {
            return;
        }

        TerrainPatchData patch = gs.md.GetTerrainPatch(gs, gx, gy);
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


        if (_loadedTerrainMaterial == null)
        {
            string matNameSuffix = "";
            //matNameSuffix = "Specular";
            // matNameSuffix = "Diffuse";
             matNameSuffix = "Standard";
            _loadedTerrainMaterial = AssetUtils.LoadResource<Material>("Materials/Terrain" + matNameSuffix);
        }
        //if (_loadedTerrainMaterial == null) _loadedTerrainMaterial = AssetUtils.LoadResource<Material>("Materials/TerrainStandard");
        terr.materialTemplate = _loadedTerrainMaterial;
        //terr.materialType = Terrain.MaterialType.BuiltInStandard;
        terr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Simple;
        terr.treeDistance = 150;
        terr.treeBillboardDistance = 100;
        terr.treeCrossFadeLength = 5;
        terr.treeMaximumFullLODCount = 30;
        terr.basemapDistance = 250;      
        terr.heightmapPixelError = 10;
        terr.detailObjectDensity = 1.0f;
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