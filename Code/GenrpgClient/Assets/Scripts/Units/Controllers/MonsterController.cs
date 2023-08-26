using System;
using System.Collections;
using UnityEngine;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Units.Entities;
using System.Threading;
using System.Collections.Generic;
using Genrpg.Shared.AI.Entities;

public class MonsterController : UnitController
{

    public float WalkAnimSpeed = 0;
    public float RunAnimSpeed = 0;

    protected override void OnDestroy()
    {
        _objectManager.RemoveObject(_unit.Id);
    }


    public Unit GetNearbyTarget(UnityGameState gs)
    {
        return null;
    }


    public void EnterCombat()
    {
        if (UnitState == LeashState)
        {
            return;
        }
        SetState(_gs, CombatState);
    }


    public override bool CanMoveNow(UnityGameState gs)
    {
        return true;
    }



    bool canMoveNow = false;

    bool _didDeadUpdate = false;
    public override void OnUpdate(CancellationToken token)
    {
        if (_unit == null)
        {
            return;
        }
        name = _unit.Id;
        if (_unit.HasFlag(UnitFlags.IsDead))
        {
            if (!_didDeadUpdate)
            {
                float height = _gs.md.SampleHeight(_gs, _unit.X, 3000, _unit.Z);
                if (height > 0)
                {
                    transform.position = new Vector3(_unit.X, height, _unit.Z);
                    transform.eulerAngles = new Vector3(0, _unit.Rot, 0);
                    TiltObject();
                    UpdateUnitFrame();
                    _didDeadUpdate = true;
                }
            }

            return;
        }

        canMoveNow = CanMoveNow(_gs);
        if (!canMoveNow || _unit == null)
        {
            return;
        }

        if ((DateTime.UtcNow - LastPosUpdate).TotalSeconds > 10)
        {
            return;
        }

        if (_unit.HasTarget() && !_unit.HasFlag(UnitFlags.IsDead))
        {
            if (_objectManager.GetUnit(_unit.TargetId,out Unit targetUnit))
            {
                UnitUtils.SetTargetPos(_unit, targetUnit,0);

                SetState(_gs, CombatState);
            }
        }
        else
        {
            SetState(_gs, IdleState);
        }

        float dx = _unit.ToX - _unit.X;
        float dz = _unit.ToZ - _unit.Z;
        float dxsize = Math.Abs(dx);
        float dzsize = Math.Abs(dz);

        if (_unit.HasTarget() && _unit.Id.IndexOf(".") < 0)
        {
            if (dxsize < 0.1 && dzsize < 0.1)
            {
                _unit.Moving = false;
            }
        }
        else if (dxsize < 0.1f && dzsize < 0.1f)
        {
            _unit.Moving = false;
        }

        float rotSpeed = UnitConstants.RotSpeedPerFrame*10;
        float oldRot = _unit.Rot;
        UnitUtils.TurnTowardPosition(_unit, _unit.ToX, _unit.ToZ, rotSpeed);

        if (_unit.Moving)
        {
            float speed = Mathf.Max(0.0f, _unit.Speed);

            float dist = Mathf.Sqrt(dx * dx + dz * dz);

            float distThisTick = speed / InputService.Instance.TargetFramerate();

            float pct = 0.01f;

            if (dist <= distThisTick)
            {
                _unit.X = _unit.ToX;
                _unit.Z = _unit.ToZ;
                if (!_unit.HasTarget())
                {
                    _unit.Moving = false;
                }
            }
            else
            {

                pct = distThisTick / dist;

                _unit.X += pct * dx;
                _unit.Z += pct * dz;
            }


            float height = _gs.md.SampleHeight(_gs, _unit.X, 3000, _unit.Z);
            transform.position = new Vector3(_unit.X, height, _unit.Z);
            transform.eulerAngles = new Vector3(0, _unit.Rot, 0);
            TiltObject();
            if (speed > _gs.data.GetGameData<AISettings>().BaseUnitSpeed)
            {
                animationSpeed = RunAnimSpeed;
                SetMoveSpeed(AnimParams.RunSpeed);
            }
            else
            {
                animationSpeed = WalkAnimSpeed;
                SetMoveSpeed(AnimParams.WalkSpeed);
            }
        }
        else
        {
            animationSpeed = 0;
            SetMoveSpeed(AnimParams.StopSpeed);
        }
    }


    public override void Initialize(UnityGameState gs)
    {
        base.Initialize(gs);
        SetState(_gs, IdleState);

        float rotDiff = transform.localEulerAngles.y - transform.eulerAngles.y;
        GameObject renderObject = GameObjectUtils.FindChild(gameObject, AnimUtils.RenderObjectName);
        if (renderObject != null)
        {
            renderObject.transform.localEulerAngles = new Vector3(0, rotDiff, 0);
        }

        animationSpeed = 1.0f;

        StartCoroutine(DelayTilt());
    }


    private IEnumerator DelayTilt()
    {
        yield return new WaitForSeconds(0.7f);
        TiltObject();
    }

    private bool triedToFindInnerPlayer = false;
    private GameObject innerPlayer = null;
    Vector3 oldAngles = Vector3.zero;


    public bool TerrainTilt = false;


    Vector3 lastTiltPos = Vector3.zero;
    Vector3 currTiltPos = Vector3.zero;
    Quaternion currTiltRot = Quaternion.identity;
    Vector3 currTiltNormal = Vector3.zero;
    Quaternion groundTilt = Quaternion.identity;
    float currTiltHeight = 0;

    int objectLayer = 0;


    int ticksSinceLastFlat = 100;
    protected virtual void TiltObject()
    {
        if (!TerrainTilt || gameObject == null)
        { 
            return;
        }
        if (UnityMap == null)
        {
            return;
        }

        currTiltPos = gameObject.transform.position;

        if (Vector3.Distance(lastTiltPos, currTiltPos) < 0.01f)
        {
            return;
        }

        currTiltHeight = _gs.md.SampleHeight(_gs, transform.position.x, 3000, transform.position.z);

        // Don't tilt things underground.
        if (currTiltPos.y < currTiltHeight-2)
        {
            return;
        }

        if (innerPlayer == null)
        {
            if (triedToFindInnerPlayer)
            {
                return;
            }
            triedToFindInnerPlayer = true;
            innerPlayer = GameObjectUtils.FindChild(gameObject, AnimUtils.RenderObjectName);
            if (innerPlayer == null)
            {
                return;
            }
        }

        oldAngles = innerPlayer.transform.eulerAngles;
        if (IsSwimming())
        {
            innerPlayer.transform.eulerAngles = new Vector3(0, oldAngles.y, 0);
            return;
        }

        currTiltRot = gameObject.transform.rotation;
        currTiltNormal = UnityMap.GetInterpolatedNormal(_gs, _gs.map, currTiltPos.x, currTiltPos.z);
        groundTilt = Quaternion.FromToRotation(Vector3.up, currTiltNormal);

        if (objectLayer == 0)
        {
            objectLayer = (1 << LayerMask.NameToLayer(LayerNames.ObjectLayer));
        }

        if (Physics.Raycast(transform.position, Vector3.down, 3, objectLayer) || ++ticksSinceLastFlat < 4)
        {
            groundTilt = Quaternion.identity;
            ticksSinceLastFlat = 0;
        }


        innerPlayer.transform.rotation = groundTilt * currTiltRot;
        lastTiltPos = currTiltPos;
    }

}


