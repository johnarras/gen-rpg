using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Genrpg.Shared.Constants;

public class CreateAssetBundle
{
	[MenuItem("Build/Build Asset Bundles")]
	static void Execute()
	{
        UnityGameState gs = SetupEditorUnityGameState.Setup(null);
		BuildAssetBundles(gs);
	}

	public static void BuildAssetBundles(UnityGameState gs)
    {
        gs = SetupEditorUnityGameState.Setup(gs);

        Debug.Log("Game Target: " + Game.Prefix);

        List<PlatformBuildData> targets = BuildConfiguration.GetbuildConfigs(gs);

        for (int t = 0; t < targets.Count; t++)
        {
            // Get base path to bundle output
            string basePath = targets[t].GetBundleOutputPath();

            string textFilePath = targets[t].GetTextFileOutputPath();
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            DirectoryInfo di = new DirectoryInfo(basePath);
           
            // Load bundle version data in if possible
            Dictionary<string, BundleVersionData> versionDict = new Dictionary<string, BundleVersionData>();

            BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression;

            AssetBundleManifest manifest = null;
            try
            {
                manifest = BuildPipeline.BuildAssetBundles(basePath,
                    options,targets[t].Target);
            }
            catch (Exception e)
            {
                Debug.Log("Asset bundle exception: " + e.Message + "\n" + e.StackTrace);
            }

            // if the manifest exists, loop through all bundles and any that have 
            // changes, increment their versions.
            if (manifest != null)
            {
                string[] bundles = manifest.GetAllAssetBundles();
                foreach (string bundle in bundles)
                {
                    string hash = manifest.GetAssetBundleHash(bundle).ToString();
                    string bundle2 = bundle.Replace(hash, "").Replace("_","");
                
                    if (!versionDict.ContainsKey(bundle2))
                    {
                        versionDict[bundle2] = new BundleVersionData() { Name = bundle2 };
                    }

                    BundleVersionData bvd = versionDict[bundle2];
                    bvd.Hash = hash;

                    try
                    {
                        byte[] bytes = File.ReadAllBytes(basePath + "/" + bundle);
                        //Debug.LogError("FileSize: " + bytes.Length);
                        bvd.Size = bytes.Length;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Failed to load file: " + basePath + "/" + bundle + " " + e.Message);
                    }
                }
            }

            // Save the versions file into the correct folder now.
            string newText = UnityAssetService.ConvertBundleVersionsToText(gs, versionDict);


            // Get path to the bundle version list for this platform
            string bundleVersionPath = textFilePath + "/" + UnityAssetService.BundleVersionFilename;
            try
            {
                File.WriteAllText(bundleVersionPath, newText);
            }
            catch (Exception e)
            {
                Debug.Log("Failed to write bundle version: " + e.Message);
            }

        }


        Debug.Log("Asset bundles built");
	}

}
