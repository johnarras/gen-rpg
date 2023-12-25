
using System;
using GEntity = UnityEngine.GameObject;

using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Interfaces;
using ClientEvents;
using Genrpg.Shared.Units.Entities;
using System.Threading;
using Assets.Scripts.Tokens;
using UnityEngine; // Needed
using Genrpg.Shared.AI.Settings;

public interface IUnitSetupService : IService, IMapTokenService
{
    GEntity SetupUnit(UnityGameState gs, string url, GEntity artGo, SpawnLoadData loadData, CancellationToken token);
}


public class UnitSetupService : IUnitSetupService
{
    protected IClientMapObjectManager _objectManager;
    protected IStatService _statService;
    protected IAssetService _assetService;


    private CancellationToken _token;
    public void SetToken(CancellationToken token)
    {
        _token = token;
    }

    public CancellationToken GetToken()
    {
        return _token;
    }

    public GEntity SetupUnit(UnityGameState gs, string url, GEntity artGo, SpawnLoadData loadData, CancellationToken token)
    {

        if (artGo == null)
        {
            gs.logger.Error("Couldn't download monster art " + url);
            return null;
        }

        if (loadData == null)
        {
            return null;
        }

        ClientMapObjectGridItem gridItem = null;
        
        if (_objectManager.GetGridItem(loadData.Obj.Id, out gridItem))
        {
            if (gridItem.Controller != null && gridItem.GameObj != null)
            {
                GEntityUtils.Destroy(artGo);
                return gridItem.GameObj;
            }
        }

        artGo.name = AnimUtils.RenderObjectName;
        float sizeScale = 1.0f;
        MonsterArtData artData = GEntityUtils.GetComponent<MonsterArtData>(artGo);
        MonsterController mc = GEntityUtils.GetComponent<MonsterController>(artGo);
        if (artData != null && mc != null)
        {
            sizeScale = Math.Max(0.01f, artData.SizeScale);

            mc.WalkAnimSpeed = artData.WalkAnimSpeed;
            mc.RunAnimSpeed = artData.RunAnimSpeed;
        }
        

        GEntity go = new GEntity();
        go.name = loadData.Obj.Name;
        if (artGo.transform().parent != null)
        {
            GEntityUtils.AddToParent(go, artGo.transform().parent.entity());
        }
        GEntityUtils.AddToParent(artGo, go);

        go.transform().localScale = GVector3.onePlatform * sizeScale;

        CharacterController cc = go.GetComponent<CharacterController>();
        if (cc == null)
        {
            cc = go.AddComponent<CharacterController>();
        }
        cc.radius = 0.5f / sizeScale;
        cc.height = 2 / sizeScale;
        cc.center = GVector3.Create(0, (cc.height / 2 * 1.02f), 0);
        cc.slopeLimit = 80;
        cc.stepOffset = cc.height / 3;

        Unit unit = loadData.Obj as Unit;
        unit.TargetId = loadData.Spawn.TargetId;
        InteractUnit interactUnit = GEntityUtils.GetOrAddComponent<InteractUnit>(gs, go);
        //interactUnit.Initialize(gs);
        interactUnit.Init(unit, go, token);
        if (loadData.Spawn.FirstAttacker != null &&
            loadData.Spawn.Loot != null &&
            loadData.Spawn.Loot.Count > 0)
        {
            unit.Loot = loadData.Spawn.Loot;
            unit.SkillLoot = loadData.Spawn.SkillLoot;
            unit.AddAttacker(loadData.Spawn.FirstAttacker.AttackerId, loadData.Spawn.FirstAttacker.GroupId);
        }

        if (unit.IsPlayer())
        {
            OnLoadPlayer(gs, go, unit, token);
        }
        else if (unit.HasFlag(UnitFlags.ProxyCharacter))
        {
            OnLoadProxyCharacter(gs, go, unit, token);
        }
        else
        {
            OnLoadMonster(gs, go, unit, token);
        }

        _statService.CalcStats(gs, unit, true);

        GAnimator animator = GEntityUtils.GetComponent<GAnimator>(go);
        if (animator != null)
        {
            animator.enabled = true;
        }

        Quaternion rot = Quaternion.AngleAxis(loadData.Spawn.Rot, GVector3.Create(0, 1, 0));
        go.transform().rotation = rot;

        GEntityUtils.SetStatic(go, false);

        go.SetActive(true);

        GEntityUtils.SetLayer(go, LayerNames.UnitLayer);

        if (gridItem == null)
        {
            gridItem = _objectManager.AddObject(unit, go);
        }

        gridItem.Controller = go.GetComponent<UnitController>();
        if (unit != null && loadData.Spawn != null)
        {
            unit.FactionTypeId = loadData.Spawn.FactionTypeId;
        }
        _objectManager.AddController(gridItem, go);

        return go;
    }

    protected void OnLoadMonster(UnityGameState gs, GEntity go, Unit unit, CancellationToken token)
    {
        MonsterController mc = GEntityUtils.GetOrAddComponent<MonsterController>(gs, go);

        MonsterArtData artData = GEntityUtils.GetComponent<MonsterArtData>(go);
        if (artData != null)
        {
            mc.TerrainTilt = artData.TerrainTilt;
        }

        mc.Init(unit, token);
        GAnimator animator = GEntityUtils.GetComponent<GAnimator>(go);
        if (animator != null)
        {
            animator.enabled = true;
        }
        // Create health bar after the stats are set up.
        CreateHealthBar(gs, go, unit, token);

    }

    protected void OnLoadProxyCharacter(UnityGameState gs, GEntity go, Unit unit, CancellationToken token)
    {
        ProxyCharacterController pxc = GEntityUtils.GetOrAddComponent<ProxyCharacterController>(gs, go);

        go.transform().localScale = GVector3.onePlatform;

        pxc.Init(unit, token);
        GAnimator animator = GEntityUtils.GetComponent<GAnimator>(go);
        if (animator != null)
        {
            animator.enabled = true;
        }
        // Create health bar after the stats are set up.
        CreateHealthBar(gs, go, unit, token);

    }


    public void OnLoadPlayer(UnityGameState gs, GEntity go, Unit unit, CancellationToken token)
    {
        PlayerController pc = GEntityUtils.GetOrAddComponent<PlayerController>(gs, go);
        pc.Init(unit, token);
        unit.Speed = gs.data.GetGameData<AISettings>(gs.ch).BaseUnitSpeed;
        unit.BaseSpeed = gs.data.GetGameData<AISettings>(gs.ch).BaseUnitSpeed;
        go.name = "Player" + go.name;
        PlayerObject.Set(go);
        _assetService.LoadAssetInto(gs, go, AssetCategoryNames.UI,
            "PlayerLight", null, null, token, "Units");
        gs.Dispatch(new SetMapPlayerEvent() { Ch = unit as Character });
        CreateHealthBar(gs, go, unit, token);
    }


    public void CreateHealthBar(UnityGameState gs, GEntity go, Unit unit, CancellationToken token)
    {
        if (unit == null || go == null)
        {
            return;
        }
        GEntity healthParent = new GEntity() { name = "HealthBarParent" };
        healthParent.transform().SetParent(go.transform());

        float height = 2.5f;
        MonsterArtData artData = GEntityUtils.GetComponent<MonsterArtData>(go);
        if (artData != null && artData.SizeScale > 0)
        {
            height = artData.HealthBarHeight / artData.SizeScale;
        }
        healthParent.transform().localPosition = GVector3.Create(0, height, 0);
        _assetService.LoadAssetInto(gs, healthParent, AssetCategoryNames.UI, 
            "MapHealthBar", OnCreateHealthBar, unit, token, "Units");
    }

    private void OnCreateHealthBar(UnityGameState gs, String url, object obj, object data, CancellationToken token)
    {
        GEntity go = obj as GEntity;
        Unit unit = data as Unit;

        if (go == null)
        {
            return;
        }

        if (unit == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }

        UnitFrame healthBar = go.GetComponent<UnitFrame>();

        if (healthBar != null)
        {
            healthBar.Init(gs, unit);
        }
        
    }
}

