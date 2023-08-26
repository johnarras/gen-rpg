using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Utils.Data;
using UnityEngine;

public static class ClientExtensionMethods
{
    public static Vector3 ToVector3(this MyPointF pt)
    {
        return new Vector3(pt.X, pt.Y, pt.Z);
    }

}