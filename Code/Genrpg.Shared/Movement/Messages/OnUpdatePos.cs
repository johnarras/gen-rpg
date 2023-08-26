using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Movement.Messages
{
    [MessagePackObject]
    public sealed class OnUpdatePos : BaseMapApiMessage
    {
        const int X = 0;
        const int Y = 1;
        const int Z = 2;
        const int Rot = 3;
        const int Speed = 4;
        const int KeysDown = 5;
        const int Max = 6;

        [Key(0)] public string ObjId { get; set; }
        [Key(1)] public ushort[] Dat { get; set; }

        public OnUpdatePos()
        {
            Dat = new ushort[Max];
        }

        const float _posDiv = 6.0f;
        const float _speedDiv = 100.0f;

        public void SetX(float x) { Dat[X] = (ushort)(x*_posDiv); }
        public float GetX() { return Dat[X]/_posDiv; }

        public void SetY(float y) { Dat[Y] = (ushort)(y*_posDiv); }
        public float GetY() { return Dat[Y]/_posDiv; }


        public void SetZ(float z) { Dat[Z] = (ushort)(z*_posDiv); }
        public float GetZ() { return Dat[Z]/_posDiv; }

        public void SetRot(float rot) { Dat[Rot] = (ushort)(MathUtils.Clamp(0,rot,360)*_posDiv); }
        public float GetRot() { return Dat[Rot]/_posDiv; }

        public void SetSpeed(float speed) { Dat[Speed] = (ushort)(speed*_speedDiv); }
        public float GetSpeed() { return Dat[Speed] / _speedDiv; }

        public int GetKeysDown() { return (int)Dat[KeysDown]; }
        public void SetKeysDown(int keysDown) { Dat[KeysDown] = (ushort)keysDown; }
    }
}
