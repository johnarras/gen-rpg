
using System;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using Cysharp.Threading.Tasks;
using System.Threading;
using Assets.Scripts.MapTerrain;
using UnityEngine; // Needed
using Genrpg.Shared.ProcGen.Settings.Texturse;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.GameSettings;


public class TerrainTextureData
{
    public TextureType TexType;

    public Texture2D RegTexture;
    public Texture2D NormTexture;
    public GEntity TextureContainer;
    public int InstanceCount = 0;
    public TerrainLayer TerrLayer;
}
public class DownloadTerrainTextureData
{
    public TextureType TexType;
    public int TextureIndex;
    public Terrain Terr;
}


public interface ITerrainTextureManager : IInjectable
{
    UniTask SetOneTerrainPatchLayers(UnityGameState gs, TerrainPatchData patch, CancellationToken token, bool allAtOnce = false);
    UniTask DownloadAllTerrainTextures(UnityGameState gs, CancellationToken token);
}

public class TerrainTextureManager : ITerrainTextureManager
{

    private ILogService _logService;
    private IGameData _gameData;
    private IAssetService _assetService;
    private IMapTerrainManager _terrainManager;

    public async UniTask SetOneTerrainPatchLayers(UnityGameState gs, TerrainPatchData patch, CancellationToken token, bool allAtOnce = false)
    {
        Terrain terr = patch.terrain as Terrain;
        if (terr == null || terr.terrainData == null)
        {
            return;
        }

        List<long> zoneIds = patch.FullZoneIdList;
        if (zoneIds == null || zoneIds.Count < 1)
        {
            zoneIds = new List<long>();
            zoneIds.Add(1);
        }

        patch.TerrainTextureIndexes = new List<long>();

        foreach (long zoneId in zoneIds)
        {
            Zone zone = gs.map.Get<Zone>(zoneId);

            for (int i = 0; i < MapConstants.MaxTerrainIndex; i++)
            {
                if (!patch.TerrainTextureIndexes.Contains(zone.GetTerrainTextureByIndex(i)))
                {
                    patch.TerrainTextureIndexes.Add(zone.GetTerrainTextureByIndex(i));
                }
            }
        }
        TerrainLayer[] terrainLayers = new TerrainLayer[patch.TerrainTextureIndexes.Count];

        for (int i = 0; i < terrainLayers.Length; i++)
        {
            terrainLayers[i] = new TerrainLayer();
        }

        terr.terrainData.terrainLayers = terrainLayers;

        await DelayLoadSplats(gs, terr, patch, allAtOnce, token);
    }

    private async UniTask DelayLoadSplats(UnityGameState gs, Terrain terr, TerrainPatchData patch, bool allAtOnce, CancellationToken token)
    {
        for (int l = 0; l < patch.TerrainTextureIndexes.Count; l++)
        {
            if (!allAtOnce)
            {
                await UniTask.NextFrame(cancellationToken: token);
            }

            SetupTerrainTexture(gs, terr, patch.TerrainTextureIndexes[l], l, token);

        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gs"></param>
    /// <param name="terr"></param>
    /// <param name="textureId"></param>
    /// <param name="index">current index taking into account the zone offset of 4 per zone</param>
	private void SetupTerrainTexture(UnityGameState gs, Terrain terr, long textureId, int index, CancellationToken token)
    {
        TerrainTextureData textureData = GetTerrainTextureCacheData(gs, textureId);
        if (textureData != null)
        {
            SetNewTerrainLayer(gs, terr, index, textureData);
            return;
        }

        TextureType textureType = _gameData.Get<TextureTypeSettings>(gs.ch).Get(textureId);
        if (textureType == null)
        {
            _logService.Message("TextureType is null: TextureId: " + textureId + " Index: " + index);
            textureType = _gameData.Get<TextureTypeSettings>(gs.ch).Get(1);
        }

        string artName = textureType.Art;

        DownloadTerrainTextureData newDownloadData = new DownloadTerrainTextureData();
        newDownloadData.TexType = textureType;
        newDownloadData.Terr = terr;
        newDownloadData.TextureIndex = index;

        _assetService.LoadAssetInto(gs, _terrainManager.GetTerrainTextureParent(), AssetCategoryNames.TerrainTex, artName, OnDownloadArt, newDownloadData, token);
    }


    private void SetNewTerrainLayer(UnityGameState gs, Terrain terr, int index, TerrainTextureData tdata)
    {
        if (terr == null || terr.terrainData == null || terr.terrainData == null || terr.terrainData.terrainLayers == null ||
            tdata == null ||
            tdata.RegTexture == null || tdata.TerrLayer == null)
        {
            return;
        }

        TerrainLayer[] currLayers = terr.terrainData.terrainLayers;
        if (currLayers == null || index < 0 || index >= currLayers.Length)
        {
            return;
        }

        if (currLayers[index] == null)
        {
            return;
        }

        IndexList indexes = GEntityUtils.GetOrAddComponent<IndexList>(gs, terr.entity());

        if (indexes.Indexes == null || indexes.Indexes.Length != currLayers.Length)
        {
            indexes.Indexes = new int[currLayers.Length];
        }

        currLayers[index] = tdata.TerrLayer;
        indexes.Indexes[index] = (int)tdata.TexType.IdKey;

        _terrainManager.SetTerrainLayerData(currLayers[index]);

        tdata.InstanceCount++;

        terr.terrainData.terrainLayers = currLayers;

    }

    private TerrainTextureData GetTerrainTextureCacheData(UnityGameState gs, long textureTypeId)
    {
        if (gs.md == null)
        {
            return null;
        }

        TextureType ttype = _gameData.Get<TextureTypeSettings>(gs.ch).Get(textureTypeId);

        if (ttype == null || string.IsNullOrEmpty(ttype.Name))
        {
            return null;
        }

        return _terrainManager.GetFromTerrainTextureCache(ttype.Name);
    }

    private void OnDownloadArt(UnityGameState gs, object obj, object dataIn, CancellationToken token)
    {
        GEntity go = obj as GEntity;

        if (go == null)
        {
            return;
        }
        DownloadTerrainTextureData ddata = dataIn as DownloadTerrainTextureData;

        if (ddata.Terr == null || ddata.TexType == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }

        TerrainTextureData currentData = GetTerrainTextureCacheData(gs, ddata.TexType.IdKey);

        if (currentData != null && currentData.RegTexture != null)
        {
            GEntityUtils.Destroy(go);
            SetNewTerrainLayer(gs, ddata.Terr, ddata.TextureIndex, currentData);
            return;
        }

        TextureList texList = go.GetComponent<TextureList>();

        int texSize = 2;

        if (texList == null || texList.Textures == null || texList.Textures.Count < texSize)
        {
            GEntityUtils.Destroy(go);
            return;
        }

        for (int i = 0; i < texSize; i++)
        {
            if (texList.Textures[i] == null)
            {
                GEntityUtils.Destroy(go);
                return;
            }
        }

        Texture2D[] newTerrains = texList.Textures.ToArray();

        TerrainTextureData tdata = new TerrainTextureData();
        tdata.RegTexture = newTerrains[0];
        tdata.NormTexture = newTerrains[1];
        tdata.TexType = ddata.TexType;
        tdata.TextureContainer = go;
        tdata.TerrLayer = _terrainManager.CreateTerrainLayer(tdata.RegTexture, tdata.NormTexture);
        _terrainManager.AddToTerrainTextureCache(ddata.TexType.Name, tdata);
        SetNewTerrainLayer(gs, ddata.Terr, ddata.TextureIndex, tdata);
    }

    public async UniTask DownloadAllTerrainTextures(UnityGameState gs, CancellationToken token)
    {
        foreach (TextureType textureType in _gameData.Get<TextureTypeSettings>(gs.ch).GetData())
        {
            DownloadTerrainTextureData newDownloadData = new DownloadTerrainTextureData();
            newDownloadData.TexType = textureType;

            _assetService.LoadAssetInto(gs, _terrainManager.GetTerrainTextureParent(), AssetCategoryNames.TerrainTex, textureType.Name, OnDownloadTextureToCache, newDownloadData, token);
        }

        await UniTask.Delay(1000, cancellationToken: token);

        while (_assetService.IsDownloading(gs))
        {
            await UniTask.NextFrame(cancellationToken: token);
        }

        await UniTask.Delay(100 * _gameData.Get<TextureTypeSettings>(gs.ch).GetData().Count, cancellationToken: token);
    }
    private void OnDownloadTextureToCache(UnityGameState gs, object obj, object dataIn, CancellationToken token)
    {
        GEntity go = obj as GEntity;

        if (go == null)
        {
            return;
        }
        DownloadTerrainTextureData ddata = dataIn as DownloadTerrainTextureData;

        if (ddata.TexType == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }
        TextureList texList = go.GetComponent<TextureList>();

        if (texList == null)
        {
            return;
        }

        TerrainTextureData currentData = GetTerrainTextureCacheData(gs, ddata.TexType.IdKey);

        if (currentData != null)
        {
            GEntityUtils.Destroy(go);
            return;
        }

        int texSize = 2;

        if (texList == null || texList.Textures == null || texList.Textures.Count < texSize)
        {
            GEntityUtils.Destroy(go);
            return;
        }

        for (int i = 0; i < texSize; i++)
        {
            if (texList.Textures[i] == null)
            {
                GEntityUtils.Destroy(go);
                return;
            }
        }

        Texture2D[] newTerrains = texList.Textures.ToArray();

        TerrainTextureData tdata = new TerrainTextureData();
        tdata.RegTexture = newTerrains[0];
        tdata.NormTexture = newTerrains[1];
        tdata.TexType = ddata.TexType;
        tdata.TextureContainer = go;
        tdata.TerrLayer = _terrainManager.CreateTerrainLayer(tdata.RegTexture, tdata.NormTexture);
        _terrainManager.AddToTerrainTextureCache(ddata.TexType.Name, tdata);
    }
}

