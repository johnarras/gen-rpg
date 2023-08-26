using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Movement.Messages
{
    [MessagePackObject]
    public sealed class UpdatePos : BaseMapApiMessage, IPlayerCommand
    {
        const int X = 0;
        const int Y = 1;
        const int Z = 2;
        const int Rot = 3;
        const int Speed = 4;
        const int KeysDown = 5;
        const int Max = 6;

        [Key(0)] public long ZoneId { get; set; }
        [Key(1)] public float[] Dat { get; set; }

        public UpdatePos()
        {
            Dat = new float[Max];
        }

        public void SetX(float x) { Dat[X] = x; }
        public float GetX() { return Dat[X]; }

        public void SetY(float y) { Dat[Y] = y; }
        public float GetY() { return Dat[Y]; }


        public void SetZ(float z) { Dat[Z] = z; }
        public float GetZ() { return Dat[Z]; }

        public void SetRot(float rot) { Dat[Rot] = rot; }
        public float GetRot() { return Dat[Rot]; }

        public void SetSpeed(float speed) { Dat[Speed] = speed; }
        public float GetSpeed() { return Dat[Speed]; }

        public int GetKeysDown() { return (int)Dat[KeysDown]; }
        public void SetKeysDown(int keysDown) { Dat[KeysDown] = keysDown; }

    }
}
