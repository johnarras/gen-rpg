using UnityEngine;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.MapObjects.Entities;

using Genrpg.Shared.Constants;
using System.Threading;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.Combat.Messages;
using Assets.Scripts.MapTerrain;
using Genrpg.Shared.Units.Constants;
using System.Threading.Tasks;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Client.Assets.Constants;

public class UnitObjectLoader : BaseMapObjectLoader
{
    public override long GetKey() { return EntityTypes.Unit; }
    protected override string GetLayerName() { return LayerNames.UnitLayer; }

    public override async Awaitable Load(OnSpawn spawn, MapObject obj, CancellationToken token)
    {

        UnitType utype = _gameData.Get<UnitSettings>(_gs.ch).Get(spawn.EntityId);
        if (utype == null)
        {
            return;
        }

        SpawnLoadData loadData = new SpawnLoadData()
        {
            Spawn = spawn,
            Obj = obj,
            Token = token,
        };
        

        _assetService.LoadAsset(AssetCategoryNames.Monsters, utype.Art, AfterLoadUnit, loadData, null, token);
        await Task.CompletedTask;
    }



    private IUnitSetupService _zoneGenService = null;
    protected virtual void AfterLoadUnit(object obj, object data, CancellationToken token)
    {
        SpawnLoadData loadData = data as SpawnLoadData;
        GameObject artGo = obj as GameObject;

        if (artGo == null)
        {
            return;
        }

        if (!TokenUtils.IsValid(loadData.Token))
        {
            _gameObjectService.Destroy(artGo);
            return;
        }

        if (_objectManager.GetController(loadData.Spawn.ObjId, out UnitController currController))
        {
            _gameObjectService.Destroy(artGo);
            return;
        }

        GameObject go = _zoneGenService.SetupUnit(artGo, loadData, loadData.Token);
        float height = _terrainManager.SampleHeight(loadData.Obj.X, loadData.Obj.Z);

        TerrainPatchData patch = _terrainManager.GetPatchFromMapPos(loadData.Obj.X, loadData.Obj.Z);

        Vector3 oldScale = go.transform.localScale;
        if (patch != null)
        {
            Terrain terr = patch.terrain as Terrain;

            if (terr != null)
            {
                _gameObjectService.AddToParent(go, terr.gameObject);
            }
            else
            {
                _gameObjectService.AddToParent(go, _terrainManager.GetPrototypeParent());
            }
        }

        go.transform.position = new Vector3(loadData.Obj.X, height, loadData.Obj.Z);
        go.transform.eulerAngles = new Vector3(0, loadData.Obj.Rot, 0);
        go.transform.localScale = oldScale;
        if (loadData.Obj is Unit unit)
        {
            if (unit.HasFlag(UnitFlags.IsDead))
            {
                UnitController unitController = go.GetComponent<UnitController>();
                if (unitController != null)
                {
                    unitController.OnDeath(new Died(), loadData.Token);
                }
            }
        }

        if (height == 0)
        {
            TaskUtils.ForgetAwaitable(WaitForTerrain(go, loadData, loadData.Token));
        }

        _objectManager.AddObject(loadData.Obj, go);
        
    }

    private async Awaitable WaitForTerrain(GameObject go, SpawnLoadData loadData, CancellationToken token)
    {
        int times = 0;
        while (!token.IsCancellationRequested && ++times < 1000)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
            float height = _terrainManager.SampleHeight(loadData.Obj.X, loadData.Obj.Z);
            if (height > 0)
            {
                go.transform.position = new Vector3(loadData.Obj.X, height, loadData.Obj.Z);
                break;
            }
        }
    }
}
