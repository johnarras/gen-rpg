﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Client.Core;
using System.Linq;

public class CreateAssetBundle
{
	[MenuItem("Tools/Build Asset Bundles")]
	static void Execute()
	{
        IClientGameState gs = SetupEditorUnityGameState.Setup(null).GetAwaiter().GetResult();
		BuildAssetBundles(gs);
	}

	public static void BuildAssetBundles(IClientGameState gs)
    {
        gs = SetupEditorUnityGameState.Setup(gs).GetAwaiter().GetResult();

        Debug.Log("Game Target: " + Game.Prefix);

        IClientAppService clientAppService = gs.loc.Get<IClientAppService>();

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

            BundleUpdateInfo updateData = new BundleUpdateInfo() { ClientVersion = clientAppService.Version, UpdateTime = DateTime.UtcNow };

            BundleVersions versionData = new BundleVersions() { UpdateInfo = updateData };
           
            BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.RecurseDependencies;

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
                

                    if (!versionData.Versions.ContainsKey(bundle2))
                    {
                        List<string> dependencies = manifest.GetAllDependencies(bundle2).ToList();

                        versionData.Versions[bundle2] = new BundleVersion() { Name = bundle2, ChildDependencies = dependencies };
                    }

                    BundleVersion bvd = versionData.Versions[bundle2];
                    bvd.Hash = hash;

                }
            }

            // Save the versions and last bundle upload time
            string bundleVersionText = SerializationUtils.PrettyPrint(versionData);
            string bundleVersionPath = textFilePath + "/" + AssetConstants.BundleVersionsFile;

            string bundleUploadTimeText = SerializationUtils.Serialize(updateData);
            string bundleUploadTimePath = textFilePath + "/" + AssetConstants.BundleUpdateFile;

            try
            {
                File.WriteAllText(bundleVersionPath, bundleVersionText);
                File.WriteAllText(bundleUploadTimePath, bundleUploadTimeText);
            }
            catch (Exception e)
            {
                Debug.Log("Failed to write bundle version: " + e.Message);
            }

        }


        Debug.Log("Asset bundles built");
	}

}
