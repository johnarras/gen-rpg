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

public class BuildClients
{

    [MenuItem("Build/BuildLocalClients")]
    static void ExecuteLocal()
    {
        BuildAllClients(EnvNames.Local);
    }

    [MenuItem("Build/BuildDevClients")]
    static void ExecuteDev()
    {
        BuildAllClients(EnvNames.Dev);
    }

    [MenuItem("Build/BuildTestClients")]
    static void ExecuteTest()
    {
        BuildAllClients(EnvNames.Test);
    }


    private static void BuildAllClients (string env)
    {
        if (string.IsNullOrEmpty(env))
        {
            Debug.Log("No environment set");
            return;
        }

        UnityGameState gs = SetupEditorUnityGameState.Setup(null);

        bool didSetEnv = false;
        ClientConfig clientConfig = AssetDatabase.LoadAssetAtPath<ClientConfig>("Assets/Resources/Config/ClientConfig.asset");

        if (clientConfig == null)
        {
            Debug.Log("Missing ClientConfig at Assets/Resources/Config/ClientConfig.asset");
            return;
        }
        string oldEnv = clientConfig.Env;
        clientConfig.Env = env;
        didSetEnv = true;

        if (!didSetEnv)
        {
            Debug.Log("Invalid Env: " + env);
            return;
        }

        EditorUtility.SetDirty(clientConfig);
        AssetDatabase.SaveAssets();

        string gamePrefix = "Game";

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
        string outputZipFolder = "../../../Build/" + lowerPrefix + "/zips/";
        if (!Directory.Exists(outputZipFolder))
        {
            Directory.CreateDirectory(outputZipFolder);
        }

        ServiceLocator loc = new ServiceLocator(gs.logger);
        ClientRepositorySystem repofact = new ClientRepositorySystem(gs.logger);
        SetupService ss = new SetupService();
        CancellationTokenSource cts = new CancellationTokenSource();
        ss.SetupGame(gs, cts.Token);

        Assembly servicesAssembly = Assembly.GetAssembly(typeof(SetupService));

        string lowerEnv = env.ToLower();
        foreach (PlatformBuildData config in configs)
        {
            string platformString = config.ClientPlatform.ToString();
            string appsuffix = ClientPlatformNames.GetApplicationSuffix(platformString);
            string outputFilesFolder = "../../../Build/" + lowerPrefix + "/" + platformString + "/" + lowerEnv + "/";
            string outputPath = outputFilesFolder + lowerPrefix + appsuffix;
            string localFolderPath = AppUtils.DataPath + "/" + outputFilesFolder;

            if (!Directory.Exists(outputFilesFolder))
            {
                Directory.CreateDirectory(outputFilesFolder);
            }
            BuildPipeline.BuildPlayer(sceneArray, outputPath, config.Target, BuildOptions.AllowDebugging | BuildOptions.CompressWithLz4HC);

        }

        string versionFilePath = outputZipFolder + PatcherUtils.GetPatchVersionFilename();
        File.WriteAllText(versionFilePath, String.Empty);
        File.WriteAllText(versionFilePath, version.ToString());
        string localVersionPath = AppUtils.DataPath + "/../" + versionFilePath;
        string remoteVersionPath = PatcherUtils.GetPatchClientPrefix(gamePrefix, env, PlatformAssetPrefixes.Win, version) + PatcherUtils.GetPatchVersionFilename();


        FileUploadData vfdata = new FileUploadData();
        vfdata.GamePrefix = gamePrefix;
        vfdata.Env = env;
        vfdata.LocalPath = localVersionPath;
        vfdata.RemotePath = remoteVersionPath;

        FileUploader.UploadFile(vfdata);
        clientConfig = AssetDatabase.LoadAssetAtPath<ClientConfig>("Assets/Resources/Config/ClientConfig.asset");
        clientConfig.Env = oldEnv;
        EditorUtility.SetDirty(clientConfig);
        AssetDatabase.SaveAssets();

        Debug.Log("Finished building clients version: " + version);

    }
}
