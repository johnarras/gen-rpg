using System;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using Genrpg.Shared.Core.Entities;
using System.Threading;
using Assets.Scripts.Model;
using Genrpg.Shared.Constants;
using System.Linq;
using Scripts.Assets.Assets.Constants;
using Genrpg.Shared.Setup.Services;
using System.Runtime.Remoting.Channels;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.GameSettings;
using Assets.Scripts.GameSettings.Entities;
using Genrpg.Shared.Client.Core;
using Assets.Editor;
using Assets.Scripts.Assets;
using UnityEngine.EventSystems;
using System.Drawing;

public class BuildClients
{

    [MenuItem("Tools/BuildSelfContainedClients")]
    static void ExecuteSelfContained()
    {
        BuildAllClients(EnvNames.Local, true);
    }

    [MenuItem("Tools/BuildLocalClients")]
    static void ExecuteLocal()
    {
        BuildAllClients(EnvNames.Local,false);
    }

    [MenuItem("Tools/BuildDevClients")]
    static void ExecuteDev()
    {
        BuildAllClients(EnvNames.Dev,false);
    }

    [MenuItem("Tools/BuildTestClients")]
    static void ExecuteTest()
    {
        BuildAllClients(EnvNames.Test,false);
    }


    private static void BuildAllClients (string env, bool playerContainsAllAssets)
    {
        if (string.IsNullOrEmpty(env))
        {
            Debug.Log("No environment set");
            return;
        }

        IClientGameState gs = SetupEditorUnityGameState.Setup(null).GetAwaiter().GetResult();

        IClientConfigContainer _configContainer = gs.loc.Get<IClientConfigContainer>();

        string oldEnv = _configContainer.Config.Env;
        bool oldPlayerContainsAllAssets = _configContainer.Config.PlayerContainsAllAssets;
        _configContainer.Config.Env = env;
        _configContainer.Config.PlayerContainsAllAssets = playerContainsAllAssets;

        List<BundleVersions> versions = CreateAssetBundle.BuildAssetBundles(gs);

        EditorUtility.SetDirty(_configContainer.Config);
        AssetDatabase.SaveAssets();

        string gamePrefix = Game.Prefix;

        EditorBuildSettingsScene mainScene = EditorBuildSettings.scenes.FirstOrDefault(x => x.path.IndexOf("GameMain") >= 0);

        if (mainScene == null)
        {
            Debug.Log("MainScene: GameMain is missing");
            return;
        }

        List<string> scenePaths = new List<string>();

        scenePaths.Add(mainScene.path);

        List<PlatformBuildData> configs = BuildConfiguration.GetbuildConfigs(gs);

        string[] sceneArray = new string[scenePaths.Count];
        for (int s = 0; s < scenePaths.Count; s++)
        {
            sceneArray[s] = scenePaths[s];
        }
        int oldVersion = 1;
        int version = 1;
        ClientBuildSettings clientSettings = ClientBuildSettings.GetClientVersionFile(env);
        if (clientSettings != null)
        {
            oldVersion = clientSettings.Version;
            clientSettings.Version++;
            version = clientSettings.Version;
        }

        ClientBuildSettings.UpdateVersionFile(clientSettings, env);
        Debug.Log("Version: " + version);
        string lowerPrefix = gamePrefix.ToLower();

        InitClient initClient = GameObject.Find("InitClient").GetComponent<InitClient>();


        lowerPrefix = initClient.GameMode.ToString().ToLower();
        string outputZipFolder = "../../../Build/" + lowerPrefix + "/zips/";
        if (!Directory.Exists(outputZipFolder))
        {
            Directory.CreateDirectory(outputZipFolder);
        }

        ILogService logService = gs.loc.Get<ILogService>();
        IAnalyticsService analyicsService = gs.loc.Get<IAnalyticsService>();
        IClientAppService appService = gs.loc.Get<IClientAppService>();
        ServiceLocator loc = new ServiceLocator(logService, analyicsService, new ClientGameData());
        ClientRepositoryService repoService = new ClientRepositoryService(logService);
        CancellationTokenSource cts = new CancellationTokenSource();
        SetupService ss = new SetupService(gs.loc);
        ss.SetupGame(cts.Token);

        Assembly servicesAssembly = Assembly.GetAssembly(typeof(SetupService));

        //PlayerSettings.productName = lowerPrefix;

        string lowerEnv = env.ToLower();
        foreach (PlatformBuildData config in configs)
        {
            string platformString = config.ClientPlatform.ToString();
            string appsuffix = ClientPlatformNames.GetApplicationSuffix(platformString);
            string outputFilesFolder = "../../../Build/" + lowerPrefix + "/" + platformString + "/" + lowerEnv + "/";
            string outputPath = outputFilesFolder + lowerPrefix + appsuffix;
            string localFolderPath = appService.DataPath + "/" + outputFilesFolder;

            if (!Directory.Exists(outputFilesFolder))
            {
                Directory.CreateDirectory(outputFilesFolder);
            }
            BuildOptions options = BuildOptions.CompressWithLz4HC;

            Directory.Delete(appService.StreamingAssetsPath, true);
            if (!Directory.Exists(appService.StreamingAssetsPath))
            {
                Directory.CreateDirectory(appService.StreamingAssetsPath);
            }

            BundleVersions bundleVersions = versions.FirstOrDefault(x => x.ClientPlatform == config.ClientPlatform);

            string bundleOutputPath = config.GetBundleOutputPath();
            string[] files = Directory.GetFiles(bundleOutputPath);

            foreach (BundleVersion bversion in bundleVersions.Versions.Values)
            {
                string origFilename = bundleOutputPath + "/" + bversion.Name;
                string newFilename = origFilename.Replace(bundleOutputPath, "");
                newFilename = newFilename.Replace("\\", "");

                if (newFilename == AssetConstants.BundleVersionsFile)
                {
                    File.Copy(origFilename, "Assets/Resources/Config/" + newFilename, true);
                }
                else if (playerContainsAllAssets || bversion.IsLocal)
                {
                    File.Copy(origFilename, appService.StreamingAssetsPath + "/" + newFilename, true);
                }
            }

            // Maybe do some stuff with debug symbols here.

            BuildPipeline.BuildPlayer(sceneArray, outputPath, config.Target, options);

        }

        string versionFilePath = outputZipFolder + PatcherUtils.GetPatchVersionFilename();
        File.WriteAllText(versionFilePath, String.Empty);
        File.WriteAllText(versionFilePath, version.ToString());
        string localVersionPath =   appService.DataPath + "/../" + versionFilePath;
        string remoteVersionPath = PatcherUtils.GetPatchClientPrefix(gamePrefix, env, PlatformAssetPrefixes.Win, version) + PatcherUtils.GetPatchVersionFilename();


        FileUploadData vfdata = new FileUploadData();
        vfdata.GamePrefix = gamePrefix;
        vfdata.Env = env;
        vfdata.LocalPath = localVersionPath;
        vfdata.RemotePath = remoteVersionPath;

        FileUploader.UploadFile(vfdata);

        _configContainer.Config.Env = oldEnv;
        _configContainer.Config.PlayerContainsAllAssets = oldPlayerContainsAllAssets;
        AssetDatabase.SaveAssets();


        Debug.Log("Finished building clients version: " + version);

    }

}
