using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Genrpg.Shared.Core.Entities;


using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Client.Core;
using Assets.Scripts.Assets.Bundles;

public class SetupBundles
{
	public const int BillboardSize = 128;
	public const int AtlasSize = 512;
	public const int CoreSize = 1024;

    [MenuItem("Tools/Clear Bundles")]
    static void ClearAllAssetBundles()
    {
        ClearBundlesInPath("Assets");
    }

    private static void ClearBundlesInPath(string path)
    {
        path += "/";
        if (!Directory.Exists(path))
        {
            return;
        }

        string[] dirs = Directory.GetDirectories(path);
        string[] files = Directory.GetFiles(path);

        foreach (string file in files)
        {
            AssetImporter importer = AssetImporter.GetAtPath(file) as AssetImporter;

            if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName))
            {
                importer.assetBundleName = "";
                importer.SaveAndReimport();
            }
        }

        foreach (string dir in dirs)
        {
            ClearBundlesInPath(dir);
        }
                
    }


    public static BundleList SetupAll(IClientGameState gs)
    { 
        gs = SetupEditorUnityGameState.Setup(gs).GetAwaiter().GetResult();

        BundleList blist = new BundleList();

        BundleSetupUtils.BundleFilesInDirectory(blist, "", false);

        return blist;
    }
}
