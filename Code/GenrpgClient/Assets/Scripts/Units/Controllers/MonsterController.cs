using System;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Units.Entities;
using System.Threading;
using UnityEngine; // Needed
using Genrpg.Shared.AI.Settings;
using Genrpg.Shared.Pathfinding.Entities;
using System.Linq;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Units.Constants;

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

    protected virtual float GetMaxRotation() { return 360; }

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
                float height = _terrainManager.SampleHeight(_gs, _unit.X, _unit.Z);
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
            if (_objectManager.GetUnit(_unit.TargetId, out Unit targetUnit))
            {
                _unit.FinalX = targetUnit.X;
                _unit.FinalZ = targetUnit.Z;

                if (_unit.Waypoints != null && _unit.Waypoints.Waypoints.Count > 0)
                {
                    _unit.Waypoints.Waypoints.Last().X = (int)(_unit.FinalX);
                    _unit.Waypoints.Waypoints.Last().Z = (int)(_unit.FinalZ);
                }

                SetState(_gs, CombatState);
            }
        }
        else
        {
            SetState(_gs, IdleState);
        }

        float nextX = _unit.GetNextXPos();
        float nextZ = _unit.GetNextZPos();
        nextX = _unit.FinalX;
        nextZ = _unit.FinalZ;

        if (_unit.HasTarget() && _unit.Waypoints != null && _unit.Waypoints.Waypoints.Count < 3 &&
            _unit.Waypoints.Waypoints.Count > 0)
        {
            if (_objectManager.GetObject(_unit.TargetId, out MapObject mapObject))
            {
                nextX = mapObject.X;
                nextZ = mapObject.Z;
                _unit.Waypoints.Waypoints.Last().X = (int)nextX;
                _unit.Waypoints.Waypoints.Last().Z = (int)nextZ;
            }
        }

        float dx = nextX - _unit.X;
        float dz = nextZ - _unit.Z;
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

        if (_unit.Moving)
        {
            float speed = Math.Max(0.0f, _unit.Speed);

            float dist = (float)Math.Sqrt(dx * dx + dz * dz);

            float distThisTick = speed / AppUtils.TargetFrameRate;

            float pct = 0.01f;

            if (dist <= distThisTick)
            {
                if (_unit.Waypoints != null && _unit.Waypoints.Waypoints.Count > 0)
                {
                    _unit.Waypoints.Waypoints.RemoveAt(0);

                     nextX = _unit.GetNextXPos();
                     nextZ = _unit.GetNextZPos();

                     dx = nextX - _unit.X;
                     dz = nextZ - _unit.Z;
                     dxsize = Math.Abs(dx);
                     dzsize = Math.Abs(dz);
                }
                else
                {
                    _unit.X = _unit.FinalX;
                    _unit.Z = _unit.FinalZ;
                    if (!_unit.HasTarget())
                    {
                        _unit.Moving = false;
                    }
                }
            }
            else
            {

                pct = distThisTick / dist;

                _unit.X += pct * dx;
                _unit.Z += pct * dz;
            }

            UnitUtils.TurnTowardNextPosition(_unit, GetMaxRotation());

            float height = _terrainManager.SampleHeight(_gs, _unit.X, _unit.Z);
            entity.transform().position = GVector3.Create(_unit.X, height, _unit.Z);
            entity.transform().eulerAngles = GVector3.Create(0, _unit.Rot, 0);
            TiltObject();
            if (speed > _gameData.Get<AISettings>(_gs.ch).BaseUnitSpeed)
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

        currTiltHeight = _terrainManager.SampleHeight(_gs,entity.transform().position.x,entity.transform().position.z);

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
        currTiltNormal = _terrainManager.GetInterpolatedNormal(_gs, _gs.map, currTiltPos.x, currTiltPos.z);
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


