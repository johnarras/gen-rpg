using System;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Units.Entities;
using System.Threading;
using UnityEngine; // Needed
using Genrpg.Shared.AI.Settings;

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

    int updateTimes = 0;
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
                   entity.transform().position = GVector3.Create(_unit.X, height, _unit.Z);
                   entity.transform().eulerAngles = GVector3.Create(0, _unit.Rot, 0);
                    TiltObject();
                    UpdateUnitFrame();
                    _didDeadUpdate = true;
                }
            }

            return;
        }
        
        if (++updateTimes == 10)
        {
            TiltObject();
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
            float speed = Math.Max(0.0f, _unit.Speed);

            float dist = (float)Math.Sqrt(dx * dx + dz * dz);

            float distThisTick = speed / AppUtils.TargetFrameRate;

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
            entity.transform().position = GVector3.Create(_unit.X, height, _unit.Z);
            entity.transform().eulerAngles = GVector3.Create(0, _unit.Rot, 0);
            TiltObject();
            if (speed > _gs.data.GetGameData<AISettings>(_gs.ch).BaseUnitSpeed)
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

        float rotDiff =entity.transform().localEulerAngles.y -entity.transform().eulerAngles.y;
        GEntity renderObject = GEntityUtils.FindChild(entity, AnimUtils.RenderObjectName);
        if (renderObject != null)
        {
            renderObject.transform().localEulerAngles = GVector3.Create(0, rotDiff, 0);
        }

        animationSpeed = 1.0f;

    }


    private bool triedToFindInnerPlayer = false;
    public GEntity innerPlayer = null;
    GVector3 oldAngles = GVector3.zero;
    GEntity raycastHit;

    public bool TerrainTilt = false;


    GVector3 lastTiltPos = GVector3.zero;
    GVector3 currTiltPos = GVector3.zero;
    Quaternion currTiltRot = GQuaternion.identity;
    GVector3 currTiltNormal = GVector3.zero;
    Quaternion groundTilt = GQuaternion.identity;
    float currTiltHeight = 0;

    int objectLayer = 0;


    int ticksSinceLastFlat = 100;
    protected virtual void TiltObject()
    {
        if (!TerrainTilt || entity == null)
        { 
            return;
        }
        if (UnityMap == null)
        {
            return;
        }

        currTiltPos = GVector3.Create(entity.transform().position);

        if (GVector3.Distance(lastTiltPos, currTiltPos) < 0.01f)
        {
            return;
        }

        currTiltHeight = _gs.md.SampleHeight(_gs,entity.transform().position.x, 3000,entity.transform().position.z);

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
            innerPlayer = GEntityUtils.FindChild(entity, AnimUtils.RenderObjectName);
            if (innerPlayer == null)
            {
                return;
            }
        }

        oldAngles = GVector3.Create(innerPlayer.transform().eulerAngles);
        if (IsSwimming())
        {
            innerPlayer.transform().eulerAngles = GVector3.Create(0, oldAngles.y, 0);
            return;
        }

        currTiltRot = entity.transform().rotation;
        currTiltNormal = UnityMap.GetInterpolatedNormal(_gs, _gs.map, currTiltPos.x, currTiltPos.z);
        groundTilt = GQuaternion.FromToRotation(GVector3.up, currTiltNormal);

        if (objectLayer == 0)
        {
            objectLayer = (1 << LayerUtils.NameToLayer(LayerNames.ObjectLayer));
        }

        if (GPhysics.Raycast(GVector3.Create(entity.transform().position), GVector3.down, out raycastHit, 3, objectLayer) || ++ticksSinceLastFlat < 4)
        {
            groundTilt = GQuaternion.identity;
            ticksSinceLastFlat = 0;
        }

        innerPlayer.transform().rotation = groundTilt * currTiltRot;
        lastTiltPos = currTiltPos;
    }

}


