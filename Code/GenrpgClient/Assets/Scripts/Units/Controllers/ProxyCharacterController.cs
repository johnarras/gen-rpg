using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.Units.Entities;
using System;
using System.Threading;

public class ProxyCharacterController : UnitController
{
    protected string _lastMoveKey = null;   
    public override void OnUpdate(CancellationToken token)
    {
        if (_unit == null || !_unit.HasFlag(UnitFlags.ProxyCharacter))
        {
            return;
        }

        float delta = _inputService.GetDeltaTime();
        float targetDeltaTime = 1.0f / AppUtils.TargetFrameRate;
        delta = targetDeltaTime;
        float moveSpeed = _unit.Speed * delta;

        float dz = 0.0f;
        float dx = 0.0f;
        float sKeyPct = GetKeyPercent(KeyComm.Backward)*BackupSpeedScale;
        float wKeyPct = GetKeyPercent(KeyComm.Forward);

        dx += sKeyPct * moveSpeed;
        dx -= wKeyPct * moveSpeed;
        if (wKeyPct > 0)
        {
            _lastMoveKey = KeyComm.Forward;
        }
        else if (sKeyPct > 0)
        {
            _lastMoveKey = KeyComm.Backward;
        }

        GVector3 startPos = GVector3.Create(entity.transform().position);
        float ddx = _unit.FinalX - startPos.x;
        float ddz = _unit.FinalZ - startPos.z;
        float totalDist = (float)Math.Sqrt(ddx * ddx + ddz * ddz);

        float minDistToMove = 0.5f;

        bool didErrorCorrectionMove = false;
        if (dx == 0 && totalDist >= minDistToMove)
        {
           entity.transform().localEulerAngles = GVector3.Create(0, _unit.Rot, 0);
            if (_lastMoveKey == KeyComm.Forward)
            {
                UnitUtils.TurnTowardNextPosition(_unit, 10);
               entity.transform().localEulerAngles = GVector3.Create(0, _unit.Rot, 0);
                dx = -moveSpeed;
                didErrorCorrectionMove = true;
            }
            else if (_lastMoveKey == KeyComm.Backward)
            {
                UnitUtils.TurnTowardNextPosition(_unit, 10);
               entity.transform().localEulerAngles = GVector3.Create(0, _unit.Rot, 0);
                didErrorCorrectionMove = true;
            }
        }
        else
        {
           entity.transform().localEulerAngles = GVector3.Create(0, _unit.Rot, 0);
        }

        ShowMoveAnimations(dx, dz);

        if (dx != 0 || dz != 0)
        {
            float nrot = entity.transform().localEulerAngles.y;
            double sin = Math.Sin(nrot * Math.PI / 180.0);
            double cos = Math.Cos(nrot * Math.PI / 180.0);

            float nz = (float)-(dx * cos + dz * sin);
            float nx = (float)(dz * cos - dx * sin);

            float ny = 0.0f;

            float mx = startPos.x + nx;
            float my = startPos.y + ny;
            float mz = startPos.z + nz;

            GVector3 endPos = new GVector3 (mx, my, mz);

            float edx = endPos.x - _unit.FinalX;
            float edz = endPos.z - _unit.FinalZ;

            float endDist = (float)Math.Sqrt(edx * edx + edz * edz);
            if (endDist >= totalDist && didErrorCorrectionMove)
            {
                _lastMoveKey = null;
            }
            _unit.X = endPos.x;
            _unit.Z = endPos.z;

            float endHeight = _terrainManager.SampleHeight(_gs, endPos.x, endPos.z);
           entity.transform().position = GVector3.Create(endPos.x, endHeight, endPos.z);

        }
    }
}
