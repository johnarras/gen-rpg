using System;
using UnityEngine; // Needed
using static System.Net.WebRequestMethods;
using Genrpg.Shared.Configs.Interfaces;
using System.Collections.Generic;
using Genrpg.Shared.Interfaces;
using Assets.Scripts.Assets;




#if UNITY_EDITOR
using UnityEditor;
#endif
using Genrpg.Shared.Constants;


public interface IClientConfigContainer : IInjectable
{
   ClientConfig Config { get; set; }
}

public class ClientConfigContainer : IClientConfigContainer
{
    public ClientConfig Config { get; set; }
}

[Serializable]
public class ClientConfig : ScriptableObject, IConnectionConfig
{
    public string Env = EnvNames.Dev;
    public string AssetDataEnv = EnvNames.Dev;
    public string WorldDataEnv = EnvNames.Dev;
    public string InitialConfigEndpoint = "https://genrpgconfig.azurewebsites.net/api/GenrpgConfig";
    public int AccountProductId = 2;
    public bool PlayerContainsAllAssets = false;

    public string ResponseContentRoot { get; set; }
    public string ResponseAssetEnv { get; set; }

    public string GetContentRoot()
    {
        if (!string.IsNullOrWhiteSpace(ResponseContentRoot))
        {
            return ResponseContentRoot;
        }
        return AssetConstants.DefaultDevContentRoot;
    }

    public string GetWorldDataEnv()
    {
        return WorldDataEnv;
    }

    public string GetAssetDataEnv()
    {
        if (!string.IsNullOrWhiteSpace(AssetDataEnv))
        {
            return AssetDataEnv;
        }
        return ResponseAssetEnv;
    }

#if UNITY_EDITOR
    [MenuItem("Assets/Create/ScriptableObjects/ClientConfig", false, 0)]
    public static void Create()
    {
        ScriptableObjectUtils.CreateBasicInstance<ClientConfig>();
    }
#endif

    public string GetConnectionString(string key)
    {
        return "";
    }

    public Dictionary<string, string> GetConnectionStrings()
    {
        return new Dictionary<string, string>();
    }
}