using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using GEntity = UnityEngine.GameObject;
using GComponent = UnityEngine.MonoBehaviour;

public static class GEntityExtensionMethods
{
    public static CancellationToken GetCancellationToken(this GComponent comp)
    {
        return comp.destroyCancellationToken;
    }
}
