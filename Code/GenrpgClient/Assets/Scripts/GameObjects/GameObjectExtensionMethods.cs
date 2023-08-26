using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class GameObjectExtensionMethods
{
    public static CancellationToken GetToken(this GameObject gameObject)
    {
        return gameObject.GetCancellationTokenOnDestroy();
    }

}
