

using Genrpg.Shared.Units.Entities;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Characters.PlayerData;
using System.Threading;
using Genrpg.Shared.MapObjects.Messages;
using UnityEngine;

class AddPlayerToMap : BaseZoneGenerator
{

    protected IUnitSetupService _unitSetupService;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);

        UnitType utype = _gameData.Get<UnitSettings>(_gs.ch).Get(_gs.ch.EntityId);

        if (utype == null || string.IsNullOrEmpty(utype.Art))
        {
            return;
        }

        _assetService.LoadAssetInto(null, AssetCategoryNames.Monsters, utype.Art, OnLoadPlayer, _gs.ch, token);

    }

    private void OnLoadPlayer(object obj, object data, CancellationToken token)
    {
        GEntity artGo = obj as GEntity;

        Character ch = data as Character;

        if (artGo == null || ch == null)
        { 
            return;
        }
        SpawnLoadData loadData = new SpawnLoadData()
        {
            Obj = ch,
            Spawn = new OnSpawn(),
            Token = _token,
        };
        
        GEntity go = _unitSetupService.SetupUnit(artGo, loadData, _token);
        float height = _terrainManager.SampleHeight(ch.X, ch.Z);
        go.transform().position = GVector3.Create(ch.X, MapConstants.MapHeight, ch.Z);
        go.transform().eulerAngles = GVector3.Create(0, ch.Rot, 0);

    }
}
