using Genrpg.Shared.Input.PlayerData;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Security.Policy;
using System.Threading;
using UnityEngine;

public class ProxyCharacterController : MonsterController
{

    class PositionData
    {
        public float FinalX;
        public float FinalZ;
        public float FinalRot;
        public float Speed;
        public int KeysDown;
        public bool EverSetData = false;
    }

    private float _rotationLerpTime = 0;
    private DateTime _lastInputSetTime = DateTime.UtcNow;

    PositionData _oldPos = new PositionData();
    PositionData _newPos = new PositionData();

    float _lastNonzeroSpeed = 0;
    float _lastX = 0;
    float _lastZ = 0;
    public override void SetInputValues(int keysDown, float rot)
    {
        base.SetInputValues(keysDown, rot);

        if (_unit.Speed != 0)
        {
            _lastNonzeroSpeed = _unit.Speed;
        }

        _oldPos.FinalX = _newPos.FinalX;
        _oldPos.FinalZ = _newPos.FinalZ;
        _oldPos.FinalRot = _newPos.FinalRot;
        _oldPos.Speed = _newPos.Speed;
        _oldPos.KeysDown = _newPos.KeysDown;

        _newPos.FinalX = _unit.FinalX;
        _newPos.FinalZ = _unit.FinalZ;
        _newPos.FinalRot = _unit.FinalRot;
        _newPos.Speed = _unit.Speed;
        _newPos.KeysDown = keysDown;

        if (_lastX == 0 || _lastZ == 0)
        {
            _lastX = _unit.FinalX;
            _lastZ = _unit.FinalZ;
        }

        _lastInputSetTime = DateTime.UtcNow;
    }
    public override void OnUpdate(CancellationToken token)
    {
        if (_unit == null || !_unit.HasFlag(UnitFlags.ProxyCharacter))
        {
            return;
        }
        if (_rotationLerpTime == 0)
        {
            _rotationLerpTime = ((1.0f * AppUtils.TargetFrameRate * PlayerController.TimeBetweenPlayerUpdates + -1.0f)) / AppUtils.TargetFrameRate;
        }


        float moveSpeed = _unit.Speed != 0 ? _unit.Speed : _lastNonzeroSpeed;

        float dz = 0.0f;
        float dx = 0.0f;
        float sKeyPct = GetKeyPercent(KeyComm.Backward) * BackupSpeedScale;
        float wKeyPct = GetKeyPercent(KeyComm.Forward);

        dx += sKeyPct * moveSpeed;
        dx -= wKeyPct * moveSpeed;

        ShowMoveAnimations(dx, dz);

        float deltaTime = _inputService.GetDeltaTime();

        float totalDX = Math.Abs(_newPos.FinalX - _lastX);
        float totalDZ = Math.Abs(_newPos.FinalZ - _lastZ);

        if (totalDX > 0.05f || totalDZ > 0.05f)
        {
            float speed = _unit.Speed != 0 ? _unit.Speed : _lastNonzeroSpeed;

            if (sKeyPct > 0)
            {
                speed *= BackupSpeedScale;
            }

            float travelDist = Math.Abs(deltaTime * speed);

            float totalDD = MathF.Sqrt(totalDX * totalDX + totalDZ * totalDZ);

            float dxpct = totalDX / totalDD;
            float dzpct = totalDZ / totalDD;

            float travelPercent = MathUtils.Clamp(0, (travelDist / totalDD), 1.0f);

            float ndx = _newPos.FinalX - _lastX;
            float ndz = _newPos.FinalZ - _lastZ;

            float angle = (float)(Math.Atan2(ndx, ndz) * 180.0f/ Math.PI);

            if (wKeyPct != 0)
            {
                entity.transform.eulerAngles = new Vector3(0, angle, 0);
            }
            else if (sKeyPct != 0)
            {
                entity.transform.eulerAngles = new Vector3(0, angle + 180, 0);
            }

            float xpos = _lastX + ndx * travelPercent;
            float zpos = _lastZ + ndz * travelPercent;

            _lastX = xpos;
            _lastZ = zpos;

            float endHeight = _terrainManager.SampleHeight(xpos, zpos);

            entity.transform.position = new Vector3(xpos, endHeight, zpos);
        }
        else
        {
            _lastNonzeroSpeed = 0; float baseLerpPercent = MathUtils.Clamp(0, (float)((DateTime.UtcNow - _lastInputSetTime).TotalSeconds - 0.0f / AppUtils.TargetFrameRate) / _rotationLerpTime, 1);

            while (_newPos.FinalRot - _oldPos.FinalRot > 180)
            {
                _newPos.FinalRot -= 360;
            }
            while (_newPos.FinalRot - _oldPos.FinalRot <= -180)
            {
                _newPos.FinalRot += 360;
            }

            float rot = _oldPos.FinalRot + baseLerpPercent * (_newPos.FinalRot - _oldPos.FinalRot);

          
            entity.transform().localEulerAngles = new Vector3(0, rot, 0);
        }

    }
}
