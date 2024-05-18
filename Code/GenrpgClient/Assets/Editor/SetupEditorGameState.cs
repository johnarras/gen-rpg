using UnityEditor;
using UnityEngine;
using Genrpg.Shared.Setup.Services;
using System.Threading;
using Genrpg.Shared.GameSettings;
using Assets.Scripts.GameSettings.Services;
using System;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Interfaces;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class SetupEditorUnityGameState
{
    public static async UniTask<UnityGameState> Setup(UnityGameState gs = null)
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

            await ss.SetupGame(gs, _cts.Token);

            ClientConfig config = ClientConfig.Load();

            config.ResponseContentRoot = AssetConstants.DefaultDevContentRoot;

            try
            {
                ClientInitializer clientInitializer = new ClientInitializer();
                clientInitializer.AddClientServices(gs, null, false, _cts.Token);

                await clientInitializer.FinalInitialize(gs, _cts.Token);
            }
            catch (Exception e)
            {
                Debug.Log("Exception: " + e.Message + " " + e.StackTrace);
            }
            IClientGameDataService _clientGameDataService = gs.loc.Get<IClientGameDataService>();

            await _clientGameDataService.LoadCachedSettings(gs);
        }
        return gs;
    }
}
