using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GEntity = UnityEngine.GameObject;

using Quat = UnityEngine.Quaternion;

public static class GQuaternion
{
    public static GVector3 MultVector(Quat q, GVector3 vect)
    {
        return GVector3.Create(q * GVector3.Create(vect));
    }

    public static Quat FromToRotation (GVector3 from, GVector3 to)
    {
        return Quat.FromToRotation(GVector3.Create(from),GVector3.Create(to));
    }

    public static Quat identity { get { return Quat.identity; } }
}



