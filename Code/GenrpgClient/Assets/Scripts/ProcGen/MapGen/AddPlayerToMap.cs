
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Units.Entities;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Characters.PlayerData;
using System.Threading;
using Genrpg.Shared.MapObjects.Messages;

class AddPlayerToMap : BaseZoneGenerator
{

    protected IUnitSetupService _unitSetupService;
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);

        UnitType utype = _gameData.Get<UnitSettings>(gs.ch).Get(gs.ch.EntityId);

        if (utype == null || string.IsNullOrEmpty(utype.Art))
        {
            return;
        }

        _assetService.LoadAssetInto(gs, null, AssetCategoryNames.Monsters, utype.Art, OnLoadPlayer, gs.ch, token);

    }

    private void OnLoadPlayer(UnityGameState gs, object obj, object data, CancellationToken token)
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
        
        GEntity go = _unitSetupService.SetupUnit(gs, artGo, loadData, _token);
        float height = _terrainManager.SampleHeight(gs, ch.X, ch.Z);
        go.transform().position = GVector3.Create(ch.X, MapConstants.MapHeight, ch.Z);
        go.transform().eulerAngles = GVector3.Create(0, ch.Rot, 0);

    }
}
