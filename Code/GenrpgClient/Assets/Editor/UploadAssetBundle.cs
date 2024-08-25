using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Logging.Interfaces;

public class UploadAssetBundle
{
	[MenuItem("Build/Dev Upload Asset Bundles")]
	static void ExecuteDev()
    {
        IUnityGameState gs = SetupEditorUnityGameState.Setup(null).GetAwaiter().GetResult();
        UploadAssetBundles(EnvNames.Dev);
	}

    [MenuItem("Build/Prod Upload Asset Bundles")]
    static void ExecuteProd()
    {
        IUnityGameState gs = SetupEditorUnityGameState.Setup(null).GetAwaiter().GetResult();
        UploadAssetBundles(EnvNames.Prod);
    }
    public static void UploadAssetBundles(string env)
    {
        IUnityGameState gs = SetupEditorUnityGameState.Setup().GetAwaiter().GetResult();
        InnerUploadFiles(gs, env);
	}


	private static void InnerUploadFiles(IUnityGameState gs, string env)
	{

        BinaryFileRepository localRepo = new BinaryFileRepository(gs.loc.Get<ILogService>());
        gs = SetupEditorUnityGameState.Setup(gs).GetAwaiter().GetResult();

        List<PlatformBuildData> targets = BuildConfiguration.GetbuildConfigs(gs);

		int uploadCount = 0;
		for (int t = 0; t < targets.Count; t++)
		{
            string bundleVersionPath = targets[t].GetTextFileOutputPath() + "/" + AssetConstants.BundleVersionsFile;

            BundleVersions currentData = localRepo.LoadObject<BundleVersions>(bundleVersionPath);
            foreach (string bname in currentData.Versions.Keys)
            {
                string prefix = targets[t].FilePath;

                BundleVersion bvd = currentData.Versions[bname];
                string fullName = bvd.Name + "_" + bvd.Hash;
                string localPath = AppUtils.DataPath + "/../" + BuildConfiguration.AssetBundleRoot + prefix + "/" + bvd.Name;
                string remotePath = AppUtils.Version + "/" + targets[t].FilePath + "/" + fullName;
                FileUploadData fdata = new FileUploadData();
                fdata.GamePrefix = Game.Prefix;
                fdata.Env = env;
                fdata.LocalPath = localPath;
                fdata.RemotePath = remotePath;
                if (uploadCount % 20 == 10)
                {
                    fdata.WaitForComplete = true;
                }
                FileUploader.UploadFile(fdata);
                ++uploadCount;
                if (uploadCount % 10 == 0)
                {
                    Debug.Log("Uploaded " + uploadCount + "/" + currentData.Versions.Keys.Count * targets.Count);
                }
			}

            // Now upload the new bundle list.
            if (currentData.Versions.Keys.Count > 0)
            {
                string prefix = targets[t].FilePath;

                // Upload bundle version and create time file

                List<string> filesToUpload = new List<string>() { AssetConstants.BundleUpdateFile, AssetConstants.BundleVersionsFile };

                foreach (string fileName in filesToUpload)
                {
                    string localPath = AppUtils.DataPath + "/../" + BuildConfiguration.AssetBundleRoot + prefix + "/" + fileName;
                    string remotePath = AppUtils.Version + "/" + targets[t].FilePath + "/" + fileName;
                    FileUploadData fdata = new FileUploadData();
                    fdata.GamePrefix = Game.Prefix;
                    fdata.Env = env;
                    fdata.LocalPath = localPath;
                    fdata.RemotePath = remotePath;

                    FileUploader.UploadFile(fdata);
                }
            }
		}

		Debug.Log("Upload Count: " + uploadCount);
	}

}
