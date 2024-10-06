using Assets.Scripts.MapTerrain;

using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public interface IMapTerrainManager : IInitializable
{
    GameObject GetTerrainTextureParent();
    GameObject GetTerrainProtoObject(string name);
    void AddTerrainProtoPatch(string name, int gx, int gy);
    void RemovePatchFromPrototypes(int gx, int gy);
    TerrainTextureData GetFromTerrainTextureCache(string textureName);
    void Clear();
    Awaitable SetupOneTerrainPatch(int gx, int gy, CancellationToken token);
    bool AddingPatches();
    List<Terrain> GetTerrains();
    TerrainPatchData GetPatchFromMapPos(float worldx, float worldy);
    TerrainPatchData GetMapGrid(int gx, int gy);
    Awaitable AddPatchObjects(int gx, int gy, CancellationToken token);
    void AddToTerrainTextureCache(string textureName, TerrainTextureData data);
    void ClearPatches();
    GameObject GetPrototypeParent();
    GameObject AddOrReuseTerrainProtoObject(string name, GameObject go);
    void ClearMapObjects();
    void SetFastLoading();
    long GetPatchesRemoved();
    long GetPatchesAdded();
    void IncrementPatchesAdded();
    bool IsLoadingPatches();
    void RemoveLoadingPatches(int gx, int gy);
    Vector3 GetInterpolatedNormal(Map map, float x, float y);
    float SampleHeight(float x, float z);
    float GetInterpolatedHeight(float xpos, float ypos);
    TerrainPatchData GetTerrainPatch(int gx, int gy, bool createIfNotExist = false);
    void SetTerrainPatchAtGridLocation(int xgrid, int ygrid, Map map, TerrainPatchData data);
    void SetOneTerrainNeighbors(int gx, int gy);
    float GetSteepness(float xpos, float ypos);
    TerrainData GetTerrainData(int gx, int gy);
    TerrainLayer CreateTerrainLayer(Texture2D diffuse, Texture2D normal = null);
    Texture2D GetBasicTerrainTexture(int index);
    void SetTerrainLayerData(TerrainLayer tl);
    void SetAllTerrainNeighbors();
    BaseObjectLoader GetLoader(long mapObjectOffset);
    Awaitable SetupOneTerrainPatch(TerrainPatchData patch, CancellationToken token);
}