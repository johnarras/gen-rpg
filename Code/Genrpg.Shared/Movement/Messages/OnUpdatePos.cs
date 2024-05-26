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
        [Key(1)] public string TargetId { get; set; }
        [Key(2)] public float[] Dat { get; set; }

        public OnUpdatePos()
        {
            Dat = new float[Max];
        }

        public void SetX(float x) { Dat[X] = x; }
        public float GetX() { return Dat[X]; }

        public void SetY(float y) { Dat[Y] = y; }
        public float GetY() { return Dat[Y]; }


        public void SetZ(float z) { Dat[Z] = z; }
        public float GetZ() { return Dat[Z]; }

        public void SetRot(float rot) { Dat[Rot] = MathUtils.Clamp(0,rot,360); }
        public float GetRot() { return Dat[Rot]; }

        public void SetSpeed(float speed) { Dat[Speed] = speed; }
        public float GetSpeed() { return Dat[Speed]; }

        public int GetKeysDown() { return (int)Dat[KeysDown]; }
        public void SetKeysDown(int keysDown) { Dat[KeysDown] = (ushort)keysDown; }
    }
}
