using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Constants;
using Scripts.Assets.Assets.Constants;

public class PlatformBuildData
{
    public string Name;
    public BuildTarget Target;
    public string FilePath;
    public string ClientPlatform;

    public string GetBundleOutputPath()
    {

        return BuildConfiguration.AssetBundleRoot + FilePath;
    }

    public string GetTextFileOutputPath()
    {
        Assembly assemb = Assembly.GetExecutingAssembly();
        string loc = assemb.Location;
        return Path.GetDirectoryName(loc) + "/../../" + GetBundleOutputPath();
    }



}

public class BuildConfiguration
{
    public const string AssetBundleRoot = "AssetBundles/";



    public static List<PlatformBuildData> GetbuildConfigs(UnityGameState gs)
    {
        List<PlatformBuildData> list = new List<PlatformBuildData>();

        bool useOSX = false;
        if (useOSX)
        {
            list.Add(new PlatformBuildData()
            {
                Target = BuildTarget.StandaloneOSX,
                FilePath = PlatformAssetPrefixes.OSX,
                ClientPlatform = ClientPlatformNames.OSX,
            });
        }

        list.Add(new PlatformBuildData()
        {
            Target = BuildTarget.StandaloneWindows,
            FilePath = PlatformAssetPrefixes.Win,
            ClientPlatform = ClientPlatformNames.Win,
        });

        return list;
    }
}
