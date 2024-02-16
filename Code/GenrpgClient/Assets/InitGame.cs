using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    public class InitGame
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void OnGameStart()
        {
            GEntityUtils.InstantiateIntoParent(AssetUtils.LoadResource<GameObject>(AssetCategoryNames.Prefabs + "/InitClient"), null);
        }
    }
}
