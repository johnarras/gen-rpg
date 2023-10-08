using System;
using UnityEngine; // Needed
#if UNITY_EDITOR
using UnityEditor;
#endif
using Genrpg.Shared.Constants;

[Serializable]
public class ClientConfig : ScriptableObject
{
    public EnvEnum Env;
#if UNITY_EDITOR
    [MenuItem("Assets/Create/ScriptableObjects/ClientConfig", false, 0)]
    public static void Create()
    {
        ScriptableObjectUtils.CreateBasicInstance<ClientConfig>();
    }
#endif
}