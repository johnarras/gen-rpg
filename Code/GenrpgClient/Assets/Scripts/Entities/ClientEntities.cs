using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Obj = UnityEngine.Object;
using GEntity = UnityEngine.GameObject;
using GTransform = UnityEngine.Transform;
using UnityEngine.EventSystems;
using UnityEngine; // Needed

public static class ClientEntities
{
    public static GEntity entity (this Component comp)
    {
        return comp.gameObject;
    }   
    
    public static GEntity entity(this Collision coll)
    {
        return coll.gameObject;
    }

    public static GEntity entity(this RaycastResult res)
    {
        return res.gameObject;
    }

    public static GTransform transform (this GEntity entity)
    {
        return entity.transform;
    }

    public static GTransform transform(this Component comp)
    {
        return comp.transform;
    }

    public static GTransform transform(this RaycastHit hit)
    {
        return hit.transform;
    }

    

}
