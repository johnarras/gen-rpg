
using System;
using UnityEngine;

using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.Entities;
using ClientEvents;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.NPCs.Entities;
using System.Threading;
using Assets.Scripts.Tokens;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.AI.Entities;

public interface IUnitSetupService : IService, IMapTokenService
{
    GameObject SetupUnit(UnityGameState gs, string url, GameObject artGo, SpawnLoadData loadData, CancellationToken token);
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

    public GameObject SetupUnit(UnityGameState gs, string url, GameObject artGo, SpawnLoadData loadData, CancellationToken token)
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
                GameObject.Destroy(artGo);
                return gridItem.GameObj;
            }
        }

        artGo.name = AnimUtils.RenderObjectName;
        float sizeScale = 1.0f;
        MonsterArtData artData = GameObjectUtils.GetComponent<MonsterArtData>(artGo);
        MonsterController mc = GameObjectUtils.GetComponent<MonsterController>(artGo);
        if (artData != null && mc != null)
        {
            sizeScale = Mathf.Max(0.01f, artData.SizeScale);

            mc.WalkAnimSpeed = artData.WalkAnimSpeed;
            mc.RunAnimSpeed = artData.RunAnimSpeed;
        }
        

        GameObject go = new GameObject();
        go.name = loadData.Obj.Name;
        if (artGo.transform.parent != null)
        {
            GameObjectUtils.AddToParent(go, artGo.transform.parent.gameObject);
        }
        GameObjectUtils.AddToParent(artGo, go);

        go.transform.localScale = Vector3.one * sizeScale;

        CharacterController cc = go.GetComponent<CharacterController>();
        if (cc == null)
        {
            cc = go.AddComponent<CharacterController>();
        }
        cc.radius = 0.5f / sizeScale;
        cc.height = 2 / sizeScale;
        cc.center = new Vector3(0, (cc.height / 2 * 1.02f), 0);
        cc.slopeLimit = 80;
        cc.stepOffset = cc.height / 3;

        Unit unit = loadData.Obj as Unit;
        unit.TargetId = loadData.Spawn.TargetId;
        InteractUnit interactUnit = GameObjectUtils.GetOrAddComponent<InteractUnit>(gs, go);
        interactUnit.Init(unit, go, token);
        if (!string.IsNullOrEmpty(loadData.Spawn.FirstAttackerId) &&
            loadData.Spawn.Loot != null &&
            loadData.Spawn.Loot.Count > 0)
        {
            unit.Loot = loadData.Spawn.Loot;
            unit.SkillLoot = loadData.Spawn.SkillLoot;
            unit.AddAttacker(loadData.Spawn.FirstAttackerId);
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

        Animator animator = GameObjectUtils.GetComponent<Animator>(go);
        if (animator != null)
        {
            animator.enabled = true;
        }

        Quaternion rot = Quaternion.AngleAxis(loadData.Spawn.Rot, new Vector3(0, 1, 0));
        go.transform.rotation = rot;

        GameObjectUtils.SetStatic(go, false);

        go.SetActive(true);

        GameObjectUtils.SetLayer(go, LayerNames.UnitLayer);

        if (gridItem == null)
        {
            gridItem = _objectManager.AddObject(unit, go);
        }

        gridItem.Controller = go.GetComponent<UnitController>();
        if (unit != null && loadData.Spawn != null)
        {
            unit.NPCTypeId = loadData.Spawn.NPCTypeId;
            unit.NPCType = gs.map.Get<NPCType>(loadData.Spawn.NPCTypeId);
            unit.FactionTypeId = loadData.Spawn.FactionTypeId;
        }
        _objectManager.AddController(gridItem, go);

        return go;
    }

    protected void OnLoadMonster(UnityGameState gs, GameObject go, Unit unit, CancellationToken token)
    {
        MonsterController mc = GameObjectUtils.GetOrAddComponent<MonsterController>(gs, go);

        MonsterArtData artData = GameObjectUtils.GetComponent<MonsterArtData>(go);
        if (artData != null)
        {
            mc.TerrainTilt = artData.TerrainTilt;
        }

        go.transform.localScale *= 2.0f;

        mc.Init(unit, token);
        Animator animator = GameObjectUtils.GetComponent<Animator>(go);
        if (animator != null)
        {
            animator.enabled = true;
        }
        // Create health bar after the stats are set up.
        CreateHealthBar(gs, go, unit, token);

    }

    protected void OnLoadProxyCharacter(UnityGameState gs, GameObject go, Unit unit, CancellationToken token)
    {
        ProxyCharacterController pxc = GameObjectUtils.GetOrAddComponent<ProxyCharacterController>(gs, go);


        go.transform.localScale = Vector3.one;

        pxc.Init(unit, token);
        Animator animator = GameObjectUtils.GetComponent<Animator>(go);
        if (animator != null)
        {
            animator.enabled = true;
        }
        // Create health bar after the stats are set up.
        CreateHealthBar(gs, go, unit, token);

    }


    public void OnLoadPlayer(UnityGameState gs, GameObject go, Unit unit, CancellationToken token)
    {
        PlayerController pc = GameObjectUtils.GetOrAddComponent<PlayerController>(gs, go);
        pc.Init(unit, token);
        unit.Speed = gs.data.GetGameData<AISettings>().BaseUnitSpeed;
        unit.BaseSpeed = gs.data.GetGameData<AISettings>().BaseUnitSpeed;
        go.name = "Player" + go.name;
        PlayerObject.Set(go);
        _assetService.LoadAssetInto(gs, go, AssetCategory.UI, "PlayerLight", null, null, token);
        gs.Dispatch(new SetMapPlayerEvent() { Ch = unit as Character });
        CreateHealthBar(gs, go, unit, token);
    }


    public void CreateHealthBar(UnityGameState gs, GameObject go, Unit unit, CancellationToken token)
    {
        if (unit == null || go == null)
        {
            return;
        }
        GameObject healthParent = new GameObject() { name = "HealthBarParent" };
        healthParent.transform.SetParent(go.transform);

        float height = 2.5f;
        MonsterArtData artData = GameObjectUtils.GetComponent<MonsterArtData>(go);
        if (artData != null && artData.SizeScale > 0)
        {
            height = artData.HealthBarHeight / artData.SizeScale;
        }
        healthParent.transform.localPosition = new Vector3(0, height, 0);
        _assetService.LoadAssetInto(gs, healthParent, AssetCategory.UI, "MapHealthBar", OnCreateHealthBar, unit, token);
    }

    private void OnCreateHealthBar(UnityGameState gs, String url, object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        Unit unit = data as Unit;

        if (go == null)
        {
            return;
        }

        if (unit == null)
        {
            GameObject.Destroy(go);
            return;
        }

        UnitFrame healthBar = go.GetComponent<UnitFrame>();

        if (healthBar != null)
        {
            healthBar.Init(gs, unit);
        }
        
    }
}

