using System;
using System.Linq;
using System.Collections.Generic;
using Genrpg.Shared.Units.Entities;
using System.Threading;
using Genrpg.Shared.Movement.Messages;
using Genrpg.Shared.Targets.Messages;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.Pathfinding.Entities;
using Cysharp.Threading.Tasks.Triggers;
using System.Text;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Pathfinding.Constants;
using Assets.Scripts.Pathfinding.Utils;

public class PlayerController : UnitController
{
    public const float SlopeLimit = 60f;
    public const float StepOffset = 1.0f;

    GVector3 lastSendPos = GVector3.zero;

    public float UpDistance = 0.0f;
    protected bool _sendUpdates = true;
    protected float _lastSendSpeed = 0.0f;
    public float SpeedMult = 1.0f;
    public float RotateMult = 1.0f;

    public void StopUpdates()
    {
        _sendUpdates = false;
    }
    public void StartUpdates()
    {
        _sendUpdates = true;
    }

    public override bool AlwaysShowHealthBar() 
    { 
        return true; 
    }


    public override bool HardStopOnSlopes() { return true; }

    public static PlayerController Instance { get; set; }
    public override void Initialize(UnityGameState gs)
    {
        Instance = this;
        base.Initialize(gs);
        animationSpeed = 0.33f;
    }

    DateTime lastServerSendTime = DateTime.UtcNow;

    TimeSpan span;

    private const float TimeBetweenPlayerUpdates = 0.6f;
    private const float PlayerUpdateMaxDistance = 0.1f;
    private int _keysDown = 0;
    public override void OnUpdate(CancellationToken token)
    {
        if (CameraController.Instance != null)
        {
            CameraController.Instance.BeforeMoveUpdate();
        }

        _unit.X =entity.transform().position.x;
        _unit.Z =entity.transform().position.z;

        if (CanMoveNow(_gs))
        {
            base.OnUpdate(token);
        }
        SendPositionUpdate();


        if (CameraController.Instance != null)
        {
           CameraController.Instance.AfterMoveUpdate();
        }
    }


    private bool _everSentPositionUpdate = false;
    private void SendPositionUpdate()
    {
        // Send aentity.transform() update to the server
        if (_sendUpdates && !_gs.md.GeneratingMap &&
            _gs.map != null && UnityAssetService.LoadSpeed != LoadSpeed.Fast &&
            entity == PlayerObject.Get())
        {
            float oldRot = _unit.Rot;
            _unit.Rot =entity.transform().eulerAngles.y;
            span = DateTime.UtcNow - lastServerSendTime;
            if (entity == PlayerObject.Get())
            {

                GVector3 pos = GVector3.Create(entity.transform().position);

                GVector3 diff = pos - lastSendPos;


                _unit.X = pos.x;
                _unit.Y = pos.y;
                _unit.Z = pos.z;
                int keysDown = GetKeysDown();
                if ((((diff.magnitude >= PlayerUpdateMaxDistance) ||  oldRot != _unit.Rot) &&
                    span.TotalSeconds > TimeBetweenPlayerUpdates/2) 
                    || keysDown != _keysDown ||
                    span.TotalSeconds > TimeBetweenPlayerUpdates)
                {
                    _keysDown = keysDown;
                    float moveSpeed = (GetKeyPercent(KeyComm.Forward) - GetKeyPercent(KeyComm.Backward))*_unit.Speed;
                    moveSpeed = _unit.Speed;


                    _lastSendSpeed = moveSpeed;
                    lastServerSendTime = DateTime.UtcNow;

                    UpdatePos posMessage = new UpdatePos()
                    {
                        ZoneId = _gs.ch.ZoneId,
                    };

                    GVector3 extraDist = GVector3.zero;
                    if (_everSentPositionUpdate)
                    {
                        extraDist = (pos - lastSendPos) * 0.1f;
                    }

                    _unit.Rot =entity.transform().eulerAngles.y;
                    posMessage.SetX((float)Math.Round(pos.x+extraDist.x, 1));
                    posMessage.SetY((float)Math.Round(pos.y+extraDist.y, 1));
                    posMessage.SetZ((float)Math.Round(pos.z+extraDist.z, 1));
                    posMessage.SetRot(_unit.Rot);
                    posMessage.SetKeysDown(_keysDown);
                    posMessage.SetSpeed(moveSpeed);
                    _everSentPositionUpdate = true;

                    _networkService.SendMapMessage(posMessage);
                    lastSendPos = pos;
                }
            }

        }

    }

    private List<Unit> _lastUnitsTabbed = new List<Unit>();
    public void TargetNext(UnityGameState gs)
    {
        int dist = 30;
        int rad = dist + 10;

        if (_unit != null)
        {
            GVector3 newPos = GVector3.Create(entity.transform().position +entity.transform().forward * dist);
            List<Unit> units = _objectManager.GetTypedObjectsNear<Unit>(newPos.x, newPos.z, rad);
            if (units.Count < 1)
            {
                return;
            }

            _lastUnitsTabbed = _lastUnitsTabbed.Where(X => X != null && X != _unit).ToList();

            float minDistToPlayer = 1000000;
            float cx = newPos.x;
            float cz = newPos.z;
            foreach (Unit obj in units)
            {
                if (obj == _unit)
                {
                    continue;
                }
                float dx = obj.X - cx;
                float dz = obj.Z - cz;
                float currDist = (float)Math.Sqrt(dx * dx + dz * dz);
                if (currDist < minDistToPlayer)
                {
                    minDistToPlayer = currDist;
                }
            }


            List<Unit> closeUnits = new List<Unit>();
            foreach (Unit obj in units)
            {

                if (obj == _unit)
                {
                    continue;
                }
                float dx = obj.X - cx;
                float dz = obj.Z - cz;
                float currDist = (float)Math.Sqrt(dx * dx + dz * dz);

                if (currDist < minDistToPlayer + 10)
                {
                    closeUnits.Add(obj);
                }
            }

            List<Unit> nontabbedUnits = new List<Unit>();
            foreach (Unit obj in closeUnits)
            {
                if (!_lastUnitsTabbed.Contains(obj))
                {
                    nontabbedUnits.Add(obj);
                }
            }

            List<Unit> finalUnits = (nontabbedUnits.Count > 0 ? nontabbedUnits : closeUnits);

            if (nontabbedUnits.Count < 1)
            {
                _lastUnitsTabbed.Clear();
            }

            if (finalUnits.Count < 1)
            {
                return;
            }

            finalUnits = finalUnits.Where(x => !x.HasFlag(UnitFlags.IsDead)).ToList();


            if (finalUnits.Count < 1)
            {
                return;
            }

            int unitPos = gs.rand.Next() % finalUnits.Count;

            Unit finalUnit = finalUnits[unitPos];



            if (finalUnit == _unit)
            {
                return;
            }
            SetCurrentTarget(finalUnit);
        }
    }

    public void SetCurrentTarget(Unit finalUnit)
    {

        if (finalUnit != null)
        {
            _lastUnitsTabbed.Add(finalUnit);
            _unit.TargetId = finalUnit.Id;
            //int sx = (int)(gameObject.transform.position.x);
            //int sz = (int)(gameObject.transform.position.z);
            //int ex = (int)(finalUnit.X);
            //int ez = (int)(finalUnit.Z);
            //WaypointList list = _pathfindingService.GetPath(_gs, sx, sz, ex, ez);

            //ClientPathfindingUtils.ShowPath(_gs, list);
        }
        else
        {
            _unit.TargetId = null;
        }

        _networkService.SendMapMessage(new SetTarget() { TargetId = finalUnit.Id });
    }

}


