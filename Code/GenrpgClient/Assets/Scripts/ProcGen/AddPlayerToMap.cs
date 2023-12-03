
using System.Threading.Tasks;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.DataStores.Entities;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Currencies.Entities;
using System.Threading;
using Genrpg.Shared.MapObjects.Messages;
using System.Diagnostics;

class AddPlayerToMap : BaseZoneGenerator
{

    protected IUnitSetupService _unitSetupService;
    public override async Task Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);


        gs.ch.X = gs.map.SpawnX;
        gs.ch.Z = gs.map.SpawnY;

        UnitType utype = gs.data.GetGameData<UnitSettings>(gs.ch).GetUnitType(gs.ch.EntityId);

        if (utype == null || string.IsNullOrEmpty(utype.Art))
        {
            return;
        }

        _assetService.LoadAssetInto(gs, null, AssetCategoryNames.Monsters, utype.Art, OnLoadPlayer, gs.ch, token);

    }

    private void OnLoadPlayer(UnityGameState gs, string url, object obj, object data, CancellationToken token)
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
        
        GEntity go = _unitSetupService.SetupUnit(gs, url, artGo, loadData, _token);
        float height = gs.md.SampleHeight(gs, ch.X, MapConstants.MapHeight * 2, ch.Z);
        go.transform().position = GVector3.Create(ch.X, MapConstants.MapHeight, ch.Z);
        go.transform().eulerAngles = GVector3.Create(0, ch.Rot, 0);

    }
}
