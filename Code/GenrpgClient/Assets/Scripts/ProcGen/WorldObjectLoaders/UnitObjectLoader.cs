using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.DataStores.Entities;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Constants;
using System.Threading;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.Combat.Messages;
using Assets.Scripts.MapTerrain;
using UnityEngine;
using Genrpg.Shared.Units.Constants;

public class UnitObjectLoader : BaseMapObjectLoader
{
    public override long GetKey() { return EntityTypes.Unit; }
    protected override string GetLayerName() { return LayerNames.UnitLayer; }

    public override async UniTask Load(OnSpawn spawn, MapObject obj, CancellationToken token)
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
        await UniTask.CompletedTask;

        _assetService.LoadAsset(AssetCategoryNames.Monsters, utype.Art, AfterLoadUnit, loadData, null, token);
    }



    private IUnitSetupService _zoneGenService = null;
    protected virtual void AfterLoadUnit(object obj, object data, CancellationToken token)
    {
        SpawnLoadData loadData = data as SpawnLoadData;
        GEntity artGo = obj as GEntity;

        if (artGo == null)
        {
            return;
        }

        if (!TokenUtils.IsValid(loadData.Token))
        {
            GEntityUtils.Destroy(artGo);
            return;
        }

        if (_objectManager.GetController(loadData.Spawn.ObjId, out UnitController currController))
        {
            GEntityUtils.Destroy(artGo);
            return;
        }

        GEntity go = _zoneGenService.SetupUnit(artGo, loadData, loadData.Token);
        float height = _terrainManager.SampleHeight(loadData.Obj.X, loadData.Obj.Z);

        TerrainPatchData patch = _terrainManager.GetPatchFromMapPos(loadData.Obj.X, loadData.Obj.Z);

        GVector3 oldScale = GVector3.Create(go.transform().localScale);
        if (patch != null)
        {
            Terrain terr = patch.terrain as Terrain;

            if (terr != null)
            {
                GEntityUtils.AddToParent(go, terr.entity());
            }
            else
            {
                GEntityUtils.AddToParent(go, _terrainManager.GetPrototypeParent());
            }
        }

        go.transform().position = GVector3.Create(loadData.Obj.X, height, loadData.Obj.Z);
        go.transform().eulerAngles = GVector3.Create(0, loadData.Obj.Rot, 0);
        go.transform().localScale = GVector3.Create(oldScale);
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
            WaitForTerrain(go, loadData, loadData.Token).Forget();
        }

        _objectManager.AddObject(loadData.Obj, go);
        
    }

    private async UniTask WaitForTerrain(GEntity go, SpawnLoadData loadData, CancellationToken token)
    {
        int times = 0;
        while (!token.IsCancellationRequested && ++times < 1000)
        {
            await UniTask.NextFrame(cancellationToken: token);
            float height = _terrainManager.SampleHeight(loadData.Obj.X, loadData.Obj.Z);
            if (height > 0)
            {
                go.transform().position = GVector3.Create(loadData.Obj.X, height, loadData.Obj.Z);
                break;
            }
        }
    }
}
