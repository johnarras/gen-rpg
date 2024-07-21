using System;
using System.Threading;
using Genrpg.Shared.Movement.Messages;
using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.MapServer.Services;

public class PlayerController : UnitController
{

    private ICameraController _cameraController = null;
    protected IMapProvider _mapProvider;
    protected IMapGenData _md;

    public const float SlopeLimit = 60f;
    public const float StepOffset = 1.0f;

    GVector3 lastSendPos = GVector3.zero;

    public float UpDistance = 0.0f;
    protected bool _sendUpdates = false;
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

    public override void Init()
    {
        base.Init();
        animationSpeed = 0.33f;
    }

    DateTime lastServerSendTime = DateTime.UtcNow;

    TimeSpan timeSinceLastUpdateSent;

    public const float TimeBetweenPlayerUpdates = 1/10.0f;
    private const float PlayerUpdateMaxDistance = 0.1f;
    private int _keysDown = 0;
    public override void OnUpdate(CancellationToken token)
    {
           
        _cameraController.BeforeMoveUpdate();

        _unit.X =entity.transform().position.x;
        _unit.Z =entity.transform().position.z;

        if (CanMoveNow())
        {
            base.OnUpdate(token);
        }
        SendPositionUpdate();

        _cameraController.AfterMoveUpdate();
    }

    private bool _everSentPositionUpdate = false;
    private void SendPositionUpdate()
    {
        // Send aentity.transform() update to the server
        if (_sendUpdates && !_md.GeneratingMap &&
            _mapProvider.GetMap() != null &&
            entity == _playerManager.GetEntity())
        {
            float oldRot = _unit.Rot;
            _unit.Rot = entity.transform().eulerAngles.y;
            timeSinceLastUpdateSent = DateTime.UtcNow - lastServerSendTime;
            if (entity == _playerManager.GetEntity())
            {
                GVector3 pos = GVector3.Create(entity.transform().position);
                GVector3 diff = pos - lastSendPos;

                _unit.X = pos.x;
                _unit.Y = pos.y;
                _unit.Z = pos.z;
                int keysDown = GetKeysDown();
                if ((((diff.magnitude >= PlayerUpdateMaxDistance) ||  oldRot != _unit.Rot) &&
                    timeSinceLastUpdateSent.TotalSeconds >= TimeBetweenPlayerUpdates) 
                    || keysDown != _keysDown)
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
                    posMessage.SetX(pos.x+extraDist.x);
                    posMessage.SetY(pos.y+extraDist.y);
                    posMessage.SetZ(pos.z+extraDist.z);
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
}


