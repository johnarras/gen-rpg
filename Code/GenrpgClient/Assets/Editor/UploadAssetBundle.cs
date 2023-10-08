using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Genrpg.Shared.Constants;

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
                string localPath = AppUtils.DataPath + "/../" + BuildConfiguration.AssetBundleRoot + prefix + "/" + bvd.Name;
                string remotePath = AppUtils.Version + "/" + targets[t].FilePath + "/" + fullName;
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

                string localPath = AppUtils.DataPath + "/../" + BuildConfiguration.AssetBundleRoot + prefix + "/" + UnityAssetService.BundleVersionFilename;
                string remotePath = AppUtils.Version + "/" + targets[t].FilePath + "/" + UnityAssetService.BundleVersionFilename;
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
