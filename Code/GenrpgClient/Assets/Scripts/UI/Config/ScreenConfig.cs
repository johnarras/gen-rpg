
using System;
using UI.Screens.Constants;
using UnityEditor;
using UnityEngine; // Needed
using GEntity = UnityEngine.GameObject;

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