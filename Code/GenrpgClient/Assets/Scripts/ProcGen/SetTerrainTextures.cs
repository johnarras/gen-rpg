
using System;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using System.Threading.Tasks;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;
using Assets.Scripts.MapTerrain;
using UnityEngine; // Needed

public class TerrainTextureData
{
    public TextureType TexType;

    public Texture2D RegTexture;
    public Texture2D NormTexture;
    public GEntity TextureContainer;
    public int InstanceCount = 0;
    public TerrainLayer TerrLayer;
}

public class SetTerrainTextures : BaseZoneGenerator
{

    public class DownloadTerrainTextureData
    {
        public TextureType TexType;
        public int TextureIndex;
        public Terrain Terr;
    }

    public override async Task Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);

        await DownloadAllTerrainTextures(gs, token);

        for (int gx = 0; gx < gs.map.BlockCount; gx++)
        {
            for (int gy = 0; gy < gs.map.BlockCount; gy++)
            {
                TaskUtils.AddTask(SetOneTerrainPatchLayers(gs, gs.md.terrainPatches[gx, gy], token, true), "setoneterrainpatchlayers", token);
            }
            await Task.Delay(1);
        }
        await Task.Delay(TimeSpan.FromSeconds(2.0f));
    }


    public async Task SetOneTerrainPatchLayers(UnityGameState gs, TerrainPatchData patch, CancellationToken token, bool allAtOnce = false)       
    {
        if (_terrainManager == null)
        {
            gs.loc.Resolve(this);
        }  

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

    private async Task DelayLoadSplats(UnityGameState gs, Terrain terr, TerrainPatchData patch, bool allAtOnce, CancellationToken token)
    {
        for (int l = 0; l < patch.TerrainTextureIndexes.Count; l++)
        {
            if (!allAtOnce)
            {
                await Task.Delay(1, token);
            }

            SetupTerrainTexture(gs, terr, patch.TerrainTextureIndexes[l], l, token);

        }
    }

    public static string GetTerrainTextureCacheKey (UnityGameState gs, int worldId, int zoneId, int zoneTypeId, int baseSplatChannel)
    {
        return "W" + worldId + "Z" + zoneTypeId + "S" + baseSplatChannel;
    }
	
    /// <summary>
    /// 
    /// </summary>
    /// <param name="gs"></param>
    /// <param name="terr"></param>
    /// <param name="textureId"></param>
    /// <param name="index">current index taking into account the zone offset of 4 per zone</param>
	private void SetupTerrainTexture (UnityGameState gs, Terrain terr, long textureId, int index, CancellationToken token)
	{
        TerrainTextureData textureData = GetTerrainTextureCacheData(gs, textureId);
        if (textureData != null)
        {
            SetNewTerrainLayer(gs, terr, index, textureData);
            return;
        }

        TextureType textureType = gs.data.GetGameData<TextureTypeSettings>(gs.ch).GetTextureType (textureId);
		if (textureType == null)
		{
            gs.logger.Message("TextureType is null: TextureId: " + textureId + " Index: " + index);
			textureType = gs.data.GetGameData<TextureTypeSettings>(gs.ch).GetTextureType (1);
		}

        string artName = textureType.Name;

        DownloadTerrainTextureData newDownloadData = new DownloadTerrainTextureData();
        newDownloadData.TexType = textureType;
        newDownloadData.Terr = terr;
        newDownloadData.TextureIndex = index;

		_assetService.LoadAssetInto(gs, _terrainManager.GetTerrainTextureParent(), AssetCategory.TerrainTex, artName, OnDownloadArt,newDownloadData, token);
	}


    private void SetNewTerrainLayer (UnityGameState gs, Terrain terr, int index, TerrainTextureData tdata)
    {
        if (terr == null || terr.terrainData == null || terr.terrainData == null || terr.terrainData.terrainLayers == null  ||
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

        MapGenData.SetTerrainLayerData(currLayers[index]);

        tdata.InstanceCount++;

        terr.terrainData.terrainLayers = currLayers;

    }

    private TerrainTextureData GetTerrainTextureCacheData(UnityGameState gs, long textureTypeId)
    {
        if (gs.md == null)
        {
            return null;
        }

        TextureType ttype = gs.data.GetGameData<TextureTypeSettings>(gs.ch).GetTextureType(textureTypeId);

        if (ttype == null || string.IsNullOrEmpty(ttype.Name))
        {
            return null;
        }

        return _terrainManager.GetFromTerrainTextureCache(ttype.Name);
    }

    private void OnDownloadArt(UnityGameState gs, string url, object obj, object dataIn, CancellationToken token)
    {
        if (url == null)
        {
            return;
        }

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
        tdata.TerrLayer = MapGenData.CreateTerrainLayer(tdata.RegTexture, tdata.NormTexture);
        _terrainManager.AddToTerrainTextureCache(ddata.TexType.Name, tdata);
        SetNewTerrainLayer(gs, ddata.Terr, ddata.TextureIndex, tdata);
    }

    protected async Task DownloadAllTerrainTextures(UnityGameState gs, CancellationToken token)
    {
        foreach (TextureType textureType in gs.data.GetGameData<TextureTypeSettings>(gs.ch).GetData())
        {
            DownloadTerrainTextureData newDownloadData = new DownloadTerrainTextureData();
            newDownloadData.TexType = textureType;

            _assetService.LoadAssetInto(gs, _terrainManager.GetTerrainTextureParent(), AssetCategory.TerrainTex, textureType.Name, OnDownloadTextureToCache, newDownloadData, token);
        }

        await Task.Delay(1000, cancellationToken: token);

        while (_assetService.IsDownloading(gs))
        {
            await Task.Delay(1, token);
        }

        await Task.Delay(100 * gs.data.GetGameData<TextureTypeSettings>(gs.ch).GetData().Count, cancellationToken: token);
    }
    private void OnDownloadTextureToCache(UnityGameState gs, string url, object obj, object dataIn, CancellationToken token)
    {
        if (url == null)
        {
            return;
        }

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
        tdata.TerrLayer = MapGenData.CreateTerrainLayer(tdata.RegTexture, tdata.NormTexture);
        _terrainManager.AddToTerrainTextureCache(ddata.TexType.Name, tdata);
    }
}
	
