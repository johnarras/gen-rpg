using System;
using UnityEngine; // Needed
using static System.Net.WebRequestMethods;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Genrpg.Shared.Constants;

[Serializable]
public class ClientConfig : ScriptableObject
{
    public string Env = EnvNames.Dev;
    public string ContentDataEnvOverride = EnvNames.Dev;
    public string WorldDataEnv = EnvNames.Dev;
    public string InitialConfigEndpoint = "https://genrpgconfig.azurewebsites.net/api/GenrpgConfig";

#if UNITY_EDITOR
    [MenuItem("Assets/Create/ScriptableObjects/ClientConfig", false, 0)]
    public static void Create()
    {
        ScriptableObjectUtils.CreateBasicInstance<ClientConfig>();
    }
#endif

    public static ClientConfig Load()
    {
        return Resources.Load<ClientConfig>("Config/ClientConfig");
    }
}