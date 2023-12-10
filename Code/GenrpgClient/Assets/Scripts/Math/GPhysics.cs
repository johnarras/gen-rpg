using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using GEntity = UnityEngine.GameObject;
using RayHit = UnityEngine.RaycastHit;
using GRay = UnityEngine.Ray;
using PhysicsImpl = UnityEngine.Physics;
using UnityEngine; // Needed

public class GPhysics
{
    public static bool Raycast(GVector3 origin, GVector3 direction, out GEntity objHit, float camDist, int camCollideLayerMask)
    {
        RayHit outHit;
        objHit = null;
        bool didHit = PhysicsImpl.Raycast(GVector3.Create(origin), GVector3.Create(direction), out outHit, camDist, camCollideLayerMask);
        if (didHit)
        {
            objHit = outHit.transform().entity();
        }
        return didHit;
    }
    
    public static bool Raycast(GRay ray, out RayHit objHit, float maxDistance, int layerMask)
    {
        return PhysicsImpl.Raycast(ray, out objHit, maxDistance, layerMask);
    }

    public static List<GEntity> RaycastAll(GVector3 origin, GVector3 direction)
    {
        RayHit[] hits = PhysicsImpl.RaycastAll(GVector3.Create(origin), GVector3.Create(direction));

        List<GEntity> retval = new List<GEntity>();

        foreach (RaycastHit hit in hits)
        {
            retval.Add(hit.collider.entity());
        }

        return retval;
    }
}
