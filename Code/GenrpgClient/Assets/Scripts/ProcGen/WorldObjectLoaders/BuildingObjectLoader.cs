
using UnityEngine;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Constants;
using System.Threading;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.Buildings.Settings;
using Assets.Scripts.Buildings;
using System.Threading.Tasks;
using Genrpg.Shared.Client.Assets.Constants;

public class BuildingObjectLoader : BaseMapObjectLoader
{
    public override long GetKey() { return EntityTypes.Building; }
    protected override string GetLayerName() { return LayerNames.ObjectLayer; }

    public override async Awaitable Load(OnSpawn spawn, MapObject obj, CancellationToken token)
    {
        BuildingType buildingType = _gameData.Get<BuildingSettings>(_gs.ch).Get(spawn.EntityId);
        if (buildingType == null)
        {
            return;
        }
        float wx = spawn.X;
        float wz = spawn.Z;

        SpawnLoadData loadData = new SpawnLoadData()
        {
            Obj = obj,
            Spawn = spawn,
            Token = token,
        };

        _assetService.LoadAsset(AssetCategoryNames.Buildings , "Default/" + buildingType.Art, OnDownloadBuildingObject, loadData, null, token);


        await Task.CompletedTask;
        return;
    }

    private void OnDownloadBuildingObject(object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            return;
        }

        SpawnLoadData loadData = data as SpawnLoadData;
        if (loadData ==null)
        {
            return;
        }
        loadData.FixedPosition = true;
        MapBuilding building = _clientEntityService.GetOrAddComponent<MapBuilding>(go);

        BuildingType buildingType = _gameData.Get<BuildingSettings>(_gs.ch).Get(loadData.Spawn.EntityId);

        building.Init(buildingType, loadData.Spawn);
 
        FinalPlaceObject(go, loadData, LayerNames.ObjectLayer);
    }
}



