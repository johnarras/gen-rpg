using Genrpg.Shared.Input.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Threading;
using UnityEngine;

public class ProxyCharacterController : UnitController
{
    protected string _lastMoveKey = null;   
    public override void OnUpdate(CancellationToken token)
    {
        if (_unit == null || !_unit.HasFlag(UnitFlags.ProxyCharacter))
        {
            return;
        }

        float delta = InputService.Instance.GetDeltaTime();
        float targetDeltaTime = 1.0f / InputService.Instance.TargetFramerate();
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

        Vector3 startPos = transform.position;
        float ddx = _unit.ToX - startPos.x;
        float ddz = _unit.ToZ - startPos.z;
        float totalDist = Mathf.Sqrt(ddx * ddx + ddz * ddz);

        float minDistToMove = 0.5f;

        bool didErrorCorrectionMove = false;
        if (dx == 0 && totalDist >= minDistToMove)
        {
            transform.localEulerAngles = new Vector3(0, _unit.Rot, 0);
            if (_lastMoveKey == KeyComm.Forward)
            {
                UnitUtils.TurnTowardPosition(_unit, _unit.ToX, _unit.ToZ, 10);
                transform.localEulerAngles = new Vector3(0, _unit.Rot, 0);
                dx = -moveSpeed;
                didErrorCorrectionMove = true;
            }
            else if (_lastMoveKey == KeyComm.Backward)
            {
                UnitUtils.TurnTowardPosition(_unit, startPos.x - ddx, startPos.z - ddz, 10);
                transform.localEulerAngles = new Vector3(0, _unit.Rot, 0);
                didErrorCorrectionMove = true;
            }
        }
        else
        {
            transform.localEulerAngles = new Vector3(0, _unit.Rot, 0);
        }

        ShowMoveAnimations(dx, dz);

        if (dx != 0 || dz != 0)
        {
            float nrot = gameObject.transform.localEulerAngles.y;
            double sin = Math.Sin(nrot * Math.PI / 180.0);
            double cos = Math.Cos(nrot * Math.PI / 180.0);

            float nz = (float)-(dx * cos + dz * sin);
            float nx = (float)(dz * cos - dx * sin);

            float ny = 0.0f;

            float mx = startPos.x + nx;
            float my = startPos.y + ny;
            float mz = startPos.z + nz;

            Vector3 endPos = new Vector3(mx, my, mz);

            float edx = endPos.x - _unit.ToX;
            float edz = endPos.z - _unit.ToZ;

            float endDist = Mathf.Sqrt(edx * edx + edz * edz);
            if (endDist >= totalDist && didErrorCorrectionMove)
            {
                _lastMoveKey = null;
            }
            _unit.X = endPos.x;
            _unit.Z = endPos.z;

            float endHeight = _gs.md.SampleHeight(_gs, endPos.x, 2000, endPos.z);
            transform.position = new Vector3(endPos.x, endHeight, endPos.z);

        }
    }
}
