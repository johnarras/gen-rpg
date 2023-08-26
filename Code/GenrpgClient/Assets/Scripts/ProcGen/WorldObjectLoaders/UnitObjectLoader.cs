
using UnityEngine;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.DataStores.Entities;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Constants;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.Combat.Messages;
using Assets.Scripts.MapTerrain;

public class UnitObjectLoader : BaseMapObjectLoader
{
    public UnitObjectLoader(UnityGameState gs) : base(gs)
    {

    }
    public override long GetKey() { return EntityType.Unit; }
    protected override string GetLayerName() { return LayerNames.UnitLayer; }

    public override async UniTask Load(UnityGameState gs, OnSpawn spawn, MapObject obj, CancellationToken token)
    {

        UnitType utype = gs.data.GetGameData<UnitSettings>().GetUnitType(spawn.EntityId);
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

        _assetService.LoadAsset(gs, AssetCategory.Monsters, utype.Art, AfterLoadUnit, loadData, null, token);
    }



    private IUnitSetupService _zoneGenService = null;
    protected virtual void AfterLoadUnit(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        SpawnLoadData loadData = data as SpawnLoadData;
        GameObject artGo = obj as GameObject;

        if (artGo == null)
        {
            return;
        }

        if (!TokenUtils.IsValid(loadData.Token))
        {
            GameObject.Destroy(artGo);
            return;
        }

        if (_objectManager.GetController(loadData.Spawn.MapObjectId, out UnitController currController))
        {
            GameObject.Destroy(artGo);
            return;
        }

        GameObject go = _zoneGenService.SetupUnit(gs, url, artGo, loadData, loadData.Token);
        float height = gs.md.SampleHeight(gs, loadData.Obj.X, MapConstants.MapHeight * 2, loadData.Obj.Z);

        TerrainPatchData patch = _mapTerrainManager.GetPatchFromMapPos(gs, loadData.Obj.X, loadData.Obj.Z);

        Vector3 oldScale = go.transform.localScale;
        if (patch != null)
        {
            Terrain terr = patch.terrain as Terrain;

            if (terr != null)
            {
                GameObjectUtils.AddToParent(go, terr.gameObject);
            }
            else
            {
                GameObjectUtils.AddToParent(go, _mapTerrainManager.GetPrototypeParent());
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
            WaitForTerrain(gs, go, loadData, loadData.Token).Forget();
        }

        _objectManager.AddObject(loadData.Obj, go);
    }

    private async UniTask WaitForTerrain(UnityGameState gs, GameObject go, SpawnLoadData loadData, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.NextFrame(token);
            float height = gs.md.SampleHeight(gs, loadData.Obj.X, MapConstants.MapHeight * 2, loadData.Obj.Z);
            if (height > 0)
            {
                go.transform.position = new Vector3(loadData.Obj.X, height, loadData.Obj.Z);
                break;
            }
        }
    }
}
