
using Genrpg.Shared.UI.Entities;
using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.UI.Config
{
    [Serializable]
    public class ScreenConfig : ScriptableObject
    {
        public ScreenId ScreenName;
        public ScreenLayers ScreenLayer;
        public string Subdirectory;
#if UNITY_EDITOR
        [MenuItem("Assets/Create/ScriptableObjects/ScreenConfig", false, 0)]
        public static void Create()
        {
            ScriptableObjectUtils.CreateBasicInstance<ScreenConfig>();
        }
#endif
    }
}