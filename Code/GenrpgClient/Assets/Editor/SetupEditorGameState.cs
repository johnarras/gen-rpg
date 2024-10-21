using UnityEditor;
using UnityEngine;
using Genrpg.Shared.Setup.Services;
using System.Threading;
using Genrpg.Shared.GameSettings;
using Assets.Scripts.GameSettings.Services;
using System;

using Genrpg.Shared.Interfaces;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Genrpg.Shared.Client.Core;
using Assets.Scripts.Assets;

public class SetupEditorUnityGameState
{
    public static async Awaitable<IClientGameState> Setup(IClientGameState gs = null)
    {

        CancellationTokenSource _cts = new CancellationTokenSource();
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

        if (gs == null) 
        { 
            GameObject initObject = GameObject.Find("InitClient");

            InitClient initClient = initObject.GetComponent<InitClient>();

            gs = initClient.InitialSetup();

            SetupService ss = new SetupService(gs.loc);

            await ss.SetupGame(_cts.Token);

            IClientConfigContainer configContainer = gs.loc.Get<IClientConfigContainer>();
            
            configContainer.Config.ResponseContentRoot = AssetConstants.DefaultDevContentRoot;

            try
            {
                ClientInitializer clientInitializer = new ClientInitializer(gs);
                clientInitializer.AddClientServices(initClient, false, _cts.Token);

                await clientInitializer.FinalInitialize(_cts.Token);
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
