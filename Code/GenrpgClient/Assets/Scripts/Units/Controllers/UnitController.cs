
using System;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;

using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Input.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Spawns.Entities;
using System.Threading;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Combat.Messages;
using UnityEngine; // Needed

public class UnitController : BaseBehaviour
{
    protected IClientMapObjectManager _objectManager;
    protected IMapTerrainManager _terrainManager;
    public const int IdleState = 0;
    public const int CombatState = 1;
    public const int LeashState = 2;
    public int UnitState = IdleState;
    public DateTime LastPosUpdate;

    protected const float BackupSpeedScale = 0.5f;

    protected const float MinWalkAnimationSpeed = 0.03f;
    protected const float MinRunAnimationSpeed = 0.5f;
    public const float SwimDepth = 1.2f;

    protected GAnimator anims = null;
    protected MapGenData UnityMap = null;
	protected CharacterController cc = null;
    Rigidbody rb = null;
    protected Unit _unit = null;
    protected CancellationToken _token;
    private CancellationTokenSource _cts;
    public Unit GetUnit()
    {
        return _unit;
    }

    public void Init(Unit unit, CancellationToken token)
    {
        _unit = unit;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        _token = _cts.Token;
    }


    protected float animationSpeed = 1.0f;
    int currMoveSpeed = -1;



    public virtual float SwimSpeedScale() { return 0.5f; }
    public virtual bool HardStopOnSlopes() { return false; }
    public virtual bool IsSwimming() { return false; }
    public virtual bool CanMoveNow(UnityGameState gs) { return true; }

    public override void Initialize(UnityGameState gs)
    {
        base.Initialize(gs);
        anims = GEntityUtils.GetComponent<GAnimator>(entity);
        cc = GetComponent<CharacterController>();
        rb = GEntityUtils.GetOrAddComponent<Rigidbody>(gs, entity);
        rb.isKinematic = true;
        rb.freezeRotation = true;
        UnityMap = _gs.md;
    }

	protected IDictionary<string, float> _downKeys = new Dictionary<string,float>();
	public virtual void SetKeyPercent (string commandName, float percent)
	{
        if (string.IsNullOrEmpty(commandName))
        {
            return;
        }
        _downKeys[commandName] = percent;
	}

	public virtual float GetKeyPercent (string commandName)
	{
        if (string.IsNullOrEmpty(commandName)) 
        { 
            return 0.0f; 
        }
        if (!_downKeys.ContainsKey (commandName)) 
        { 
            _downKeys[commandName] = 0.0f; 
        }
        return MathUtils.Clamp(0, _downKeys[commandName], 1);
	}
    
    public void SetInputValues(int keysDown, float rot)
    {
        for (int i = 0; i < InputService.MoveInputsToCheck.Length; i++)
        {
            SetKeyPercent(InputService.MoveInputsToCheck[i], FlagUtils.IsSet(keysDown, 1 << i) ? 1 : 0);
        }
        _unit.Rot = rot;
        GVector3 currEuler = GVector3.Create(entity.transform().eulerAngles);
       entity.transform().eulerAngles = GVector3.Create(currEuler.x, rot, currEuler.z);
    }

    protected int GetKeysDown()
    {
        int retval = 0;
        for (int i = 0; i < InputService.MoveInputsToCheck.Length; i++)
        {
            if (GetKeyPercent(InputService.MoveInputsToCheck[i]) > 0)
            {
                retval |= 1 << i;
            }
        }
        return retval;
    }



    protected virtual bool IsOkUnit()
    {
        return _unit is Character ch2 || _unit.HasFlag(UnitFlags.ProxyCharacter);
    }


    public virtual void OnUpdate(CancellationToken token)
    {
        if (_unit == null)
        {
            return;
        }

        if (!(_unit is Character ch2))
        {
            return;
        }

        float delta = InputService.Instance.GetDeltaTime();
        float targetDeltaTime = 1.0f / AppUtils.TargetFrameRate;
        delta = targetDeltaTime;

        float rotSpeed = UnitConstants.RotSpeedPerFrame;
        float moveSpeed = _unit.Speed * delta;
#if UNITY_EDITOR
        if (InitClient.EditorInstance.PlayerSpeedMult > 0)
        {
            moveSpeed *= InitClient.EditorInstance.PlayerSpeedMult;
        }
#endif 
        float animSpeedBase = targetDeltaTime;

        float dz = 0.0f;
        float dx = 0.0f;
        float sKeyPct = GetKeyPercent(KeyComm.Backward)*BackupSpeedScale;
        float wKeyPct = GetKeyPercent(KeyComm.Forward);


        dx += sKeyPct * moveSpeed;
        dx -= wKeyPct * moveSpeed;

        
        if ((dx != 0 || dz != 0) &&
            _unit.ActionMessage != null)
        {
            if (_unit == _gs.ch)
            {
                _networkService.SendMapMessage(new InterruptCast());
                _unit.ActionMessage = null;
            }
        }

        if (IsSwimming()) { dx *= SwimSpeedScale(); }
       
        GVector3 startPos = GVector3.Create(entity.transform().position);
        float rotateAmount = 0.0f;
        rotateAmount -= GetKeyPercent(KeyComm.TurnLeft) * rotSpeed;
        rotateAmount += GetKeyPercent(KeyComm.TurnRight) * rotSpeed;
        if (rotateAmount != 0) {entity.transform().rotation *= Quaternion.Euler(0, rotateAmount, 0); }

        ShowMoveAnimations(dx, dz);

        GVector3 endPos = startPos;
		if (dx != 0 || dz != 0)
		{
            //dz *= (100.0f + _unit.Stats.Pct(StatType.Speed) / 100);
            float nrot = entity.transform().localEulerAngles.y;
            double sin = Math.Sin (nrot*Math.PI/180.0);
            double cos = Math.Cos(nrot*Math.PI/180.0);

            float nz = (float) -(dx*cos+dz*sin);
            float nx = (float) (dz*cos-dx*sin);

            float ny = 0.0f;

            float mx = startPos.x+nx;
            float my = startPos.y+ny;
            float mz = startPos.z+nz;

            endPos = new GVector3(mx, my, mz);

        }

        if (entity.transform().position.y <= 100)
        {
            float endHeight = _gs.md.SampleHeight(_gs, endPos.x, 2000, endPos.z);

            if (endHeight > 0)
            {
                endPos = new GVector3(endPos.x, endHeight + 1, endPos.z);
               entity.transform().position = GVector3.Create(endPos);
                startPos = endPos;
                return;

            }
        }

        GVector3 diff = (endPos - startPos) / InputService.Instance.GetDeltaTime();
        if (cc != null)
        {
            cc.SimpleMove(GVector3.Create(diff));
        }
	}

    float _oldAnimDx = 0;
    float _oldAnimDz = 0;
    protected virtual void ShowMoveAnimations (float dx, float dz)
    {
        if (anims == null)
        {
            return;
        }

        if (dx == _oldAnimDx && dz == _oldAnimDz)
        {
            return;
        }
        _oldAnimDx = dx;
        _oldAnimDz = dz;

        if (dx < -MinRunAnimationSpeed) { SetMoveSpeed(AnimParams.RunSpeed); }
        else if (dx < -MinWalkAnimationSpeed) { SetMoveSpeed(AnimParams.WalkSpeed); }
        else if (dx > MinWalkAnimationSpeed) { SetMoveSpeed(AnimParams.BackSpeed); }
        else { SetMoveSpeed(AnimParams.StopSpeed); }
    }

    protected void SetMoveSpeed(int moveSpeed)
    {
        if (moveSpeed == currMoveSpeed)
        {
            return;
        }

        currMoveSpeed = moveSpeed;
        float speedScale = 0.0f;
        if (_unit.HasFlag(UnitFlags.IsDead))
        {
            return;
        }
        AnimUtils.SetInteger(anims, AnimParams.MoveSpeed, moveSpeed, speedScale);
    }

    public void TriggerAnim(string animName)
    {
        if (_unit.HasFlag(UnitFlags.IsDead))
        {
            return;
        }
        AnimUtils.Trigger(anims, animName, 1);
    }

    public virtual void ProcessDeath(UnityGameState gs, Unit killer) { }
   
    protected UnitFrame _mapHealthBar = null;
    public virtual void SetState(UnityGameState gs, int state)
    {
        UnitState = state;

        UpdateUnitFrame();

    }

    public virtual bool AlwaysShowHealthBar() { return false; }


    public void SetUnitFrame(UnitFrame unitFrame)
    {
        _unitFrame = unitFrame;
    }

    protected UnitFrame _unitFrame = null;
    public void UpdateUnitFrame()
    {
        if (_unitFrame == null)
        {
            _unitFrame = GEntityUtils.GetComponent<UnitFrame>(entity);
        }
        if (_unitFrame == null)
        {
            return;
        }
        _unitFrame.UpdateVisibility();
    }

    private bool _died = false;
    public void OnDeath(Died died, CancellationToken token)
    {
        _unit.AddFlag(UnitFlags.IsDead); 
        if (_died)
        {
            return;
        }

        _died = true;
        _unit.TargetId = null;
        _unit.ToX = _unit.X;
        _unit.ToZ = _unit.Z;
        _unit.Moving = false;

        if (UnitUtils.AttackerInfoMatchesObject(died.FirstAttacker, _gs.ch))
        {
            _unit.Loot = died.Loot;
            _unit.SkillLoot = died.SkillLoot;
            _unit.AddAttacker(_gs.ch.Id, _gs.ch.GetGroupId());
            if ((_unit.Loot != null && _unit.Loot.Count > 0) ||
                (_unit.SkillLoot != null && _unit.SkillLoot.Count > 0))
            {
                InteractUnit interactUnit = entity.GetComponent<InteractUnit>();
                if (interactUnit != null)
                {
                    interactUnit.ShowGlow(0);
                }
            }
        }
        else
        {
            _unit.Loot = new List<SpawnResult>();
            _unit.SkillLoot = new List<SpawnResult>();
        }
        _updateService.AddDelayedUpdate(entity, ShowDeathAnim, token, 0.1f);
    }

    private void ShowDeathAnim(CancellationToken token)
    {
        if (entity == null || anims == null)
        {
            return;
        }
        AnimUtils.Trigger(anims, AnimParams.Die, 1);
    }

    public void StartCasting(OnStartCast msg)
    {
        _unit.ActionMessage = msg;
        _gs.Dispatch(msg);
    }

    public void SetActionAnimation(int actionId)
    {
        if (_unit.HasFlag(UnitFlags.IsDead))
        {
            return;
        }
        anims?.SetInteger(AnimParams.Action, actionId);
    }

    public void ShowCombatText(CombatText text)
    {
        _assetService.LoadAssetInto(_gs, entity, AssetCategoryNames.UI, CombatTextUI.UIPrefabName, OnLoadCombatText, text, _token);
    }

    private void OnLoadCombatText(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GEntity go = obj as GEntity;
        if (go == null)
        {
            return;
        }

        CombatText text = data as CombatText;

        if (text == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }

        CombatTextUI ui = go.GetComponent<CombatTextUI>();
        if (ui == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }

        ui.Init(gs, text);
    }
}

