using System.Reflection;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using Genrpg.Shared.Constants;
using Scripts.Assets.Assets.Constants;
using Genrpg.Shared.Client.Core;
using Assets.Editor;

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



    public static List<PlatformBuildData> GetbuildConfigs(IClientGameState gs)
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

        //list.Add(new PlatformBuildData()
        //{
        //    Target = BuildTarget.Android,
        //    FilePath = PlatformAssetPrefixes.Android,
        //    ClientPlatform = ClientPlatformNames.Android,
        //});

        return list;
    }
}
