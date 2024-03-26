using UnityEditor;
using UnityEngine;
using Genrpg.Shared.Setup.Services;
using System.Threading;
using Genrpg.Shared.GameSettings;
using Assets.Scripts.GameSettings.Services;
using System;

public class SetupEditorUnityGameState
{

    public static UnityGameState Setup(UnityGameState gs = null)
    {

        CancellationTokenSource _cts = new CancellationTokenSource();
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

        bool needInit = false;
        if (gs == null) 
        { 
            gs = new UnityGameState();
            needInit = true; 
        }

        if (needInit)
        {

            GameObject initObject = GameObject.Find("InitClient");
            gs.SetInitObject(initObject);
            SetupService ss = new SetupService();

            ss.SetupGame(gs, _cts.Token);
            gs.data = new GameData();

            ClientConfig config = ClientConfig.Load();

            config.ResponseContentRoot = AssetConstants.DefaultDevContentRoot;

            try
            {
                ClientInitializer clientInitializer = new ClientInitializer();
                clientInitializer.SetupClientServices(gs, false, _cts.Token).Wait();
            }
            catch (Exception e)
            {
                Debug.Log("Exception: " + e.Message + " " + e.StackTrace);
            }
            IClientGameDataService _clientGameDataService = gs.loc.Get<IClientGameDataService>();

            _clientGameDataService.LoadCachedSettings(gs);
        }
        return gs;
    }
}
