using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using Genrpg.Shared.Core.Entities;

using Services;


using UnityEngine;
using UnityEditor;
using System.Threading;
using Genrpg.Shared.Constants;
using Entities;
using Entities.Bundles;

public class UploadAssetBundle
{
	[MenuItem("Build/Dev Upload Asset Bundles")]
	static void ExecuteDev()
    {
        UnityGameState gs = SetupEditorUnityGameState.Setup(null);
        UploadAssetBundles(gs,EnvNames.Dev);
	}

    [MenuItem("Build/Prod Upload Asset Bundles")]
    static void ExecuteProd()
    {
        UnityGameState gs = SetupEditorUnityGameState.Setup(null);
        UploadAssetBundles(gs,EnvNames.Prod);
    }
    public static void UploadAssetBundles(UnityGameState gs,string env)
    {
        gs = SetupEditorUnityGameState.Setup(gs);
        InnerUploadFiles(gs,env);
	}


	private static void InnerUploadFiles(UnityGameState gs,string env)
	{

        gs = SetupEditorUnityGameState.Setup(gs);

        List<PlatformBuildData> targets = BuildConfiguration.GetbuildConfigs(gs);

		int numUploads = 0;
		for (int t = 0; t < targets.Count; t++)
		{
            string bundleVersionPath = targets[t].GetTextFileOutputPath() + "/" + UnityAssetService.BundleVersionFilename;
            string versionText = File.ReadAllText(bundleVersionPath);
            Dictionary<string, BundleVersionData> versionData = UnityAssetService.ConvertTextToBundleVersions(gs, versionText);
            foreach (string bname in versionData.Keys)
            {

                string prefix = targets[t].FilePath;

                BundleVersionData bvd = versionData[bname];
                string fullName = bvd.Name + "_" + bvd.Hash;
                string localPath = Application.dataPath + "/../" + BuildConfiguration.AssetBundleRoot + prefix + "/" + bvd.Name;
                string remotePath = Application.version + "/" + targets[t].FilePath + "/" + fullName;
                FileUploadData fdata = new FileUploadData();
                fdata.GamePrefix = Game.Prefix;
                fdata.Env = env;
                fdata.LocalPath = localPath;
                fdata.RemotePath = remotePath;

                FileUploader.UploadFile(fdata);
                ++numUploads;
                if (numUploads % 10 == 0)
                {
                    Debug.Log("Uploaded " + numUploads + "/" + versionData.Keys.Count * targets.Count);
                }
			}

            // Now upload the new bundle list.
            if (versionData.Keys.Count > 0)
            {
                string prefix = targets[t].FilePath;

                string localPath = Application.dataPath + "/../" + BuildConfiguration.AssetBundleRoot + prefix + "/" + UnityAssetService.BundleVersionFilename;
                string remotePath = Application.version + "/" + targets[t].FilePath + "/" + UnityAssetService.BundleVersionFilename;
                FileUploadData fdata = new FileUploadData();
                fdata.GamePrefix = Game.Prefix;
                fdata.Env = env;
                fdata.LocalPath = localPath;
                fdata.RemotePath = remotePath;

               FileUploader.UploadFile(fdata);

            }
		}

		Debug.Log("Num uploads: " + numUploads);
	}

}
