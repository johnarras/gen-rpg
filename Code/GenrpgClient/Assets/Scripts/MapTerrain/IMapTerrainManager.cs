using Assets.Scripts.MapTerrain;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using GEntity = UnityEngine.GameObject;

public interface IMapTerrainManager : ISetupService
{
    GEntity GetTerrainTextureParent();
    GEntity GetTerrainProtoObject(string name);
    void AddTerrainProtoPatch(string name, int gx, int gy);
    void RemovePatchFromPrototypes(int gx, int gy);
    TerrainTextureData GetFromTerrainTextureCache(string textureName);
    void Clear(UnityGameState gs);
    UniTask SetupOneTerrainPatch(UnityGameState gs, int gx, int gy, CancellationToken token);
    bool AddingPatches(UnityGameState gs);
    List<Terrain> GetTerrains(UnityGameState gs);
    TerrainPatchData GetPatchFromMapPos(UnityGameState gs, float worldx, float worldy);
    TerrainPatchData GetMapGrid(UnityGameState gs, int gx, int gy);
    UniTask AddPatchObjects(UnityGameState gs, int gx, int gy, CancellationToken token);
    void AddToTerrainTextureCache(string textureName, TerrainTextureData data);
    void ClearPatches(UnityGameState gs);
    GEntity GetPrototypeParent();
    GEntity AddOrReuseTerrainProtoObject(string name, GEntity go);
    void ClearMapObjects(UnityGameState gs);
    void SetFastLoading();
    long GetPatchesRemoved();
    long GetPatchesAdded();
    void IncrementPatchesAdded();
    bool IsLoadingPatches();
    void RemoveLoadingPatches(int gx, int gy);
    GVector3 GetInterpolatedNormal(UnityGameState gs, Map map, float x, float y);
    float SampleHeight(UnityGameState gs, float x, float z);
    float GetInterpolatedHeight(UnityGameState gs, float xpos, float ypos);
    TerrainPatchData GetTerrainPatch(UnityGameState gs, int gx, int gy, bool createIfNotExist = false);
    void SetTerrainPatchAtGridLocation(UnityGameState gs, int xgrid, int ygrid, Map map, TerrainPatchData data);
    void SetOneTerrainNeighbors(UnityGameState gs, int gx, int gy);
    float GetSteepness(UnityGameState gs, float xpos, float ypos);
    TerrainData GetTerrainData(UnityGameState gs, int gx, int gy);
    TerrainLayer CreateTerrainLayer(Texture2D diffuse, Texture2D normal = null);
    Texture2D GetBasicTerrainTexture(UnityGameState gs, int index);
    void SetTerrainLayerData(TerrainLayer tl);
    void SetAllTerrainNeighbors(UnityGameState gs);
}