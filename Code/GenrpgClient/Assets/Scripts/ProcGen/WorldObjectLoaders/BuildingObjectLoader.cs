using Cysharp.Threading.Tasks;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Constants;
using System.Threading;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.Buildings.Settings;
using Assets.Scripts.Buildings;

public class BuildingObjectLoader : BaseMapObjectLoader
{
    public override long GetKey() { return EntityTypes.Building; }
    protected override string GetLayerName() { return LayerNames.ObjectLayer; }

    public override async UniTask Load(UnityGameState gs, OnSpawn spawn, MapObject obj, CancellationToken token)
    {
        BuildingType buildingType = gs.data.Get<BuildingSettings>(gs.ch).Get(spawn.EntityId);
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

        _assetService.LoadAsset(gs, AssetCategoryNames.Buildings , "Default/" + buildingType.Art, OnDownloadBuildingObject, loadData, null, token);

        await UniTask.CompletedTask;
        return;
    }

    private void OnDownloadBuildingObject(UnityGameState gs, object obj, object data, CancellationToken token)
    {
        GEntity go = obj as GEntity;
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
        MapBuilding building = GEntityUtils.GetOrAddComponent<MapBuilding>(gs,go);

        BuildingType buildingType = gs.data.Get<BuildingSettings>(gs.ch).Get(loadData.Spawn.EntityId);

        building.Init(buildingType, loadData.Spawn);
 
        FinalPlaceObject(gs, go, loadData, LayerNames.ObjectLayer);
    }
}



