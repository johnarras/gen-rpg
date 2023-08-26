using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Constants;

public class ClientBuildSettings : ScriptableObject
{

    public int Version = 0;


    public static string GetVersionAssetPath (string env)
    {
        if (string.IsNullOrEmpty(env))
        {
            env = EnvNames.Test;
        }

        return "Assets/Editor/" + env + "ClientBuildSettings.asset";
    }

    public static void UpdateVersionFile (ClientBuildSettings asset, string env)
    {
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<ClientBuildSettings>();
            asset.Version = 1;
            string path = GetVersionAssetPath(env);
            Debug.Log("Create new asset at: " + path);
            AssetDatabase.CreateAsset(asset, path);
        }
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();

    }

    public static ClientBuildSettings GetClientVersionFile (string env)
    {
        ClientBuildSettings asset = AssetDatabase.LoadAssetAtPath<ClientBuildSettings>(GetVersionAssetPath(env));
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<ClientBuildSettings>();
            AssetDatabase.CreateAsset(asset, GetVersionAssetPath(env));
        }
        return asset;
    }

}
