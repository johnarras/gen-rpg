﻿using Genrpg.Shared.Core.Entities;
using UnityEditor;
using Assets.Scripts.Model;
using Genrpg.Shared.Constants;
using UnityEngine;
using Genrpg.Shared.Setup.Services;
using Cysharp.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.GameDatas;

public class SetupEditorUnityGameState
{
    public static UnityGameState Setup(UnityGameState gs = null)
    {
        CancellationTokenSource _cts = new CancellationTokenSource();
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

        bool needInit = false;
        if (gs == null) { gs = new UnityGameState(); needInit = true; }
        if (gs.logger == null) gs.logger = new ClientLogger(gs);
        if (gs.loc == null) { gs.loc = new ServiceLocator(gs.logger); needInit = true; }
        if (gs.repo == null) { gs.repo = new ClientRepositorySystem(gs.logger); needInit = true; }


        if (needInit)
        {

            GameObject initObject = GameObject.Find("__Init");
            gs.SetInitObject(initObject);
            SetupService ss = new SetupService();

            gs.Env = EnvNames.Dev;
            ss.SetupGame(gs, _cts.Token);
            gs.data = new GameData();

            ClientSetupService.SetupClient(gs, false, _cts.Token);
        }
        return gs;
    }
}