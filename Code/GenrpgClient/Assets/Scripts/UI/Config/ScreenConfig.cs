using Genrpg.Shared.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Screens.Constants;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.UI.Config
{
    [Serializable]
    public class ScreenConfig : ScriptableObject
    {
        public ScreenId ScreenName;
        public ScreenLayers ScreenLayer;
#if UNITY_EDITOR
        [MenuItem("Assets/Create/ScriptableObjects/ScreenConfig", false, 0)]
        public static void Create()
        {
            ScriptableObjectUtils.CreateBasicInstance<ScreenConfig>();
        }
#endif
    }
}